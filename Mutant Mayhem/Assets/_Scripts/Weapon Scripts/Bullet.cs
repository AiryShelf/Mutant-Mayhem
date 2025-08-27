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

    [Header("Set by GunSO for Player, and manually here for Enemies' projectiles")]
    public string objectPoolName;

    [Header("Optional")]
    [SerializeField] GameObject AiTrggerPrefab;
    [SerializeField] float AITriggerSize;

    [Header("Set by GunSO for Player, and manually here for Enemies' projectiles")]
    public float damage = 10;
    public float damageVariance;
    public float knockback = 1f;
    public float destroyTime;
    [HideInInspector] public Vector2 velocity;
    [HideInInspector] public Transform origin;
    public CriticalHit criticalHit;
    public float critChanceMult = 1;
    public float critDamageMult = 1;

    protected TileManager tileManager;
    Collider2D myCollider;

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
    }

    protected virtual void FixedUpdate()
    {
        CheckCollisions();
    }

    public virtual void Fly()
    {
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
        //Debug.Log("Destroying bullet after seconds: " + destroyTime);
        yield return new WaitForSeconds(destroyTime);

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

        float damageNew = damage;
        damageNew *= 1 + Random.Range(-damageVariance, damageVariance);
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

        // Return to pool
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
        Debug.Log("Player hit by bullet for " + damageNew + " damage");
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
