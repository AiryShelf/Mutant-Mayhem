using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeControllerEnemy : MonoBehaviour
{
    public float meleeDamage = 20f;
    [Range(0, 1)]
    [SerializeField] float damageVariance = 0.15f;
    float meleeDamageStart;
    public float knockback = 10f;
    float knockbackStart;
    [SerializeField] float selfKnockback = 5f;
    [SerializeField] float timeBetweenAttacks = 1f;
    [SerializeField] float meleeTileDotProdRange = 0.5f;
    [SerializeField] Collider2D meleeCollider;
    public Animator meleeAnimator;
    [SerializeField] SoundSO meleeSound;

    Health myHealth;
    TileManager tileManager;
    CriticalHit criticalHit;

    [Header("Dynamic, don't set here")]
    public bool waitToAttack;

    void Awake()
    {
        myHealth = GetComponentInParent<Health>();
        tileManager = FindObjectOfType<TileManager>();
        criticalHit = GetComponent<CriticalHit>();

        meleeDamageStart = meleeDamage;
        knockbackStart = knockback;
    }

    void OnEnable()
    {
        meleeDamage = meleeDamageStart;
        knockback = knockbackStart;
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    public void Reset()
    {
        meleeAnimator.Rebind();
        //meleeAnimator.Play("No Attack", 0, 0.0f);
        //meleeAnimator.ResetTrigger("Melee");
        
        waitToAttack = false;
    }

    public void Hit(Health otherHealth, Vector2 point)
    {   
        bool isCritical = false;
        float critMult = 1;
        if (criticalHit != null)
            (isCritical, critMult) = criticalHit.RollForCrit(1, 1);
        
        float damage = meleeDamage;
        damage *= 1 + Random.Range(-damageVariance, damageVariance);
        damage *= critMult;

        waitToAttack = true;
        if (gameObject.activeSelf)
            StartCoroutine(AttackTimer());

        meleeAnimator.SetTrigger("Melee");
        myHealth.Knockback((Vector2)myHealth.transform.position - point, selfKnockback);
        Vector2 hitDir = (Vector2)otherHealth.transform.position - point;
        otherHealth.Knockback(hitDir, knockback);

        float damageScale = damage / meleeDamage;
        otherHealth.ModifyHealth(-damage, damageScale, hitDir, gameObject);

        if (otherHealth.CompareTag("Player"))
            ParticleManager.Instance.PlayMeleeBlood(point, transform.right);

        PlayMeleeSound(point);

        StatsCounterPlayer.MeleeAttacksByEnemies++;
        StatsCounterPlayer.MeleeDamageByEnemies += damage;
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
            meleeAnimator.SetTrigger("Melee"); 
            myHealth.Knockback((Vector2)myHealth.transform.position - point, selfKnockback * 0.8f);
            tileManager.ModifyHealthAt(point, -meleeDamage);
            ParticleManager.Instance.PlayMeleeHitWall(point, transform.right);

            PlayMeleeSound(point);

            StatsCounterPlayer.MeleeAttacksByEnemies++;
            //StatsCounterPlayer.DamageToStructures += meleeDamage;
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
        SFXManager.Instance.PlaySoundAt(meleeSound, point);;
    }

    IEnumerator AttackTimer()
    {
        yield return new WaitForSeconds(timeBetweenAttacks);
        waitToAttack = false;
    }
}
