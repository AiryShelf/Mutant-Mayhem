using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Chase-Direct Chase Player", menuName = "Enemy Logic/Chase Logic/Direct Chase Player")]
public class EnemyChaseDirectToPlayer : EnemyChaseSOBase
{
    public override void Initialize(GameObject gameObject, EnemyBase enemy)
    {
        base.Initialize(gameObject, enemy);
    }

    [SerializeField] private float _movementSpeed = 1.75f;

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

        // Move towards player
        Vector2 moveDirection = (playerTransform.position - transform.position).normalized;
        enemyBase.MoveEnemy(moveDirection * _movementSpeed);
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