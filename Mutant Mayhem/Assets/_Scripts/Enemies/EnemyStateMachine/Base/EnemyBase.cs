using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour, IDamageable, IEnemyMoveable, ITriggerCheckable
{
    public AnimationControllerEnemy animControllerEnemy;
    public MeleeControllerEnemy meleeController;
    public SpriteRenderer SR;
    public Rigidbody2D RB { get; set; }
    public Vector2 FacingDirection { get; set; }
    public float moveSpeedBase = 1f;
    public float rotateSpeedBase = 3f;

    public bool IsAggroed { get; set; }
    public bool IsWithinMeleeDistance { get; set; }
    public bool IsWithinShootDistance { get; set; }

    #region Randomize Variables

    [Header("Randomize Variables")]
    public bool randomize;
    public float minSize;
    public float randomColorFactor;
    public float gaussMeanSize;
    public float gaussStdDev;

    #endregion 

    #region Health Variables

    [field: Header("Health Variables")]
    [field: SerializeField] public Health health { get; set; }
    public bool isHit { get; set; }
    [field: SerializeField] public float unfreezeTime { get; set; }
    public Coroutine unfreezeAfterTime { get; set; }

    #endregion

    #region State Machine Variables

    [Header("State Machine Logic")]
    [SerializeField] string CurrentSMStateDebug;
    public EnemyStateMachine StateMachine { get; set; }
    public EnemyIdleState IdleState { get; set; }
    public EnemyChaseState ChaseState { get; set; }
    public EnemyShootState ShootState { get; set; }
    public EnemyMeleeState MeleeState { get; set; }

    #endregion

    #region Scriptable Object Logics Setup

    // Reference these in Inspector
    [SerializeField] private EnemyIdleSOBase EnemyIdleSOBase;
    [SerializeField] private EnemyChaseSOBase EnemyChaseSOBase;
    [SerializeField] private EnemyShootSOBase EnemyShootSOBase;
    [SerializeField] private EnemyMeleeSOBase EnemyMeleeSOBase;

    public EnemyIdleSOBase EnemyIdleSOBaseInstance { get; set; }
    public EnemyChaseSOBase EnemyChaseSOBaseInstance { get; set; }
    public EnemyShootSOBase EnemyShootSOBaseInstance { get; set; }
    public EnemyMeleeSOBase EnemyMeleeSOBaseInstance { get; set; }

    #endregion

    void Awake()
    {
        EnemyIdleSOBaseInstance = Instantiate(EnemyIdleSOBase);
        EnemyChaseSOBaseInstance = Instantiate(EnemyChaseSOBase);
        EnemyShootSOBaseInstance = Instantiate(EnemyShootSOBase);
        //EnemyMeleeSOBaseInstance = Instantiate(EnemyMeleeSOBase);

        StateMachine = new EnemyStateMachine();

        IdleState = new EnemyIdleState(this, StateMachine);
        ChaseState = new EnemyChaseState(this, StateMachine);
        ShootState = new EnemyShootState(this, StateMachine);
        //MeleeState = new EnemyMeleeState(this, StateMachine);
    }

    void Start()
    {
        //CurrentHealth = MaxHealth;
        RB = GetComponent<Rigidbody2D>();

        EnemyIdleSOBaseInstance.Initialize(gameObject, this);
        EnemyChaseSOBaseInstance.Initialize(gameObject, this);
        EnemyShootSOBaseInstance.Initialize(gameObject, this);
        //EnemyMeleeSOBaseInstance.Initialize(gameObject, this);

        StateMachine.Initialze(IdleState);

        RandomizeStats();
    }

    void Update()
    {
        StateMachine.CurrentEnemyState.FrameUpdate();
    }
    void FixedUpdate() 
    {
        StateMachine.CurrentEnemyState.PhysicsUpdate();
        CurrentSMStateDebug = StateMachine.CurrentEnemyState.ToString();
    }

    #region Randomize Function

    void RandomizeStats()
    {
        // Randomize color
        float randomColorRed = Random.Range(-randomColorFactor, randomColorFactor);
        float randomColorGreen = Random.Range(-randomColorFactor, randomColorFactor);
        float randomColorBlue = Random.Range(-randomColorFactor, randomColorFactor);
        SR.color = new Color(SR.color.r + randomColorRed,
                               SR.color.g + randomColorGreen,
                               SR.color.b + randomColorBlue);
        
        // Randomize size
        GaussianRandom _gaussianRandomm = new GaussianRandom();
        float randomSizeFactor = (float)_gaussianRandomm.NextDouble(gaussMeanSize, gaussStdDev);
        randomSizeFactor = Mathf.Clamp(randomSizeFactor, minSize, float.MaxValue);
        transform.localScale *= randomSizeFactor;

        // Randomize stats by size
        moveSpeedBase *= randomSizeFactor;
        health.SetMaxHealth(health.GetMaxHealth() * randomSizeFactor);
        health.SetHealth(health.GetMaxHealth());
        meleeController.meleeDamage *= randomSizeFactor;
        meleeController.knockback *= randomSizeFactor;
        //meleeController.selfKnockback *= randomSizeFactor; no good*
        RB.mass *= randomSizeFactor;

        animControllerEnemy.animSpeedFactor /= randomSizeFactor;
    }

    void OnCollisionStay2D(Collision2D other)
    {
        // Structures layer# 12
        if (other.gameObject.layer == 12)
        {
            meleeController.HitStructure(other.GetContact(0).point);
        }
    }

    #endregion

    #region Health / Hit Functions

    public void ModifyHealth(float amount, GameObject gameObject)
    {
        health.ModifyHealth(amount, gameObject);
    }
    public void StartFreeze()
    {
        isHit = true;
        if (unfreezeAfterTime == null)
            unfreezeAfterTime = StartCoroutine(UnfreezeAfterTime());
        else
        {
            StopCoroutine(unfreezeAfterTime);
            unfreezeAfterTime = StartCoroutine(UnfreezeAfterTime());
        }
    }

    public IEnumerator UnfreezeAfterTime()
    {
        yield return new WaitForSeconds(unfreezeTime);
        isHit = false;
    }

    public void Knockback(Vector2 dir, float knockback)
    {
        health.Knockback(dir, knockback);
    }

    public void BulletHitEffect(Vector2 hitPos, Vector2 hitDir)
    {
        health.BulletHitEffect(hitPos, hitDir);
    }

    public void MeleeHitEffect(Vector2 hitPos, Vector2 hitDir)
    {
        health.MeleeHitEffect(hitPos, hitDir);
    }

    public void Die()
    {
        health.Die();
    }

    #endregion

    #region Movement Functions

    public void MoveEnemy(Vector2 velocity)
    {
        //Debug.Log("checking isSprinting: " + EnemyChaseSOBaseInstance.isSprinting);
        if (!isHit)
        {
            if (EnemyChaseSOBaseInstance.isSprinting)
                RB.AddForce(moveSpeedBase * velocity); 
            else 
            {
                Vector2 force = moveSpeedBase * velocity;
                Vector2 acc = force / RB.mass;
                Vector2 deltaV = acc * Time.fixedDeltaTime;
                RB.velocity += deltaV;  
            }
        }

        
    }

    public void ChangeFacingDirection(Vector2 velocity, float speedMultipliers)
    {
        if (!isHit)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            Vector3 rotator = new Vector3(transform.rotation.x, transform.rotation.y,
                                    Mathf.LerpAngle(RB.rotation, angle, 
                                    Time.fixedDeltaTime * rotateSpeedBase * speedMultipliers));
            transform.rotation = Quaternion.Euler(rotator);
            FacingDirection = velocity.normalized;
        }
    }

    #endregion

    #region Status Checks

    public void SetAggroToPlayerStatus(bool isAggroed)
    {
        IsAggroed = isAggroed;
    }

    public void SetMeleeDistanceBool(bool isWithinMeleeDistance)
    {
        IsWithinMeleeDistance = isWithinMeleeDistance;
    }

    public void SetShootDistanceBool(bool isWithinShootDistance)
    {
        IsWithinShootDistance = isWithinShootDistance;
    }

    #endregion

    #region Animation Triggers

    private void AnimationTriggerEvent(AnimationTriggerType triggerType)
    {
        StateMachine.CurrentEnemyState.AnimationTriggerEvent(triggerType);
    }

    public enum AnimationTriggerType
    {
        EnemyDamaged,
        PlayFootstepSound
    }

    #endregion    
}