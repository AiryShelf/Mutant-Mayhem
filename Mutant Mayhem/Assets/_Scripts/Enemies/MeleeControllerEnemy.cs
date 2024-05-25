using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class MeleeControllerEnemy : MonoBehaviour
{
    public float meleeDamage = 10f;
    public float knockback = 10f;
    public float selfKnockback = 5f;
    public float meleeAttackSpeed = 0.5f;
    public float meleeAttackRange = 0.1f;
    [SerializeField] float meleeTileDotProduct;
    [SerializeField] PolygonCollider2D meleeCollider;
    [SerializeField] Animator meleeAnimator;
    [SerializeField] Health myHealth;
    
    TileManager tileManager;
    [HideInInspector] public Coroutine attackingCoroutine = null;
    [HideInInspector] public bool isAttacking;
    bool waitToAttackTile;

    void Start()
    {
        tileManager = FindObjectOfType<TileManager>();
    }

    void Update()
    {
        Attack();
    }

    public void Attack()
    {
        if (isAttacking && attackingCoroutine == null)
        {
            attackingCoroutine = StartCoroutine(AttackContinuously());
        }
    }

    // Triggered by animation
    public void MeleeAnimationAttackToggle(int isOn)
    {
        if (isOn == 1)
        {
            meleeCollider.enabled = true;
        }
        else if (isOn == 0)
        {
            meleeCollider.enabled = false;
        }
    }

    IEnumerator AttackContinuously()
    {
        meleeAnimator.SetTrigger("Melee");
        yield return new WaitForSeconds(meleeAttackSpeed);;
        attackingCoroutine = null;
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.tag == "Player")
        {
            Debug.Log("Player was detected by enemy Melee");
            Hit(other, other.ClosestPoint(transform.position));
        }
    }

    void Hit(Collider2D other, Vector2 point)
    {      
        // Player
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            Health otherHealth = other.GetComponent<Health>();
            if (otherHealth != null)
            {
                myHealth.Knockback((Vector2)myHealth.transform.position - point, selfKnockback);
                otherHealth.Knockback((Vector2)otherHealth.transform.position - point, knockback);
                otherHealth.MeleeHitEffect(point, transform.right);
                otherHealth.ModifyHealth(-meleeDamage);
            }
        }
    }

    public void HitStructure(Vector2 point)
    {
        if (!waitToAttackTile)
        {
            Vector2 dir = (point - (Vector2)myHealth.transform.position).normalized;
            float dotProduct = Vector2.Dot(myHealth.transform.right, dir);
            point += dir / 10;

            if (dotProduct > meleeTileDotProduct)
            {
                myHealth.Knockback((Vector2)myHealth.transform.position - point, selfKnockback / 2);
                tileManager.ModifyHealthAt(point, -meleeDamage);
                tileManager.MeleeHitEffectAt(point, transform.right);
            }  
            waitToAttackTile = true;
            StartCoroutine(WaitToAttackTile()); 
        }     
    }

    IEnumerator WaitToAttackTile()
    {
        yield return new WaitForSeconds(meleeAttackSpeed);
        waitToAttackTile = false;
    }
}
