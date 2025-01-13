using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public LayerMask hitLayers;
    LayerMask hitLayersStart;
    public GunType gunType;
    public string objectPoolName;
    [HideInInspector] public float damage = 10;
    [HideInInspector] public float damageVariance;
    [HideInInspector] public float knockback = 1f;
    [HideInInspector] public float destroyTime;
    [HideInInspector] public Vector2 velocity;
    [HideInInspector] public Transform origin;
    [HideInInspector] public CriticalHit criticalHit;
    [HideInInspector] public float critChanceMult = 1;
    [HideInInspector] public float critDamageMult = 1;

    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected SoundSO shootSound;
    [SerializeField] protected SoundSO hitSound;

    [Header("Optional")]
    [SerializeField] GameObject AiTrggerPrefab;
    [SerializeField] float AITriggerSize;

    protected TileManager tileManager;

    void Awake()
    {
        hitLayersStart = hitLayers;
        tileManager = FindObjectOfType<TileManager>();
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
        SFXManager.Instance.PlaySoundFollow(shootSound, transform);

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
        // Check with raycast
        Vector2 raycastDir = rb.velocity;
        RaycastHit2D raycast = Physics2D.Raycast(transform.position, raycastDir, 
                                                 raycastDir.magnitude * Time.fixedDeltaTime, hitLayers);
        if (raycast.collider)
        {
            Hit(raycast.collider, raycast.point + raycastDir/10 * Time.fixedDeltaTime);
        }
    }

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
            if (otherCollider.gameObject.layer == LayerMask.NameToLayer("PlayerProjectiles"))
            {
                tileManager.ModifyHealthAt(point, -damageNew / 3, damageScale, hitDir);
            }
            else
            {
                tileManager.ModifyHealthAt(point, -damageNew, damageScale, hitDir);
            }
        }

        // Play bullet hit effect
        ParticleManager.Instance.PlayBulletHitEffect(gunType, point, hitDir);
        SFXManager.Instance.PlaySoundAt(hitSound, point);
        
        // Return to pool
        PoolManager.Instance.ReturnToPool(objectPoolName, gameObject);
    }

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
            //Debug.Log("AiTrigger instantiated");
            GameObject triggerObj = Instantiate(AiTrggerPrefab, point, Quaternion.identity);
            triggerObj.GetComponent<AiTrigger>().origin = this.origin;
            triggerObj.transform.localScale = new Vector3(AITriggerSize, AITriggerSize, 1);
        } 
    }
}
