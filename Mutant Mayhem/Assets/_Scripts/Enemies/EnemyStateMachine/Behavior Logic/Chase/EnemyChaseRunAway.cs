using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Chase-Run Away", 
                 menuName = "Enemy Logic/Chase Logic/Run Away")]
public class EnemyChaseRunAway : EnemyChaseSOBase
{
    public override void Initialize(GameObject gameObject, EnemyBase enemy)
    {
        base.Initialize(gameObject, enemy);
    }
    
    [SerializeField] private float runAwaySpeedMultiplier = 1f;

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

        // Run away from Player
        Vector2 targetDir = -(enemyBase.targetTransform.position - transform.position).normalized;
        enemyBase.ChangeFacingDirection(targetDir, rotateSpeedMultiplier);
        enemyBase.MoveEnemy(enemyBase.facingDirection * (runAwaySpeedMultiplier * _sprintFactor));
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
