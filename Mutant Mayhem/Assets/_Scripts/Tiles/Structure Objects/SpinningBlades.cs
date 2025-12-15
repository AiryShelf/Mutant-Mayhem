using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SpinningBlades : MonoBehaviour, ITileObject, ITileObjectExplodable
{
    [SerializeField] List<Sprite> damagedSprites;
    [SerializeField] float rotationForce = 10000f;
    [SerializeField] float maxAngularVelocity = 1000f;
    [SerializeField] float minAngularVelocityToDamage = 100f;
    [SerializeField] float damagePerAngularVel = 0.1f;
    [SerializeField] float damageInterval = 0.5f;
    [SerializeField] float selfDamagePerAngularVel = 0.01f;
    [SerializeField] float flyingEnemyKnockbackPerAngularVel = 0.04f;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Light2D postLight;

    [SerializeField] bool hasPower = true;

    [Header("Shard Explosion")]
    [SerializeField] bool enableShardExplosion = true;
    [SerializeField] string shardBulletPoolName = "BulletSpinningBlade_Shard";
    [SerializeField, Min(1)] int shardCount = 6;
    [SerializeField] float minAngularVelForShards = 250f;
    [SerializeField] float shardMinSpeed = 8f;
    [SerializeField] float shardMaxSpeed = 18f;
    [SerializeField] float shardMinDamage = 5f;
    [SerializeField] float shardMaxDamage = 20f;
    [SerializeField] float shardLifeTime = 1.2f;
    [SerializeField] float shardKnockback = 0.5f;
    [SerializeField, Range(0f, 360f)] float shardStartAngleOffset = 0f;
    [SerializeField, Range(0f, 30f)] float shardAngleJitter = 0f;


    List<Health> tempHits = new List<Health>();
    float previousAngularVelocity = 0f;

    public string explosionPoolName;

    void Start()
    {
        Daylight.OnSunrise += LightsOff;
        Daylight.OnSunset += LightsOn;
    }

    void OnDestroy()
    {
        Daylight.OnSunrise -= LightsOff;
        Daylight.OnSunset -= LightsOn;
    }

    void LightsOn()
    {
        postLight.enabled = true;
    }

    void LightsOff()
    {
        postLight.enabled = false;
    }

    public void Explode()
    {
        TrySpawnShardExplosion(previousAngularVelocity);

        if (!string.IsNullOrEmpty(explosionPoolName))
        {
            GameObject explosion = PoolManager.Instance.GetFromPool(explosionPoolName);
            Vector3Int rootPos = TileManager.Instance.WorldToGrid(transform.position);
            explosion.transform.position = TileManager.Instance.TileCellsCenterToWorld(rootPos);
        }
    }

    public void UpdateHealthRatio(float healthRatio)
    {
        int damageIndex = GetDamageIndex(healthRatio);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (damageIndex > 0 && damagedSprites.Count >= damageIndex)
            sr.sprite = damagedSprites[damageIndex - 1];
        else
            sr.sprite = damagedSprites[0];
    }

    int GetDamageIndex(float healthRatio)
    {
        int damageIndex;
        int spriteCount = damagedSprites.Count;

        // Reserve index 0 for full health
        if (healthRatio >= 1f)
        {
            damageIndex = 0;
        }
        else
        {
            // Divide remaining indices (1 to spriteCount-1) across the 0–99% damage range
            float normalized = 1f - healthRatio;
            damageIndex = 1 + Mathf.FloorToInt(normalized * (spriteCount - 1));
            damageIndex = Mathf.Clamp(damageIndex, 1, spriteCount - 1);
        }

        return damageIndex;
    }

    public void PowerOn()
    {
        hasPower = true;
        LightsOn();
    }

    public void PowerOff()
    {
        hasPower = false;
        LightsOff();
    }

    void FixedUpdate()
    {
        // Apply torque and limit angular velocity
        if (hasPower && Mathf.Abs(rb.angularVelocity) < maxAngularVelocity)
            rb.AddTorque(rotationForce * Time.fixedDeltaTime);

        previousAngularVelocity = rb.angularVelocity;
    }

    void ApplyDamage(Collider2D otherCollider)
    {
        Health health = otherCollider.GetComponentInParent<Health>();
        if (health != null)
        {
            if (tempHits.Contains(health))
                return;

            // Target damage  
            float damageToApply = damagePerAngularVel * Mathf.Abs(previousAngularVelocity);
            // Get ratio of damage from min to max angular velocity for TextFly scaling
            float damageScale = 1 + Mathf.Clamp01(Mathf.Abs(previousAngularVelocity) / maxAngularVelocity);
            Vector2 hitDir = otherCollider.transform.position - transform.position;
            health.ModifyHealth(-damageToApply, damageScale, hitDir, otherCollider.gameObject);

            // Self damage
            float selfDamage = selfDamagePerAngularVel * Mathf.Abs(previousAngularVelocity);
            TileManager.Instance.ModifyHealthAt(transform.position, -selfDamage, 1f, -hitDir);

            // Add to temp list to avoid multiple damage instances in short time
            tempHits.Add(health);
            StartCoroutine(RemoveColliderFromTempList(health, damageInterval));
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (Mathf.Abs(previousAngularVelocity) < minAngularVelocityToDamage)
            return;

        // Damage health on collision
        Collider2D otherCollider = collision.collider;
        if (otherCollider.CompareTag("Enemy") || otherCollider.CompareTag("Player"))
        {
            ApplyDamage(otherCollider);
        }
        // Handle pickup collisions
        /*
        else if (otherCollider.gameObject.layer == LayerMask.NameToLayer("Pickups"))
        {
            PoolManager.Instance.ReturnToPool("Pickup", otherCollider.gameObject);
            return; 
        }
        */

        //previousAngularVelocity = rb.angularVelocity;
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        // Handle enemy melee collider collisions
        if (collider.gameObject.layer == LayerMask.NameToLayer("EnemyMeleeTrigger"))
        {
            EnemyMutant mutant = collider.GetComponentInParent<EnemyMutant>();
            if (mutant != null && mutant.individual.genome.legGene.isFlying)
            {
                Rigidbody2D mutantRb = mutant.GetComponent<Rigidbody2D>();
                // Calculate knockback based on angular velocity
                float knockbackForce = flyingEnemyKnockbackPerAngularVel * Mathf.Abs(previousAngularVelocity);
                Vector2 knockbackDir = (mutant.transform.position - transform.position).normalized;
                mutantRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);

                ApplyDamage(collider);
                return;
            }
            EnemyBase enemy = collider.GetComponentInParent<EnemyBase>();
            if (enemy != null && enemy.gameObject.layer == LayerMask.NameToLayer("FlyingEnemies"))
            {
                Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                // Calculate knockback based on angular velocity
                float knockbackForce = flyingEnemyKnockbackPerAngularVel * Mathf.Abs(previousAngularVelocity);
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                enemyRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
            }
            


            ApplyDamage(collider);
        }
    }

    void TrySpawnShardExplosion(float angularVel)
    {
        if (!enableShardExplosion)
            return;

        float absAngVel = Mathf.Abs(angularVel);
        if (absAngVel < minAngularVelForShards)
            return;

        if (string.IsNullOrEmpty(shardBulletPoolName))
            return;

        // Map angular velocity into a 0..1 ratio using the same maxAngularVelocity clamp used for spinning.
        float ratio = Mathf.InverseLerp(minAngularVelForShards, maxAngularVelocity, absAngVel);
        float speed = Mathf.Lerp(shardMinSpeed, shardMaxSpeed, ratio);

        // Damage scales with speed relative to top speed.
        float dmgT = (shardMaxSpeed <= 0.0001f) ? 0f : Mathf.Clamp01(speed / shardMaxSpeed);
        float damage = Mathf.Lerp(shardMinDamage, shardMaxDamage, dmgT);

        SpawnShards(shardCount, speed, damage);
    }

    void SpawnShards(int count, float speed, float damage)
    {
        if (count <= 0)
            return;

        // Randomize the initial angle within offset range
        float baseOffset = Random.Range(0f, shardStartAngleOffset);
        float step = 360f / count;

        for (int i = 0; i < count; i++)
        {
            GameObject bulletObj = PoolManager.Instance.GetFromPool(shardBulletPoolName);
            bulletObj.transform.position = transform.position;

            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet == null)
            {
                // If the pooled prefab is misconfigured, just skip safely.
                PoolManager.Instance.ReturnToPool(shardBulletPoolName, bulletObj);
                continue;
            }

            float angle = baseOffset + (step * i);
            if (shardAngleJitter > 0f)
                angle += Random.Range(-shardAngleJitter, shardAngleJitter);

            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            // Mirror the important bullet fields we rely on elsewhere.
            bullet.damage = damage;
            bullet.damageVarianceFactor = 0f;
            bullet.origin = this.transform;
            bullet.knockback = shardKnockback;
            bullet.destroyTime = shardLifeTime;
            bullet.objectPoolName = shardBulletPoolName;

            bullet.velocity = dir * speed;

            // Rotate to match velocity.
            bulletObj.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            bullet.Fly();
        }
    }

    IEnumerator RemoveColliderFromTempList(Health health, float delay)
    {
        yield return new WaitForSeconds(delay);
        tempHits.Remove(health);
    }
}
