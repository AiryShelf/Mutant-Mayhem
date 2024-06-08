using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Idle-Move Towards Base", 
                 menuName = "Enemy Logic/Idle Logic/Move Towards Base")]
public class EnemyIdleWanderTowards : EnemyIdleSOBase
{
    [SerializeField] private float RandomMovementSpeedMult = 1f;
    
    private Vector3 _targetPos;
    private Vector3 _direction;

    public override void Initialize(GameObject gameObject, EnemyBase enemy)
    {
        base.Initialize(gameObject, enemy);

        _targetPos = FindObjectOfType<QCubeController>().transform.position;
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

        _direction = (_targetPos - transform.position).normalized;

        enemyBase.ChangeFacingDirection(_direction, rotateSpeedMultiplier);
        enemyBase.MoveEnemy(enemyBase.facingDirection * RandomMovementSpeedMult);
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

