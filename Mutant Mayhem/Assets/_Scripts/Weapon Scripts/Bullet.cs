using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Bullet : MonoBehaviour
{
    public LayerMask hitLayers;
    LayerMask hitLayersStart;
    public GunType gunType;
    public Light2D bulletLight;

    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected SoundSO shootSound;
    [SerializeField] protected SoundSO hitSound;

    [Header("Set by GunSO for Player, and manually here for other projectiles")]
    public string objectPoolName;

    [Header("Optional")]
    [SerializeField] GameObject AiTrggerPrefab;
    [SerializeField] float AITriggerSize;

    [Header("Set by GunSO for Player, and manually here for Enemies' projectiles")]
    public float damage = 10;
    public float damageVarianceFactor; // e.g., 0.1 = ±10% damage variance
    public float knockback = 1f;
    public float destroyTime;
    [HideInInspector] public Vector2 velocity;
    [HideInInspector] public Transform origin;
    public CriticalHit criticalHit;
    public float critChanceMult = 1;
    public float critDamageMult = 1;

    protected TileManager tileManager;
    Collider2D myCollider;
    bool isDying;

    protected virtual void Awake()
    {
        hitLayersStart = hitLayers;
        tileManager = FindObjectOfType<TileManager>();
        myCollider = GetComponent<Collider2D>();
        if (myCollider == null)
            Debug.LogError("No collider found on bullet prefab: " + gameObject.name);
    }

    protected virtual void OnDisable()
    {
        hitLayers = hitLayersStart;
        StopAllCoroutines();

        // Reset pooled state
        isDying = false;
        if (myCollider != null)
            myCollider.enabled = true;
        if (rb != null)
            rb.simulated = true;
    }

    protected virtual void FixedUpdate()
    {
        if (isDying)
            return;

        CheckCollisions();
    }

    public virtual void Fly()
    {
        // Reset pooled state
        isDying = false;
        if (myCollider != null)
            myCollider.enabled = true;
        if (rb != null)
            rb.simulated = true;

        if (shootSound != null)
            AudioManager.Instance.PlaySoundFollow(shootSound, transform);

        // Check origin point for collision
        Collider2D other = Physics2D.OverlapPoint(transform.position, hitLayers);
        if (other)
        {
            Hit(other, transform.position);
            return;
        }

        StartCoroutine(RepoolAfterSeconds());
        rb.velocity = velocity;
    }

    protected IEnumerator RepoolAfterSeconds()
    {
        // Align most of the lifetime to physics steps, but don't overshoot destroyTime.
        float elapsed = 0f;

        while (elapsed + Time.fixedDeltaTime <= destroyTime)
        {
            yield return new WaitForFixedUpdate();
            elapsed += Time.fixedDeltaTime;

            if (isDying)
                yield break;
        }

        // Cover any remaining fractional time.
        float remaining = destroyTime - elapsed;
        if (remaining > 0f && !isDying)
        {
            // Check current position first.
            Collider2D other = Physics2D.OverlapBox(myCollider.bounds.center, myCollider.bounds.size, transform.eulerAngles.z, hitLayers);
            if (other)
            {
                Hit(other, transform.position);
                yield break;
            }

            // Raycast the remaining distance (speed * remaining).
            Vector2 v = rb.velocity;
            float dist = v.magnitude * remaining;
            if (dist > 0f)
            {
                RaycastHit2D raycast = Physics2D.Raycast(transform.position, v, dist, hitLayers);
                if (raycast.collider)
                {
                    // Preserve your existing "land inside collider" behavior by using the raycast point as-is
                    // and letting Hit/your existing offset logic handle it where needed.
                    Hit(raycast.collider, raycast.point + v / 32 * remaining);
                    yield break;
                }

                // No hit: visually advance to max range for one render frame, without extending gameplay.
                Vector2 finalPos = (Vector2)transform.position + (v * remaining);
                isDying = true;
                StopAllCoroutines();
                StartCoroutine(ReturnToPoolAfterVisualFrame(finalPos));
                yield break;
            }
        }

        // No remaining fraction (or already dying): return normally.
        if (!isDying && gameObject.activeInHierarchy)
            PoolManager.Instance.ReturnToPool(objectPoolName, gameObject);
    }

    protected virtual void CheckCollisions()
    {
        // Check collider
        Collider2D other = Physics2D.OverlapBox(myCollider.bounds.center, myCollider.bounds.size, transform.eulerAngles.z, hitLayers);
        if (other)
        {
            Hit(other, transform.position);
            return;
        }

        // Check with raycast
        Vector2 raycastDir = rb.velocity;
        RaycastHit2D raycast = Physics2D.Raycast(transform.position, raycastDir,
                                                 raycastDir.magnitude * Time.fixedDeltaTime, hitLayers);
        if (raycast.collider)
        {
            Hit(raycast.collider, raycast.point + raycastDir / 32 * Time.fixedDeltaTime);
        }
    }

    #region Hit

    protected virtual void Hit(Collider2D otherCollider, Vector2 point)
    {
        bool isCritical = false;
        float critMult = 1;
        if (criticalHit != null)
            (isCritical, critMult) = criticalHit.RollForCrit(critChanceMult, critDamageMult);

        if (isCritical) 
        {
            GameObject obj = PoolManager.Instance.GetFromPool(criticalHit.effectPoolName);
            obj.transform.position = point;
            obj.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        }

        float damageNew = damage;
        damageNew *= 1 + Random.Range(-damageVarianceFactor, damageVarianceFactor);
        damageNew *= critMult;

        Vector2 hitDir = transform.right;
        EnemyBase enemy = otherCollider.GetComponent<EnemyBase>();
        float damageScale = 1;
        // Enemies
        if (enemy)
        {
            HitEnemy(enemy, hitDir, point, damageNew);
        }
        // Structures Layer #12
        else if (otherCollider.gameObject.layer == 12)
        {
            ParticleManager.Instance.PlayBulletHitWall(point, hitDir);

            // If player projectile, do 1/3 damage
            if (gameObject.CompareTag("PlayerBullet"))
            {
                damageNew /= 3;
                // Allows for scaling of textFly numbers
                damageScale = damageNew / (damage / 3);
                tileManager.ModifyHealthAt(point, -damageNew, damageScale, hitDir);
            }
            else
            {
                tileManager.ModifyHealthAt(point, -damageNew, damageScale, hitDir);
            }
        }
        else if (otherCollider.CompareTag("Player"))
        {
            PlayerHealth playerHealth = otherCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                HitPlayer(playerHealth, hitDir, point, damageNew);
            }
        }
        else if (otherCollider.CompareTag("QCube"))
        {
            QCubeHealth qCubeHealth = otherCollider.GetComponent<QCubeHealth>();
            if (qCubeHealth != null)
            {
                HitCube(qCubeHealth, hitDir, point, damageNew);
            }
        }
        else
        {
            // Drones
            DroneHealth droneHealth = otherCollider.GetComponent<DroneHealth>();
            if (droneHealth != null)
            {
                HitDrone(droneHealth, hitDir, point, damageNew);
            }
            else
                Debug.LogError("Bullet hit something with no Health component: " + otherCollider.name);
        }



        // Play bullet hit effect
        ParticleManager.Instance.PlayBulletHitEffect(gunType, point, hitDir);
        AudioManager.Instance.PlaySoundAt(hitSound, point);

        // Linger visually for one rendered frame at the final hit point,
        // without extending gameplay range/collisions.
        if (isDying)
            return;

        isDying = true;
        StopAllCoroutines();
        StartCoroutine(ReturnToPoolAfterVisualFrame(point));
    }

    IEnumerator ReturnToPoolAfterVisualFrame(Vector2 finalPos)
    {
        // NOTE: Do not StopAllCoroutines() in here, or we'll cancel this coroutine before it can pool.

        // Freeze gameplay + collisions immediately.
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }
        if (myCollider != null)
            myCollider.enabled = false;

        // Snap to final position so the bullet is drawn exactly where it ended.
        transform.position = finalPos;

        // Ensure the bullet gets rendered at its final position at least once.
        yield return new WaitForEndOfFrame();

        if (gameObject.activeInHierarchy)
            PoolManager.Instance.ReturnToPool(objectPoolName, gameObject);
    }

    #endregion

    #region Hit Types

    void HitEnemy(EnemyBase enemy, Vector2 hitDir, Vector2 point, float damageNew)
    {
        enemy.Knockback(hitDir, knockback);
        ParticleManager.Instance.PlayBulletBlood(point, hitDir);
        enemy.StartFreeze();
        enemy.EnemyChaseSOBaseInstance.StartSprint();

        float damageScale = damageNew / damage;
        enemy.ModifyHealth(-damageNew, damageScale, hitDir, gameObject);

        // Stat Counting
        if (this.gameObject.CompareTag("PlayerBullet"))
        {
            StatsCounterPlayer.EnemyDamageByPlayerProjectiles += damageNew;
            StatsCounterPlayer.ShotsHitByPlayer++;
        }
        else if (this.gameObject.CompareTag("TurretBullet"))
            StatsCounterPlayer.EnemyDamageByTurrets += damageNew;

        // Create AI Trigger
        if (AiTrggerPrefab != null)
        {
            //Debug.Log("AiTrigger activated");
            GameObject triggerObj = PoolManager.Instance.GetFromPool("Trigger_PlayerBulletHit");
            triggerObj.transform.position = point;
            triggerObj.GetComponent<AiTrigger>().origin = this.origin;
            triggerObj.transform.localScale = new Vector3(AITriggerSize, AITriggerSize, 1);
        }
    }

    void HitPlayer(PlayerHealth playerHealth, Vector2 hitDir, Vector2 point, float damageNew)
    {
        //Debug.Log("Player hit by bullet for " + damageNew + " damage");
        playerHealth.Knockback(hitDir, knockback);
        ParticleManager.Instance.PlayBulletBlood(point, hitDir);

        float damageScale = damageNew / damage;
        playerHealth.ModifyHealth(-damageNew, damageScale, hitDir, gameObject);

        // Stat Counting
        if (this.gameObject.layer == LayerMask.NameToLayer("EnemyProjectiles"))
            StatsCounterPlayer.DamageToPlayer += damageNew;
    }

    void HitDrone(Health health, Vector2 hitDir, Vector2 point, float damageNew)
    {
        health.Knockback(hitDir, knockback);
        ParticleManager.Instance.PlayBulletHitWall(point, hitDir);
        //ParticleManager.Instance.PlayBulletBlood(point, hitDir);
        // Could add freeze effect

        if (gameObject.layer == LayerMask.NameToLayer("PlayerProjectiles"))
            damageNew *= 0.5f;
        float damageScale = damageNew / damage;
        health.ModifyHealth(-damageNew, damageScale, hitDir, gameObject);

        // Stat Counting


        // Create AI Trigger
        if (AiTrggerPrefab != null)
        {
            //Debug.Log("AiTrigger activated");
            GameObject triggerObj = PoolManager.Instance.GetFromPool("Trigger_PlayerBulletHit");
            triggerObj.transform.position = point;
            triggerObj.GetComponent<AiTrigger>().origin = this.origin;
            triggerObj.transform.localScale = new Vector3(AITriggerSize, AITriggerSize, 1);
        }
    }

    void HitCube(QCubeHealth qCubeHealth, Vector2 hitDir, Vector2 point, float damageNew)
    {
        qCubeHealth.Knockback(hitDir, knockback);
        ParticleManager.Instance.PlayBulletHitWall(point, hitDir);

        float damageScale = damageNew / damage;
        qCubeHealth.ModifyHealth(-damageNew, damageScale, hitDir, gameObject);

        // Stat Counting

        // Create AI Trigger
        if (AiTrggerPrefab != null)
        {
            //Debug.Log("AiTrigger activated");
            GameObject triggerObj = PoolManager.Instance.GetFromPool("Trigger_PlayerBulletHit");
            triggerObj.transform.position = point;
            triggerObj.GetComponent<AiTrigger>().origin = this.origin;
            triggerObj.transform.localScale = new Vector3(AITriggerSize, AITriggerSize, 1);
        }
    }

    #endregion
}
