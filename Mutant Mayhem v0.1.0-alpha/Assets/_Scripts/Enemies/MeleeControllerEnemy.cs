using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeControllerEnemy : MonoBehaviour
{
    public float meleeDamage = 20f;
    public float knockback = 10f;
    [SerializeField] float selfKnockback = 5f;
    [SerializeField] float timeBetweenAttacks = 1f;
    [SerializeField] float meleeTileDotProdRange = 0.5f;
    [SerializeField] Collider2D meleeCollider;
    [SerializeField] Animator meleeAnim;
    [SerializeField] SoundSO meleeSound;

    Health myHealth;
    TileManager tileManager;
    bool waitToAttack;

    void Awake()
    {
        myHealth = GetComponentInParent<Health>();
        tileManager = FindObjectOfType<TileManager>();
    }

    public void Hit(Health otherHealth, Vector2 point)
    {   
        waitToAttack = true;
        StartCoroutine(AttackTimer());
        meleeAnim.SetTrigger("Melee");   
        myHealth.Knockback((Vector2)myHealth.transform.position - point, selfKnockback);
        otherHealth.Knockback((Vector2)otherHealth.transform.position - point, knockback);
        otherHealth.MeleeHitEffect(point, transform.right);
        otherHealth.ModifyHealth(-meleeDamage, gameObject);

        PlayMeleeSound(point);

        StatsCounterPlayer.MeleeAttacksByEnemies++;
        StatsCounterPlayer.MeleeDamageByEnemies += meleeDamage;
    }

    public void HitStructure(Vector2 point)
    {
        // Find dotProdcut
        Vector2 dir = (point - (Vector2)myHealth.transform.position).normalized;
        float dotProduct = Vector2.Dot(myHealth.transform.right, dir);

        // Move the point "inside" the tile for tileManager dictionary detection.
        // The could be improved to avoid the odd tile miss on corners.
        point += dir / 20;

        if (dotProduct > meleeTileDotProdRange)
        {
            waitToAttack = true;
            StartCoroutine(AttackTimer());
            meleeAnim.SetTrigger("Melee"); 
            myHealth.Knockback((Vector2)myHealth.transform.position - point, selfKnockback * 0.8f);
            tileManager.ModifyHealthAt(point, -meleeDamage);
            tileManager.MeleeHitEffectAt(point, transform.right);

            PlayMeleeSound(point);

            StatsCounterPlayer.MeleeAttacksByEnemies++;
            StatsCounterPlayer.DamageToStructures += meleeDamage;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (waitToAttack)
            return;

        // Structures layer# 12
        if (other.gameObject.layer == 12)
        {
            HitStructure(other.ClosestPoint(transform.position));
        }
        // Attack Player
        else if (other.CompareTag("Player") || other.CompareTag("PlayerBody"))
        {
            Vector2 point = other.ClosestPoint(transform.position);
            Health health = other.GetComponentInParent<Health>();
            if (health)
                Hit(health, point);
            else
                Debug.LogError("Health not found");
        }
        // Attack Cube
        else if (other.CompareTag("QCube"))
        {
            Vector2 point = other.ClosestPoint(transform.position);
            Hit(other.GetComponent<Health>(), point);
        }        
    }

    void PlayMeleeSound(Vector2 point)
    {
        AudioManager.Instance.PlaySoundAt(meleeSound, point);;
    }

    IEnumerator AttackTimer()
    {
        yield return new WaitForSeconds(timeBetweenAttacks);
        waitToAttack = false;
    }
}
