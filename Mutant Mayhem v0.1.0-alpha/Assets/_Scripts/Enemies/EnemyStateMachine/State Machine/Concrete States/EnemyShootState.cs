using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShootState : EnemyState
{
    

    public EnemyShootState(EnemyBase enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {
        
    }

    public override void EnterState() 
    {
        base.EnterState();

        enemyBase.EnemyShootSOBaseInstance.DoEnterLogic();
    }
    public override void ExitState() 
    {
        base.ExitState();

        enemyBase.EnemyShootSOBaseInstance.DoExitLogic();
    }
    public override void FrameUpdate() 
    {
        base.FrameUpdate();

        enemyBase.EnemyShootSOBaseInstance.DoFrameUpdateLogic();
    }
    public override void PhysicsUpdate() 
    {
        base.PhysicsUpdate();

        enemyBase.EnemyShootSOBaseInstance.DoPhysicsUpdateLogic();
    }
    public override void AnimationTriggerEvent(EnemyBase.AnimationTriggerType triggerType) 
    {
        base.AnimationTriggerEvent(triggerType);

        enemyBase.EnemyShootSOBaseInstance.DoAnimationTriggerEventLogic(triggerType);
    }
}
