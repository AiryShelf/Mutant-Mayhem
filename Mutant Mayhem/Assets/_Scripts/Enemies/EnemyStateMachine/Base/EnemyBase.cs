using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour, IDamageable, IFreezable, IEnemyMoveable, ITriggerCheckable
{
    public string objectPoolName;
    public AnimationControllerEnemy animControllerEnemy;
    public MeleeControllerEnemy meleeController;
    public SpriteRenderer sr;
    public Rigidbody2D rb { get; set; }
    public Vector2 facingDirection { get; set; }

    [Header("Movement")]
    public float moveSpeedBaseStart;
    public float moveSpeedBase = 1f;
    float slowFactor = 1;
    public float rotateSpeedBaseStart = 3f;
    public float rotateSpeedBase = 3f;
    public float startMass; // For debug, don't set

    [Header("Randomize")]
    public bool randomize;
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
    public EnemyMeleeState MeleeState { get; set; }

    public bool IsAggroed { get; set; }
    public bool IsShotAggroed { get; set; }
    public bool IsWithinMeleeDistance { get; set; }
    public bool IsWithinShootDistance { get; set; }

    #endregion

    #region SO Logic Setup

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

    #region Initialize / Reset

    WaveControllerRandom waveController;

    void Awake()
    {
        waveController = FindObjectOfType<WaveControllerRandom>();

        // Logic machine linked to state machine
        EnemyIdleSOBaseInstance = Instantiate(EnemyIdleSOBase);
        EnemyChaseSOBaseInstance = Instantiate(EnemyChaseSOBase);
        EnemyShootSOBaseInstance = Instantiate(EnemyShootSOBase);
        //EnemyMeleeSOBaseInstance = Instantiate(EnemyMeleeSOBase);

        StateMachine = new EnemyStateMachine();
        IdleState = new EnemyIdleState(this, StateMachine);
        ChaseState = new EnemyChaseState(this, StateMachine);
        ShootState = new EnemyShootState(this, StateMachine);
        //MeleeState = new EnemyMeleeState(this, StateMachine);

        rb = GetComponent<Rigidbody2D>();

        startMass = rb.mass;
        startLocalScale = transform.localScale;
        moveSpeedBaseStart = moveSpeedBase;

        StateMachine.Initialize(IdleState);
    }

    void OnDisable() 
    {
        StopAllCoroutines();
    }

    void Start()
    {
        EnemyIdleSOBaseInstance.Initialize(gameObject, this);
        EnemyChaseSOBaseInstance.Initialize(gameObject, this);
        EnemyShootSOBaseInstance.Initialize(gameObject, this);
        //EnemyMeleeSOBaseInstance.Initialize(gameObject, this);
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
        RandomizeStats();
    }

    #endregion

    #region Randomize

    void RandomizeStats()
    {
        //Debug.Log($"Randomize stats started with {health.GetHealth()} health and {health.GetMaxHealth()} maxHealth");
        
        // Randomize speed
        moveSpeedBase *= Random.Range(1 - randSpeedRange, 1 + randSpeedRange);

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
        
        // Randomize size, apply multipliers
        GaussianRandom _gaussianRandom = new GaussianRandom();
        float randomSizeFactor = (float)_gaussianRandom.NextDouble(gaussMeanSize, gaussStdDev);
        randomSizeFactor *= waveController.sizeMultiplier;
        randomSizeFactor = Mathf.Clamp(randomSizeFactor, minSize, float.MaxValue);
        transform.localScale *= randomSizeFactor;

        // Set these stats by random size factor (negates planet property size multiplier from health)
        moveSpeedBase *= randomSizeFactor * waveController.speedMultiplier;
        health.SetMaxHealth(randomSizeFactor / PlanetManager.Instance.statMultipliers[PlanetStatModifier.EnemySize] * 
                            health.startMaxHealth * waveController.healthMultiplier);
        health.SetHealth(health.GetMaxHealth());
        Debug.Log("RandomSizeFacto: " + randomSizeFactor + ", waveController.damageMultiplier: " + waveController.damageMultiplier);
        meleeController.meleeDamage *= randomSizeFactor * waveController.damageMultiplier;
        meleeController.attackDelay = meleeController.attackDelayStart * waveController.attackDelayMult;
        meleeController.knockback *= randomSizeFactor;
        //meleeController.selfKnockback *= randomSizeFactor; no good?
        rb.mass = startMass * randomSizeFactor;
        animControllerEnemy.animSpeedFactor /= randomSizeFactor;

        //Debug.Log($"Randomize stats finished with {health.GetHealth()} health and {health.GetMaxHealth()} maxHealth");
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
        health.Knockback(dir, knockback);
    }

    public void Die()
    {
        PoolManager.Instance.ReturnToPool(objectPoolName, gameObject);
    }

    #endregion

    #region Movement

    public void MoveEnemy(Vector2 velocity)
    {
        //Debug.Log("checking isSprinting: " + EnemyChaseSOBaseInstance.isSprinting);
        if (!isHit)
        {
            if (EnemyChaseSOBaseInstance.isSprinting)
                rb.AddForce(moveSpeedBase * slowFactor * velocity); 
            else 
            {
                Vector2 force = moveSpeedBase * slowFactor * velocity;
                Vector2 acc = force / rb.mass;
                Vector2 deltaV = acc * Time.fixedDeltaTime;
                rb.velocity += deltaV;  
            }
        }  
    }

    public void ChangeFacingDirection(Vector2 velocity, float speedMultipliers)
    {
        if (!isHit)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            Vector3 rotator = new Vector3(transform.rotation.x, transform.rotation.y,
                                          Mathf.LerpAngle(rb.rotation, angle, 
                                          Time.fixedDeltaTime * rotateSpeedBase * speedMultipliers));
            transform.rotation = Quaternion.Euler(rotator);
            facingDirection = transform.right;
        }
    }

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