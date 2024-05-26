using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Idle-Random Wander Free", menuName = "Enemy Logic/Idle Logic/Random Wander Free")]
public class EnemyIdleRandomWanderFree : EnemyIdleSOBase
{
    [SerializeField] private float RandomMovementRange = 5f;
    [SerializeField] private float RandomMovementSpeed = 1f;
    
    private Vector3 _targetPos;
    private Vector3 _direction;

    public override void Initialize(GameObject gameObject, EnemyBase enemy)
    {
        base.Initialize(gameObject, enemy);
    }

    public override void DoEnterLogic() 
    {
        base.DoEnterLogic();

        _targetPos = GetRandomPointInCircle();
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

        enemyBase.ChangeFacingDirection(_direction, rotateSpeed);
        enemyBase.MoveEnemy(enemyBase.FacingDirection * RandomMovementSpeed);
        

        if ((enemyBase.transform.position - _targetPos).sqrMagnitude < 0.01f)
        {
            _targetPos = GetRandomPointInCircle();
        }
    }

    public override void DoAnimationTriggerEventLogic(EnemyBase.AnimationTriggerType triggerType) 
    {
        base.DoAnimationTriggerEventLogic(triggerType);
    }

    public override void ResetValues() 
    {
        base.ResetValues();
    }

    private Vector3 GetRandomPointInCircle()
    {
        return (Vector3)UnityEngine.Random.insideUnitCircle * RandomMovementRange;
    }
}

