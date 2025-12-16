using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SpinningBlades : MonoBehaviour, ITileObject, ITileObjectExplodable
{
    [SerializeField] List<Sprite> damagedSprites;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Light2D postLight;

    [SerializeField] bool hasPower = true;
    public bool canBeDamaged = true;


    List<Health> tempHits = new List<Health>();
    float previousAngularVelocity = 0f;

    public string explosionPoolName;
    SpinningBladesManager bladesManager;
    Vector2 centerPos = Vector2.zero;

    void Start()
    {
        bladesManager = SpinningBladesManager.Instance;
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
        Vector3Int rootPos = TileManager.Instance.WorldToGrid(transform.position);
        rootPos = TileManager.Instance.GridToRootPos(rootPos);
        centerPos = TileManager.Instance.TileCellsCenterToWorld(rootPos);
        Debug.Log($"[SpinningBlades] Explode: transformPos={transform.position} grid={TileManager.Instance.WorldToGrid(transform.position)} rootPos={rootPos} centerPos={centerPos} prevAngVel={previousAngularVelocity}");

        TrySpawnShardExplosion(previousAngularVelocity);

        if (!string.IsNullOrEmpty(explosionPoolName))
        {
            GameObject explosion = PoolManager.Instance.GetFromPool(explosionPoolName);
            explosion.transform.position = centerPos;
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
        if (hasPower && Mathf.Abs(rb.angularVelocity) < bladesManager.maxAngularVelocity)
            rb.AddTorque(bladesManager.rotationForce * Time.fixedDeltaTime);
        
        canBeDamaged = Mathf.Abs(rb.angularVelocity) < bladesManager.minAngularVelocityToDamage;

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
            float damageToApply = bladesManager.damagePerAngularVel * Mathf.Abs(previousAngularVelocity);
            // Get ratio of damage from min to max angular velocity for TextFly scaling
            float damageScale = 1 + Mathf.Clamp01(Mathf.Abs(previousAngularVelocity) / bladesManager.maxAngularVelocity);
            Vector2 hitDir = otherCollider.transform.position - transform.position;
            health.ModifyHealth(-damageToApply, damageScale, hitDir, otherCollider.gameObject);

            // Self damage
            float selfDamage = bladesManager.selfDamagePerAngularVel * Mathf.Abs(previousAngularVelocity);
            TileManager.Instance.ModifyHealthAt(transform.position, -selfDamage, 1.5f, -hitDir);

            // Add to temp list to avoid multiple damage instances in short time
            tempHits.Add(health);
            StartCoroutine(RemoveColliderFromTempList(health, bladesManager.damageInterval));
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (Mathf.Abs(previousAngularVelocity) < bladesManager.minAngularVelocityToDamage)
            return;

        // Damage health on collision
        Collider2D otherCollider = collision.collider;
        if (otherCollider.CompareTag("Enemy") || otherCollider.CompareTag("Player"))
        {
            ApplyDamage(otherCollider);
        }
    }

    void TrySpawnShardExplosion(float angularVel)
    {
        Debug.Log($"[SpinningBlades] TrySpawnShardExplosion called. angularVel={angularVel} abs={Mathf.Abs(angularVel)} centerPos={centerPos} enable={bladesManager.enableShardExplosion} pool='{bladesManager.shardBulletPoolName}' count={bladesManager.shardCount}");

        if (!bladesManager.enableShardExplosion)
            return;

        float absAngVel = Mathf.Abs(angularVel);
        if (absAngVel < bladesManager.minAngularVelForShards)
            return;

        if (string.IsNullOrEmpty(bladesManager.shardBulletPoolName))
            return;

        Debug.Log($"[SpinningBlades] Spawning shards: absAngVel={absAngVel}, count={bladesManager.shardCount}");

        // Map angular velocity into a 0..1 ratio using the same maxAngularVelocity clamp used for spinning.
        float ratio = Mathf.InverseLerp(bladesManager.minAngularVelForShards, bladesManager.maxAngularVelocity, absAngVel);
        float speed = Mathf.Lerp(bladesManager.shardMinSpeed, bladesManager.shardMaxSpeed, ratio);

        // Damage scales with speed relative to top speed.
        float dmgT = (bladesManager.shardMaxSpeed <= 0.0001f) ? 0f : Mathf.Clamp01(speed / bladesManager.shardMaxSpeed);
        float damage = Mathf.Lerp(bladesManager.shardMinDamage, bladesManager.shardMaxDamage, dmgT);
        SpawnShards(bladesManager.shardCount, speed, damage);
    }

    void SpawnShards(int count, float speed, float damage)
    {
        if (count <= 0)
            return;

        if (PoolManager.Instance == null)
        {
            Debug.LogError("[SpinningBlades] SpawnShards: PoolManager.Instance is null.");
            return;
        }

        if (string.IsNullOrEmpty(bladesManager.shardBulletPoolName))
        {
            Debug.LogError("[SpinningBlades] SpawnShards: shardBulletPoolName is null/empty.");
            return;
        }

        Debug.Log($"[SpinningBlades] SpawnShards starting. count={count} centerPos={centerPos} speed={speed} damage={damage} pool='{bladesManager.shardBulletPoolName}'");

        // Spawn base/offset: if we spawn inside our own collider, shards can instantly collide and vanish.
        Collider2D bladesCol = GetComponent<Collider2D>();
        Vector2 spawnBase = centerPos;
        float spawnRadius = 0.25f;
        if (bladesCol != null)
        {
            spawnBase = bladesCol.bounds.center;
            spawnRadius = Mathf.Max(bladesCol.bounds.extents.x, bladesCol.bounds.extents.y);
        }

        // Randomize the initial angle within offset range
        float angleOffset = Random.Range(0f, bladesManager.shardStartAngleRandomOffset);
        float step = 360f / count;

        for (int i = 0; i < count; i++)
        {
            GameObject bulletObj = PoolManager.Instance.GetFromPool(bladesManager.shardBulletPoolName);
            if (bulletObj == null)
            {
                Debug.LogError($"[SpinningBlades] SpawnShards: GetFromPool returned null for pool '{bladesManager.shardBulletPoolName}' at i={i}.");
                continue;
            }

            // Ensure pooled object is active before we configure and launch it.
            if (!bulletObj.activeSelf)
                bulletObj.SetActive(true);

            // Position will be set after we compute the direction, so it spawns outside our collider.
            bulletObj.transform.position = spawnBase;

            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet == null)
            {
                Debug.LogError($"[SpinningBlades] SpawnShards: Pooled shard prefab missing Bullet component. pool='{bladesManager.shardBulletPoolName}', obj='{bulletObj.name}'. Returning to pool.");
                PoolManager.Instance.ReturnToPool(bladesManager.shardBulletPoolName, bulletObj);
                continue;
            }

            float angle = angleOffset + (step * i);
            if (bladesManager.shardAngleJitter > 0f)
                angle += Random.Range(-bladesManager.shardAngleJitter, bladesManager.shardAngleJitter);

            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            // Spawn slightly outside our own collider along the firing direction.
            bulletObj.transform.position = spawnBase + dir * spawnRadius;

            // Get knockback based on speed
            float knockback = bladesManager.shardKnockback;
            knockback = speed / bladesManager.shardMaxSpeed * knockback;

            // Mirror the important bullet fields we rely on elsewhere.
            bullet.damage = damage;
            bullet.damageVarianceFactor = 0f;
            bullet.origin = this.transform;
            bullet.knockback = knockback;
            bullet.destroyTime = bladesManager.shardLifeTime;
            bullet.objectPoolName = bladesManager.shardBulletPoolName;

            bullet.velocity = dir * speed;

            // Rotate to match velocity.
            bulletObj.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            Debug.Log($"[SpinningBlades] Shard i={i} angle={angle} dir={dir} speed={speed} vel={bullet.velocity} spawnBase={spawnBase} spawnRadius={spawnRadius} pos={bulletObj.transform.position} active={bulletObj.activeSelf}");

            bullet.Fly();
        }
    }

    IEnumerator RemoveColliderFromTempList(Health health, float delay)
    {
        yield return new WaitForSeconds(delay);
        tempHits.Remove(health);
    }
}
