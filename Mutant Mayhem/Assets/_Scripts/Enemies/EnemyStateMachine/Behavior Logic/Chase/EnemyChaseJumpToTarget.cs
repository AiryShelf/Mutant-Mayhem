using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Chase-Jump Chase Target",
                 menuName = "Enemy Logic/Chase Logic/Jump Chase Target")]

// This behavior makes the enemy chase a target and jump towards it intermittently
public class EnemyChaseJumpToTarget : EnemyChaseSOBase
{
    [Header("Chase Settings")]
    [SerializeField] private float moveSpeedMult = 1.2f;
    [SerializeField] float distToStopChase = 0.2f;

    public override void Initialize(GameObject gameObject, EnemyBase enemy)
    {
        base.Initialize(gameObject, enemy);
    }

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
    }

    public override void DoFrameUpdateLogic()
    {
        base.DoFrameUpdateLogic();
    }

    public override void DoPhysicsUpdateLogic()
    {
        if (isJumping)
        {
            if (!jumpCoroutineStarted)
            {
                jumpCoroutineStarted = true;
                jumpCoroutine = enemyBase.StartCoroutine(JumpTowardsTarget());
            }

            // Does not move if jumping
            return;
        }
        else
        {
            // Jump Cooldown and chance of jumping
            if (jumpCooldownTimer > 0)
            {
                jumpCooldownTimer -= Time.fixedDeltaTime;
            }
            if (Random.value < jumpChancePerFixedFrame)
            {
                isJumping = true;
            }
        }

        // Skips update logic if jumping
        base.DoPhysicsUpdateLogic();

        // Move towards target (Chase logic)
        Vector2 moveDir = enemyBase.targetPos - (Vector2)transform.position;
        if (Mathf.Abs(moveDir.x) < distToStopChase && Mathf.Abs(moveDir.y) < distToStopChase)
        {
            enemyBase.StateMachine.ChangeState(enemyBase.IdleState);
        }
        moveDir = moveDir.normalized;
        enemyBase.ChangeFacingDirection(moveDir, rotateSpeedMultiplier);
        enemyBase.MoveEnemy(enemyBase.facingDirection * (moveSpeedMult * _sprintFactor));

    }

    public override void DoAnimationTriggerEventLogic(EnemyBase.AnimationTriggerType triggerType)
    {
        base.DoAnimationTriggerEventLogic(triggerType);
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }
}