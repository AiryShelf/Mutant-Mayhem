using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeControllerEnemy : MonoBehaviour
{
    public float meleeDamage = 20f;
    public float meleeDamageStart;
    [Range(0, 1)]
    [SerializeField] float damageVariance = 0.15f;
    public float knockback = 10f;
    public float knockbackStart;
    [SerializeField] float selfKnockback = 5f;
    public float attackDelay = 1f;
    public float attackDelayStart = 1;
    [SerializeField] LayerMask hitLayers;
    [SerializeField] float meleeTile_DotProdRange = -1f;
    [SerializeField] Collider2D meleeCollider;
    [SerializeField] float scaleDuration = 0.1f;
    [SerializeField] Vector3 initialScale = new Vector3(0.3f, 0.3f, 1f);
    [SerializeField] Vector3 maxScale = new Vector3(1f, 1f, 1f);
    public Animator meleeAnimator;
    [SerializeField] SpriteRenderer meleeSprite;
    [SerializeField] SoundSO meleeSound;

    ContactFilter2D contactFilter;
    List<Collider2D> colliders;
    Health myHealth;
    TileManager tileManager;
    CriticalHit criticalHit;
    Coroutine scaleCoroutine;

    [Header("Dynamic, don't set here")]
    public bool waitToAttack;
    public bool isElevated;

    void Awake()
    {
        myHealth = GetComponentInParent<Health>();
        tileManager = FindObjectOfType<TileManager>();
        criticalHit = GetComponent<CriticalHit>();

        // Initialize ContactFilter2D
        contactFilter = new ContactFilter2D
        {
            useTriggers = false
        };
        
        SetContactFilter(hitLayers);
        colliders = new List<Collider2D>(10);

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
        attackDelay = attackDelayStart;
        //meleeSprite.enabled = false;
    }

    IEnumerator ScaleMeleeObject()
{
    float elapsed = 0f;
    Vector3 startScale = initialScale;
    Vector3 targetScale = maxScale;

    while (elapsed < scaleDuration)
    {
        elapsed += Time.deltaTime;
        float t = elapsed / scaleDuration;
        meleeCollider.transform.localScale = Vector3.Lerp(startScale, targetScale, t); // Smoothly scale the object
        yield return null;
    }

    transform.localScale = targetScale; // Ensure the final scale is set
}

    void Hit(Health otherHealth, Vector2 point)
    {   
        float damage = GetDamage();

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

    void HitStructure(Vector2 point)
    {
        float damage = GetDamage();
        
        // Find dotProduct
        Vector2 dir = (point - (Vector2)myHealth.transform.position).normalized;

        // Move the point "inside" the tile for tileManager dictionary detection
        // This could be improved to avoid the odd tile miss on corners.
        point += dir / 20;

        StartCoroutine(AttackTimer());
        meleeAnimator.SetTrigger("Melee"); 
        myHealth.Knockback((Vector2)myHealth.transform.position - point, selfKnockback * 0.8f);
        float damageScale = damage / meleeDamage;
        tileManager.ModifyHealthAt(point, -damage, damageScale, dir);
        ParticleManager.Instance.PlayMeleeHitWall(point, transform.right);

        PlayMeleeSound(point);
        StatsCounterPlayer.MeleeAttacksByEnemies++;
    }

    float GetDamage()
    {
        bool isCritical = false;
        float critMult = 1;
        if (criticalHit != null)
            (isCritical, critMult) = criticalHit.RollForCrit(1, 1);
        
        float damage = meleeDamage;
        damage *= 1 + Random.Range(-damageVariance, damageVariance);
        damage *= critMult;

        return damage;
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
                                                        distanceToCube, LayerMask.GetMask("Structures"));
                if (rayHit.collider != null)
                {
                    HitStructure(rayHit.point);
                    return;
                }
            }

            HandleCollision(targetCollider, attackPoint);
        }

        //StartCoroutine(AttackTimer());
    }

    int GetTargetPriority(Collider2D collider)
    {
        if (collider.CompareTag("Player") || collider.CompareTag("PlayerBody"))
            return 1;
        if (collider.CompareTag("QCube"))
            return 2;
        if (collider.gameObject.layer == 12)
            return 3;
        if (collider.CompareTag("Drone"))
        {
            Debug.Log("Enemy found Drone Priority 4");
            return 4;
        }
        
        return int.MaxValue;
    }

    void HandleCollision(Collider2D collider, Vector2 point)
    {
        if (collider.CompareTag("Player") || collider.CompareTag("PlayerBody"))
        {
            Health health = collider.GetComponentInParent<Health>();
            if (health)
                Hit(health, point);
        }
        else if (collider.CompareTag("QCube"))
        {
            Health health = collider.GetComponent<Health>();
            if (health)
                Hit(health, point);
        }
        else if (collider.gameObject.layer == 12) // Structure
        {
            HitStructure(point);
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

    void PlayMeleeSound(Vector2 point)
    {
        SFXManager.Instance.PlaySoundAt(meleeSound, point);;
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
}
