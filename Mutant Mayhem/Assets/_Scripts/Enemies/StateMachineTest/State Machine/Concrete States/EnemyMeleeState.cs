using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMeleeState : EnemyState
{

    public EnemyMeleeState(EnemyBase enemy, EnemyStateMachine enemyStateMachine) : base(enemy, enemyStateMachine)
    {

    }

    public override void EnterState() 
    {
        base.EnterState();

        enemyBase.EnemyMeleeSOBaseInstance.DoEnterLogic();
    }
    public override void ExitState() 
    {
        base.ExitState();

        enemyBase.EnemyMeleeSOBaseInstance.DoExitLogic();
    }
    public override void FrameUpdate() 
    {
        base.FrameUpdate();

        enemyBase.EnemyMeleeSOBaseInstance.DoFrameUpdateLogic();
    }
    public override void PhysicsUpdate() 
    {
        base.PhysicsUpdate();

        enemyBase.EnemyMeleeSOBaseInstance.DoPhysicsUpdateLogic();
    }
    public override void AnimationTriggerEvent(EnemyBase.AnimationTriggerType triggerType) 
    {
        base.AnimationTriggerEvent(triggerType);

        enemyBase.EnemyMeleeSOBaseInstance.DoAnimationTriggerEventLogic(triggerType);
    }
}
