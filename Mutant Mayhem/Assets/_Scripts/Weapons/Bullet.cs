using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Bullet : MonoBehaviour
{
    public LayerMask hitLayers;
    [SerializeField] TileManager tileManager;
    [SerializeField] float damage = 10;
    [SerializeField] float knockback = 1f;

    [SerializeField] Rigidbody2D myRb;
    [SerializeField] BulletTrails bulletTrail;

    [SerializeField] GameObject AiTrggerPrefabOptional;
    [SerializeField] float AITriggerSizeOptional;

    void Start()
    {
        // Check origin point for collision
        Collider2D other = Physics2D.OverlapPoint(transform.position, hitLayers);
        if (other)
        {
            Hit(other, transform.position);
        }
    }

    void FixedUpdate()
    {
        CheckCollisions();
    }

    void CheckCollisions()
    {
        // Check with raycast
        Vector2 raycastDir = myRb.velocity;
        RaycastHit2D raycast = Physics2D.Raycast(transform.position, raycastDir, 
                                                 raycastDir.magnitude * Time.deltaTime, hitLayers);
        if (raycast.collider)
        {
            Hit(raycast.collider, raycast.point + raycastDir/10 * Time.deltaTime);
        }
    }

    void Hit(Collider2D otherCollider, Vector2 point)
    {
        // Enemies
        EnemyBase enemy = otherCollider.GetComponent<EnemyBase>();
        if (enemy)
        {
            //enemy.IsHit();
            enemy.Knockback(transform.right, knockback);
            enemy.BulletHitEffect(point, transform.right);
            enemy.ModifyHealth(-damage);
            if (AiTrggerPrefabOptional != null)
            {
                Debug.Log("AiTrigger instantiated");
                GameObject trigger = Instantiate(AiTrggerPrefabOptional, transform.position, Quaternion.identity);
                trigger.transform.localScale = new Vector3(AITriggerSizeOptional, AITriggerSizeOptional, 1);
            } 
        }
        // Structures Layer #12
        else if (otherCollider.gameObject.layer == 12)
        { 
            tileManager.BulletHitEffectAt(point, transform.right);
            tileManager.ModifyHealthAt(point, -damage);
            Debug.Log("TILE HIT");
        }

        // Delay bullet trail destroy
        if (bulletTrail != null)
        {
            bulletTrail.DestroyAfterSeconds();
            bulletTrail.transform.parent = null;
        }

        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
