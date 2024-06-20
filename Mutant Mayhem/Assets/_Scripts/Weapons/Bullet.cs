using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Bullet : MonoBehaviour
{
    public LayerMask hitLayers;
    public float damage = 10;
    public float knockback = 1f;

    [SerializeField] protected Rigidbody2D myRb;
    [SerializeField] protected BulletEffectsHandler effectsHandler;
    //[SerializeField] protected 

    [SerializeField] GameObject AiTrggerPrefabOptional;
    [SerializeField] float AITriggerSize;

    protected TileManager tileManager;
    [HideInInspector] public float destroyTime;

    void Awake()
    {
        tileManager = FindObjectOfType<TileManager>();
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
        Vector2 raycastDir = myRb.velocity;
        RaycastHit2D raycast = Physics2D.Raycast(transform.position, raycastDir, 
                                                 raycastDir.magnitude * Time.fixedDeltaTime, hitLayers);
        if (raycast.collider)
        {
            Hit(raycast.collider, raycast.point + raycastDir/10 * Time.fixedDeltaTime);
        }
    }

    protected virtual void Hit(Collider2D otherCollider, Vector2 point)
    {
        // Enemies
        EnemyBase enemy = otherCollider.GetComponent<EnemyBase>();
        if (enemy)
        {
            //enemy.IsHit();
            enemy.Knockback(transform.right, knockback);
            enemy.BulletHitEffect(point, transform.right);
            enemy.ModifyHealth(-damage, gameObject);
            enemy.StartFreeze();
            enemy.EnemyChaseSOBaseInstance.StartSprint();


            // Layer# 8 - PlayerProjectiles
            if (this.gameObject.layer == 8)
                StatsCounterPlayer.EnemyDamageByPlayerProjectiles += damage;

            // Create AI Trigger
            if (AiTrggerPrefabOptional != null)
            {
                //Debug.Log("AiTrigger instantiated");
                GameObject trigger = Instantiate(AiTrggerPrefabOptional, transform.position, Quaternion.identity);
                trigger.transform.localScale = new Vector3(AITriggerSize, AITriggerSize, 1);
            } 
        }
        // Structures Layer #12
        else if (otherCollider.gameObject.layer == 12)
        { 
            tileManager.BulletHitEffectAt(point, transform.right);
            tileManager.ModifyHealthAt(point, -damage);

            StatsCounterPlayer.DamageToStructures += damage;

            //Debug.Log("TILE HIT");
        }

        // Play bullet hit effect

        effectsHandler.DestroyAfterSeconds();
        effectsHandler.PlayBulletHitEffectAt(point);
        
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
