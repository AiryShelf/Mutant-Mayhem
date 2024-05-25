using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Chase-Run Away", menuName = "Enemy Logic/Chase Logic/Run Away")]
public class EnemyChaseRunAway : EnemyChaseSOBase
{
    public override void Initialize(GameObject gameObject, EnemyBase enemy)
    {
        base.Initialize(gameObject, enemy);
    }
    
    [SerializeField] private float _runAwaySpeed = 1f;

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

        Vector2 runDir = -(playerTransform.position - transform.position).normalized;
        enemyBase.MoveEnemy(runDir * _runAwaySpeed);
    }

    public override void DoPhysicsUpdateLogic() 
    {
        base.DoPhysicsUpdateLogic();
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
