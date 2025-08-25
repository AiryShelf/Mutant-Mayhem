using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChaseState : EnemyState
{
    

    public EnemyChaseState(EnemyBase enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {

    }

    public override void EnterState() 
    {
        base.EnterState();

        enemyBase.EnemyChaseSOBaseInstance?.DoEnterLogic();
    }
    
    public override void ExitState()
    {
        base.ExitState();

        enemyBase.EnemyChaseSOBaseInstance?.DoExitLogic();
    }
    
    public override void FrameUpdate()
    {
        base.FrameUpdate();

        enemyBase.EnemyChaseSOBaseInstance?.DoFrameUpdateLogic();
    }
    
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        enemyBase.EnemyChaseSOBaseInstance?.DoPhysicsUpdateLogic();
    }
    
    public override void AnimationTriggerEvent(EnemyBase.AnimationTriggerType triggerType)
    {
        base.AnimationTriggerEvent(triggerType);

        enemyBase.EnemyChaseSOBaseInstance?.DoAnimationTriggerEventLogic(triggerType);
    }
}
