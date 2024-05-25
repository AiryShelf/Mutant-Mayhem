using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour, IDamageable, IEnemyMoveable, ITriggerCheckable
{
    [field: SerializeField] public float MaxHealth { get; set; } = 100f;
    public float CurrentHealth { get; set; }
    public Rigidbody2D RB { get; set; }
    public Vector2 FacingDirection { get; set; }
    public float rotateSpeed { get; set; }

    public bool IsAggroed { get; set; }
    public bool IsWithinMeleeDistance { get; set; }
    public bool IsWithinShootDistance { get; set; }

    #region State Machine Variables

    public EnemyStateMachine StateMachine { get; set; }
    public EnemyIdleState IdleState { get; set; }
    public EnemyChaseState ChaseState { get; set; }
    public EnemyShootState ShootState { get; set; }
    public EnemyMeleeState MeleeState { get; set; }

    #endregion

    #region Scriptable Object Logics Setup

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
        EnemyMeleeSOBaseInstance = Instantiate(EnemyMeleeSOBase);

        StateMachine = new EnemyStateMachine();

        IdleState = new EnemyIdleState(this, StateMachine);
        ChaseState = new EnemyChaseState(this, StateMachine);
        ShootState = new EnemyShootState(this, StateMachine);
        MeleeState = new EnemyMeleeState(this, StateMachine);
    }

    void Start()
    {
        CurrentHealth = MaxHealth;
        RB = GetComponent<Rigidbody2D>();

        EnemyIdleSOBaseInstance.Initialize(gameObject, this);
        EnemyChaseSOBaseInstance.Initialize(gameObject, this);
        EnemyShootSOBaseInstance.Initialize(gameObject, this);
        EnemyMeleeSOBaseInstance.Initialize(gameObject, this);

        StateMachine.Initialze(IdleState);
    }

    void Update()
    {
        StateMachine.CurrentEnemyState.FrameUpdate();
    }
    void FixedUpdate() 
    {
        StateMachine.CurrentEnemyState.PhysicsUpdate();
    }

    #region Health / Die Functions

    public void Damage(float damageAmount)
    {
        CurrentHealth -= damageAmount;

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        
    }

    #endregion

    #region Movement Functions

    public void MoveEnemy(Vector2 velocity)
    {
        RB.velocity = velocity;
        CheckFacingDirection(velocity);
    }

    public void CheckFacingDirection(Vector2 velocity)
    {
       float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
       Vector3 rotator = new Vector3(transform.rotation.x, transform.rotation.y,
                         Mathf.LerpAngle(RB.rotation, angle, Time.deltaTime * rotateSpeed));
       transform.rotation = Quaternion.Euler(rotator);
       FacingDirection = velocity.normalized;
    }

    #endregion

    #region Distance Checks

    public void SetAggroStatus(bool isAggroed)
    {
        IsAggroed = isAggroed;
    }

    public void SetMeleeDistanceBool(bool isWithinMeleeDistance)
    {
        IsWithinMeleeDistance = isWithinMeleeDistance;
    }

    public void SetShootingDistanceBool(bool isWithinShootDistance)
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