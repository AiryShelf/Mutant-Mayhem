using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public LayerMask hitLayers;
    [HideInInspector] public float damage = 10;
    [HideInInspector] public float knockback = 1f;
    [HideInInspector] public float destroyTime;
    [HideInInspector] public Vector2 velocity;
    [HideInInspector] public Transform origin;

    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected BulletEffectsHandler effectsHandler;
    [SerializeField] protected SoundSO shootSound;
    [SerializeField] protected SoundSO hitSound;

    [Header("Optional")]
    [SerializeField] GameObject AiTrggerPrefab;
    [SerializeField] float AITriggerSize;

    protected TileManager tileManager;

    void Awake()
    {
        tileManager = FindObjectOfType<TileManager>();
    }

    void OnEnable()
    {
        AudioManager.instance.PlaySoundFollow(shootSound, transform);
    }

    protected virtual void Start()
    {
        // Check origin point for collision
        Collider2D other = Physics2D.OverlapPoint(transform.position, hitLayers);
        if (other)
        {
            Hit(other, transform.position);
            return;
        }

        StartCoroutine(DestroyAfterSeconds());
        rb.velocity = velocity;
    }

    protected virtual void FixedUpdate()
    {
        CheckCollisions();
    }

    protected IEnumerator DestroyAfterSeconds()
    {
        //Debug.Log("Destroying bullet after seconds: " + destroyTime);
        yield return new WaitForSeconds(destroyTime);

        effectsHandler.DestroyAfterSeconds();

        Destroy(gameObject);
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
        Vector2 hitDir = transform.right;

        // Enemies
        EnemyBase enemy = otherCollider.GetComponent<EnemyBase>();
        if (enemy)
        {
            enemy.Knockback(hitDir, knockback);
            enemy.BulletHitEffect(point, hitDir);
            enemy.ModifyHealth(-damage, gameObject);
            enemy.StartFreeze();
            enemy.EnemyChaseSOBaseInstance.StartSprint();

            // Layer# 8 - PlayerProjectiles
            if (this.gameObject.layer == 8)
                StatsCounterPlayer.EnemyDamageByPlayerProjectiles += damage;

            // Create AI Trigger
            if (AiTrggerPrefab != null)
            {
                //Debug.Log("AiTrigger instantiated");
                GameObject triggerObj = Instantiate(AiTrggerPrefab, point, Quaternion.identity);
                triggerObj.GetComponent<AiTrigger>().origin = this.origin;
                triggerObj.transform.localScale = new Vector3(AITriggerSize, AITriggerSize, 1);
            } 
        }
        // Structures Layer #12
        else if (otherCollider.gameObject.layer == 12)
        { 
            tileManager.BulletHitEffectAt(point, hitDir);

            // If player projectile, do 1/3 damage
            if (otherCollider.gameObject.layer == LayerMask.NameToLayer("PlayerProjectiles"))
            {
                tileManager.ModifyHealthAt(point, -damage / 3);
            }
            else
            {
                tileManager.ModifyHealthAt(point, -damage);
            }
            

            StatsCounterPlayer.DamageToStructures += damage;

            //Debug.Log("TILE HIT");
        }

        // Play bullet hit effect
        effectsHandler.DestroyAfterSeconds();
        effectsHandler.PlayBulletHitEffectAt(point);

        AudioManager.instance.PlaySoundAt(hitSound, point);
        
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
