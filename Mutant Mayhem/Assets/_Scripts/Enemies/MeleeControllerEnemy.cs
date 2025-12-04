using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeControllerEnemy : MonoBehaviour
{
    public float meleeDamage = 20f; // Set by EnemyBase or Mutant
    public float meleeDamageStart; // Set by EnemyBase or Mutant
    [Range(0, 1)]
    [SerializeField] float damageVariance = 0.15f;
    public float knockback = 10f;
    public float knockbackStart;
    public float selfKnockback = 5f;
    public float selfKnockbackStart;
    public float attackDelay = 1f;
    public float attackDelayStart = 1;
    [SerializeField] LayerMask hitLayers;
    [SerializeField] Collider2D meleeCollider;
    [SerializeField] float scaleDuration = 0.1f;
    [SerializeField] Vector3 initialScale = new Vector3(0.3f, 0.3f, 1f);
    [SerializeField] Vector3 maxScale = new Vector3(1f, 1f, 1f);
    public Animator meleeAnimator;
    public SoundSO meleeSound;

    ContactFilter2D contactFilter;
    List<Collider2D> colliders;
    Health myHealth;
    TileManager tileManager;
    CriticalHit criticalHit;
    Coroutine scaleCoroutine;

    [Header("Dynamic, don't set here")]
    public bool waitToAttack;
    public bool isElevated;
    public EnemyBase enemyBase;
    public float playerDamageDealt = 0;
    public float structureDamageDealt = 0;

    void Awake()
    {
        myHealth = GetComponentInParent<Health>();
        tileManager = FindObjectOfType<TileManager>();
        criticalHit = GetComponent<CriticalHit>();

        // Initialize ContactFilter2D
        contactFilter = new ContactFilter2D
        {
            useTriggers = true
        };

        SetContactFilter(hitLayers);
        colliders = new List<Collider2D>(10);

        selfKnockbackStart = selfKnockback;
        knockbackStart = knockback;
        meleeDamageStart = meleeDamage;
        attackDelayStart = attackDelay;
        //transform.localScale = initialScale;
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    public void SetContactFilter(LayerMask layerMask)
    {
        contactFilter.SetLayerMask(layerMask);
    }

    public void ResetContactFilter()
    {
        contactFilter.SetLayerMask(hitLayers);
    }

    public void Reset()
    {
        //meleeAnimator.Rebind();
        meleeAnimator.Update(0f);
        meleeAnimator.Play("No Attack");
        meleeCollider.enabled = true;
        meleeCollider.transform.localScale = maxScale;

        waitToAttack = false;
        meleeDamage = meleeDamageStart;
        knockback = knockbackStart;
        selfKnockback = selfKnockbackStart;
        attackDelay = attackDelayStart;
        //meleeSprite.enabled = false;
    }

    float GetDamage()
    {
        bool isCritical = false;
        float critMult = 1;
        if (criticalHit != null)
            (isCritical, critMult) = criticalHit.RollForCrit(1, 1);

        if (isCritical)
        {
            GameObject obj = PoolManager.Instance.GetFromPool(criticalHit.effectPoolName);
            obj.transform.position = meleeCollider.transform.position;
            obj.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        }

        float damage = meleeDamage;
        damage *= 1 + Random.Range(-damageVariance, damageVariance);
        damage *= critMult;

        return damage;
    }
    
    void PlayMeleeSound(Vector2 point)
    {
        AudioManager.Instance.PlaySoundAt(meleeSound, point); ;
    }

    #region Hit Methods ----------------------------------------------------

    void Hit(Health otherHealth, Vector2 point)
    {
        float damage = GetDamage();

        StartCoroutine(AttackTimer());
        meleeAnimator.SetTrigger("Melee");
        meleeAnimator.transform.position = point;

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
        playerDamageDealt += damage;
    }

    void HitStructure(Vector2 point)
    {
        float damage = GetDamage();

        Vector2 dir = (point - (Vector2)myHealth.transform.position).normalized;

        // Move the point "inside" the tile for tileManager dictionary detection
        // This could be improved to avoid the odd tile miss on corners.
        point += dir / 20;

        StartCoroutine(AttackTimer());
        meleeAnimator.SetTrigger("Melee");
        meleeAnimator.transform.position = point;
        myHealth.Knockback((Vector2)myHealth.transform.position - point, selfKnockback * 0.8f);
        float damageScale = damage / meleeDamage;
        tileManager.ModifyHealthAt(point, -damage, damageScale, dir);
        ParticleManager.Instance.PlayMeleeHitWall(point, transform.right);

        PlayMeleeSound(point);
        StatsCounterPlayer.MeleeAttacksByEnemies++;
        structureDamageDealt += damage;
    }

    #endregion

    #region Melee Control ----------------------------------------------------

    IEnumerator ScaleMeleeObject()
    {
        // Scale up the melee collider over time
        float elapsed = 0f;
        Vector3 startScale = initialScale;
        Vector3 targetScale = maxScale;

        while (elapsed < scaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / scaleDuration;
            meleeCollider.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (waitToAttack || !gameObject.activeInHierarchy)
            return;

        colliders.Clear();
        meleeCollider.OverlapCollider(contactFilter, colliders);

        Collider2D targetCollider = null;
        int highestPriority = int.MaxValue;

        // Loop through colliders to find the highest-priority target
        foreach (var collider in colliders)
        {
            if (!collider || collider == meleeCollider)
                continue;

            int priority = GetTargetPriority(collider);
            if (priority < highestPriority)
            {
                highestPriority = priority;
                targetCollider = collider;
            }
        }

        // Handle the chosen target
        if (targetCollider != null)
        {
            Vector2 attackPoint = targetCollider.ClosestPoint(transform.position);

            // Check for structure blocking the Cube
            if (targetCollider.CompareTag("QCube"))
            {
                Vector2 directionToCube = (Vector2)targetCollider.transform.position - (Vector2)transform.position;
                float distanceToCube = Vector2.Distance(transform.position, targetCollider.transform.position);

                RaycastHit2D rayHit = Physics2D.Raycast(transform.position, directionToCube,
                                                        distanceToCube, LayerMask.GetMask("Structures", "QGate"));
                if (rayHit.collider != null)
                {
                    HitStructure(rayHit.point);
                    return;
                }
            }

            HandleMeleeCollision(targetCollider, attackPoint);
        }

        //StartCoroutine(AttackTimer());
    }

    int GetTargetPriority(Collider2D collider)
    {
        if (collider.gameObject.layer == 12) // Structure
            return 3;
        if (collider.CompareTag("Player") || collider.CompareTag("PlayerBody"))
            return 1;
        if (collider.CompareTag("QGate"))
            return 2;
        if (collider.CompareTag("QCube"))
            return 5;
        if (collider.CompareTag("Drone"))
            return 4;

        return int.MaxValue;
    }

    void HandleMeleeCollision(Collider2D collider, Vector2 point)
    {
        if (collider.gameObject.layer == 12) // Structure
        {
            HitStructure(point);
        }
        else if (collider.CompareTag("Player") || collider.CompareTag("PlayerBody"))
        {
            Health health = collider.GetComponentInParent<Health>();
            if (health)
                Hit(health, point);
        }
        else if (collider.CompareTag("QGate"))
        {
            HitStructure(point);
        }
        else if (collider.CompareTag("QCube"))
        {
            Health health = collider.GetComponent<Health>();
            if (health)
                Hit(health, point);
        }
        else if (collider.CompareTag("Drone"))
        {
            Drone drone = collider.GetComponent<Drone>();
            if (drone.isFlying && !isElevated)
                return;

            //Debug.Log("Enemy found drone, trying to hit");
            Health health = collider.GetComponent<Health>();
            if (health)
                Hit(health, point);
        }
    }

    IEnumerator AttackTimer()
    {
        waitToAttack = true;
        meleeCollider.enabled = false;

        meleeCollider.transform.localScale = maxScale;

        yield return new WaitForSeconds(attackDelay);

        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
            scaleCoroutine = null;
        }

        meleeCollider.transform.localScale = initialScale;
        waitToAttack = false;
        meleeCollider.enabled = true;

        if (scaleCoroutine == null)
        {
            scaleCoroutine = StartCoroutine(ScaleMeleeObject());
        }
    }
    
    #endregion
}
