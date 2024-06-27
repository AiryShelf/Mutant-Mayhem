using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Chase-Direct Chase Player", 
                 menuName = "Enemy Logic/Chase Logic/Direct Chase Player")]
public class EnemyChaseDirectToPlayer : EnemyChaseSOBase
{
    [SerializeField] private float moveSpeedMultiplier = 1.2f;

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

        enemyBase.StopCoroutine(distanceCheck);
    }

    public override void DoFrameUpdateLogic() 
    { 
        base.DoFrameUpdateLogic();
    }

    public override void DoPhysicsUpdateLogic() 
    {
        base.DoPhysicsUpdateLogic();

        // Move towards player
        Vector2 moveDirection = (playerTransform.position - transform.position).normalized;
        enemyBase.ChangeFacingDirection(moveDirection, rotateSpeedMultiplier);
        enemyBase.MoveEnemy(enemyBase.facingDirection * (moveSpeedMultiplier * _sprintFactor));
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