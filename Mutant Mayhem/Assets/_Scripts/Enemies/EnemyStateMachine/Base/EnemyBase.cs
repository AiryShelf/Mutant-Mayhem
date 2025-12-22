using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour, IDamageable, IFreezable, IEnemyMoveable, ITriggerCheckable
{
    [Header("Enemy Base")]
    public string objectPoolName;
    public AnimationControllerEnemy animControllerEnemy;
    public MeleeControllerEnemy meleeController;
    public SpriteRenderer sr;
    public Rigidbody2D rb { get; set; }
    public Vector2 facingDirection { get; set; }

    [Header("EnemyBase ONLY Melee Stat Setters")]
    public float meleeDamageMaster = 1f;
    public float knockbackMaster = 1f;

    [Header("Exposed for Debug")]
    public Transform targetTransform;
    public Vector2 targetPos;

    [Header("Movement")]
    public float moveSpeedBaseStart;
    public float moveSpeedBase = 1f;
    float slowFactor = 1;
    public float rotateSpeedBase = 3f;
    protected float startMass;
    float startAnimSpeedFactor;
    float startSwitchToRunBuffer;

    [Header("Randomize")]
    public bool isMutant;
    public float randSpeedRange = 0.1f;
    public Vector3 startLocalScale;
    public float minSize;
    public float randColorRange;
    public float gaussMeanSize;
    public float gaussStdDev;

    [field: Header("Health")]
    [field: SerializeField] public Health health { get; set; }
    public bool isHit { get; set; }
    [field: SerializeField] public float unfreezeTime { get; set; }
    public Coroutine unfreezeAfterTime { get; set; }


    #region State Machine

    [Header("State Machine Logic")]
    public string CurrentStateDebug;
    public EnemyStateMachine StateMachine { get; set; }
    public EnemyIdleState IdleState { get; set; }
    public EnemyChaseState ChaseState { get; set; }
    public EnemyShootState ShootState { get; set; }

    public bool IsAggroed { get; set; }
    public bool IsShotAggroed { get; set; }
    public bool IsWithinMeleeDistance { get; set; }
    public bool IsWithinShootDistance { get; set; }

    #endregion

    #region SO Logic

    // Reference these in Inspector
    [SerializeField] private EnemyIdleSOBase EnemyIdleSOBase;
    [SerializeField] private EnemyChaseSOBase EnemyChaseSOBase;
    [SerializeField] private EnemyShootSOBase EnemyShootSOBase;
    [SerializeField] private EnemyMeleeSOBase EnemyMeleeSOBase;

    // Is set during runtime, exposed only for debugging
    public EnemyIdleSOBase EnemyIdleSOBaseInstance;
    public EnemyChaseSOBase EnemyChaseSOBaseInstance;
    public EnemyShootSOBase EnemyShootSOBaseInstance;
    public EnemyMeleeSOBase EnemyMeleeSOBaseInstance;

    #endregion

    #region Initialize / Reset

    protected WaveController waveController;

    public virtual void Awake()
    {
        waveController = FindObjectOfType<WaveController>();

        rb = GetComponent<Rigidbody2D>();

        startMass = rb.mass;
        startLocalScale = transform.localScale;
        moveSpeedBaseStart = moveSpeedBase;

        meleeController.meleeDamageStart = meleeDamageMaster;
        meleeController.meleeDamage = meleeDamageMaster;
        meleeController.knockbackStart = knockbackMaster;
        meleeController.knockback = knockbackMaster;

        startAnimSpeedFactor = animControllerEnemy.animSpeedFactor;
        startSwitchToRunBuffer = animControllerEnemy.switchToRunBuffer;

        InitializeStateMachine();
    }

    void OnDisable() 
    {
        StopAllCoroutines();
    }

    void Start()
    {
        if (!isMutant) InitializeSOLogic(); 
    }

    void Update()
    {
        StateMachine.CurrentEnemyState.FrameUpdate();
    }

    void FixedUpdate() 
    {
        StateMachine.CurrentEnemyState.PhysicsUpdate();
        // For SM debug
        //CurrentStateDebug = StateMachine.CurrentEnemyState.ToString();
    }
    
    public Rigidbody2D GetRigidbody()
    {
        return rb;
    }

    public void ResetStats()
    {
        isHit = false;
        health.hasDied = false;
        health.SetMaxHealth(health.startMaxHealth);
        health.SetHealth(health.GetMaxHealth());

        moveSpeedBase = moveSpeedBaseStart;
        rb.mass = startMass;
        transform.localScale = startLocalScale;

        meleeController.Reset();
        StateMachine.ChangeState(IdleState);

        if (!isMutant)
        {
            RandomizeEnemyBaseStats();
            RandomizeEnemyBaseColor();
        }
    }

    public void InitializeStateMachine()
    {
        // Logic machine linked to state machine
        if (EnemyIdleSOBase != null)
            EnemyIdleSOBaseInstance = Instantiate(EnemyIdleSOBase);
        if (EnemyChaseSOBase != null)
            EnemyChaseSOBaseInstance = Instantiate(EnemyChaseSOBase);
        if (EnemyShootSOBase != null)
            EnemyShootSOBaseInstance = Instantiate(EnemyShootSOBase);

        StateMachine = new EnemyStateMachine();
        IdleState = new EnemyIdleState(this, StateMachine);
        ChaseState = new EnemyChaseState(this, StateMachine);
        ShootState = new EnemyShootState(this, StateMachine);

        StateMachine.Initialize(IdleState);
    }

    public void RestartStateMachine()
    {
        // Optional: Stop any ongoing logic
        StopAllCoroutines();

        // Clean up old SO instances to prevent stale logic when the object is reused from the pool
        if (EnemyIdleSOBaseInstance != null) Destroy(EnemyIdleSOBaseInstance);
        if (EnemyChaseSOBaseInstance != null) Destroy(EnemyChaseSOBaseInstance);
        if (EnemyShootSOBaseInstance != null) Destroy(EnemyShootSOBaseInstance);


        // Optionally null out old state machine and states
        StateMachine = null;
        IdleState = null;
        ChaseState = null;
        ShootState = null;

        // Re-instantiate new ScriptableObject logic
        if (EnemyIdleSOBase != null)
            EnemyIdleSOBaseInstance = Instantiate(EnemyIdleSOBase);
        if (EnemyChaseSOBase != null)
            EnemyChaseSOBaseInstance = Instantiate(EnemyChaseSOBase);
        if (EnemyShootSOBase != null)
            EnemyShootSOBaseInstance = Instantiate(EnemyShootSOBase);


        // Recreate state machine and states
        StateMachine = new EnemyStateMachine();
        IdleState = new EnemyIdleState(this, StateMachine);
        ChaseState = new EnemyChaseState(this, StateMachine);
        ShootState = new EnemyShootState(this, StateMachine);

        StateMachine.Initialize(IdleState);

        // Initialize the SO logic with references to this GameObject
        InitializeSOLogic();
    }

    public void InitializeSOLogic()
    {
        StateMachine.ChangeState(IdleState);

        // Initialize the SO logic for the enemy
        if (EnemyIdleSOBaseInstance != null)
            EnemyIdleSOBaseInstance.Initialize(gameObject, this);
        if (EnemyChaseSOBaseInstance != null)
            EnemyChaseSOBaseInstance.Initialize(gameObject, this);
        if (EnemyShootSOBaseInstance != null)
            EnemyShootSOBaseInstance.Initialize(gameObject, this);
    }

    public void ApplyBehaviourSet(EnemyIdleSOBase idleSO, EnemyChaseSOBase chaseSO, EnemyShootSOBase shootSO)
    {
        EnemyIdleSOBase = idleSO;
        EnemyChaseSOBase = chaseSO;
        EnemyShootSOBase = shootSO;
    }

    #endregion

    #region Randomize

    void RandomizeEnemyBaseStats()
    {
        //Debug.Log($"Randomize stats for {gameObject.name} started with {health.GetHealth()} health and {health.GetMaxHealth()} maxHealth");
        
        // Randomize speed
        moveSpeedBase *= Random.Range(1 - randSpeedRange, 1 + randSpeedRange);
        
        // Randomize size, apply multipliers
        GaussianRandom _gaussianRandom = new GaussianRandom();
        float randomSizeFactor = (float)_gaussianRandom.NextDouble(gaussMeanSize, gaussStdDev);
        randomSizeFactor *= waveController.sizeMultiplier * PlanetManager.Instance.statMultipliers[PlanetStatModifier.EnemySize];
        randomSizeFactor = Mathf.Clamp(randomSizeFactor, minSize, float.MaxValue);
        transform.localScale *= randomSizeFactor;
        //Debug.Log($"EnemyBase - Randomized Size Factor: {randomSizeFactor}, New Local Scale: {transform.localScale}");
        float areaScale = transform.localScale.x * transform.localScale.y;

        // Set these stats by areaScale to keep health and damage proportional to size
        health.SetMaxHealth(areaScale * health.startMaxHealth * waveController.healthMultiplier);
        health.SetHealth(health.GetMaxHealth());

        //Debug.Log("RandomSizeFactor: " + randomSizeFactor + ", waveController.damageMultiplier: " + waveController.damageMultiplier);
        meleeController.meleeDamage = meleeController.meleeDamageStart * areaScale * waveController.damageMultiplier;
        meleeController.attackDelay = meleeController.attackDelayStart * waveController.attackDelayMult;
        meleeController.knockback = meleeController.knockbackStart * areaScale;
        meleeController.selfKnockback = meleeController.selfKnockbackStart * areaScale;
        rb.mass = startMass * areaScale;
        moveSpeedBase = moveSpeedBaseStart * rb.mass * waveController.speedMultiplier;
        animControllerEnemy.animSpeedFactor = animControllerEnemy.animSpeedFactorStart * (8 / transform.localScale.x); // smaller enemies have faster animations
        animControllerEnemy.switchToRunBuffer = animControllerEnemy.switchToRunBufferStart * (transform.localScale.x / 8); // smaller enemies switch to run at lower speeds

        //Debug.Log($"Randomize stats finished with {health.GetHealth()} health and {health.GetMaxHealth()} maxHealth");
    }

    public virtual void RandomizeEnemyBaseColor()
    {
        // Randomize color, allowing it to change more drastically over time
        float red = sr.color.r;
        float green = sr.color.g;
        float blue = sr.color.b;

        red += Random.Range(-randColorRange, randColorRange);
        if (red < 0.15f)
            red += 0.2f;
        green += Random.Range(-randColorRange, randColorRange);
        if (green < 0.15f)
            green += 0.2f;
        blue += Random.Range(-randColorRange, randColorRange);
        if (blue < 0.15f)
            blue += 0.2f;
        sr.color = new Color(red, green, blue);
    }

    #endregion

    #region Health / Hit / Die

    public void ModifyHealth(float amount, float damageScale, Vector2 hitDir, GameObject gameObject)
    {
        health.ModifyHealth(amount, damageScale, hitDir, gameObject);
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
        if (EnemyChaseSOBaseInstance != null && EnemyChaseSOBaseInstance.isInAirJumping)
            EnemyChaseSOBaseInstance.ApplyJumpAirImpulse(dir * knockback);
        else
            health.Knockback(dir, knockback);
    }

    public virtual void Die()
    {
        // Exit state machine
        StateMachine.ChangeState(IdleState);
        PoolManager.Instance.ReturnToPool(objectPoolName, gameObject);
    }

    #endregion

    #region Movement

    public void MoveEnemy(Vector2 velocity)
    {
        //Debug.Log("checking isSprinting: " + EnemyChaseSOBaseInstance.isSprinting);
        if (!isHit)
        {
            rb.AddForce(moveSpeedBase * slowFactor * velocity); 
        }  
    }

    public void ChangeFacingDirection(Vector2 dir, float speedMultipliers)
    {
        if (!isHit)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Vector3 rotator = new Vector3(transform.rotation.x, transform.rotation.y,
                                          Mathf.LerpAngle(rb.rotation, angle, 
                                          Time.fixedDeltaTime * rotateSpeedBase * speedMultipliers));
            transform.rotation = Quaternion.Euler(rotator);
            facingDirection = transform.right;
        }
    }

    // These are used by RazorWire to slow enemies
    public void ApplySlowFactor(float factor)
    {
        slowFactor = Mathf.Clamp(slowFactor - factor, 0.3f, 1);
    }

    public void RemoveSlowFactor(float factor)
    {
        slowFactor = Mathf.Clamp(slowFactor + factor, 0.3f, 1);
    }

    #endregion

    #region Status Checks

    public void SetAggroStatus(bool isAggroed)
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