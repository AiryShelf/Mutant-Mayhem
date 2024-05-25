using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Idle-Random Wander", menuName = "Enemy Logic/Idle Logic/Random Wander")]
public class EnemyIdleRandomWander : EnemyIdleSOBase
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

        _direction = (_targetPos - transform.position).normalized;

        enemyBase.MoveEnemy(_direction * RandomMovementSpeed);

        if ((enemyBase.transform.position - _targetPos).sqrMagnitude < 0.01f)
        {
            _targetPos = GetRandomPointInCircle();
        }
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

    private Vector3 GetRandomPointInCircle()
    {
        return enemyBase.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * RandomMovementRange;
    }
}

