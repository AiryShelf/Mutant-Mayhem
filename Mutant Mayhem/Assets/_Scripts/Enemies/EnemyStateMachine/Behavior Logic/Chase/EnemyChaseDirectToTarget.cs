using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Chase-Direct Chase Target", 
                 menuName = "Enemy Logic/Chase Logic/Direct Chase Target")]
public class EnemyChaseDirectToTarget : EnemyChaseSOBase
{
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
        base.DoPhysicsUpdateLogic();

        // Move towards target

        Vector2 moveDir = targetPos - transform.position;
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