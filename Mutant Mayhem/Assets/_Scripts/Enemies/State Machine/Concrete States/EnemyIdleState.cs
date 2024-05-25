using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleState : EnemyState
{
    
    public EnemyIdleState(EnemyBase enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {

    }

    public override void EnterState() 
    {
        base.EnterState();

        enemyBase.EnemyIdleSOBaseInstance.DoEnterLogic();
    }
    public override void ExitState() 
    {
        base.ExitState();

        enemyBase.EnemyIdleSOBaseInstance.DoExitLogic();
    }
    public override void FrameUpdate() 
    {
        base.FrameUpdate();

        enemyBase.EnemyIdleSOBaseInstance.DoFrameUpdateLogic();
    }
    public override void PhysicsUpdate() 
    {
        base.PhysicsUpdate();

        enemyBase.EnemyIdleSOBaseInstance.DoPhysicsUpdateLogic();
    }
    public override void AnimationTriggerEvent(EnemyBase.AnimationTriggerType triggerType) 
    {
        base.AnimationTriggerEvent(triggerType);

        enemyBase.EnemyIdleSOBaseInstance.DoAnimationTriggerEventLogic(triggerType);
    }

    
    
}
