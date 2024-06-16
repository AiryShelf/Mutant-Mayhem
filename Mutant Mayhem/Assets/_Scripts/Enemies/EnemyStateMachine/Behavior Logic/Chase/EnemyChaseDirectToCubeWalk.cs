using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Chase-Direct Chase Player", 
                 menuName = "Enemy Logic/Chase Logic/Direct Chase Cube Walk")]
public class EnemyChaseDirectToCubeWalk : EnemyChaseSOBase
{
    Transform qCubeTrans;
    [SerializeField] private float moveSpeedMultiplier = 1f;

    public override void Initialize(GameObject gameObject, EnemyBase enemy)
    {
        base.Initialize(gameObject, enemy);
        qCubeTrans = FindObjectOfType<QCubeController>().transform;
    }  

    public override void DoEnterLogic() 
    {
        // Do nothing
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

        // Move towards Q Cube
        Vector2 moveDirection = (qCubeTrans.position - transform.position).normalized;
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