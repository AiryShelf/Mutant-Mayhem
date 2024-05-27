using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleSOBase : ScriptableObject
{
    [SerializeField] protected float rotateSpeedMultiplier = 0.5f;
    protected EnemyBase enemyBase;
    protected Transform transform;
    protected GameObject gameObject;

    protected Transform playerTransform;

    public virtual void Initialize(GameObject gameObject, EnemyBase enemyBase)
    {
        this.gameObject = gameObject;
        transform = gameObject.transform;
        this.enemyBase = enemyBase;

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public virtual void DoEnterLogic() { }
    public virtual void DoExitLogic() { }
    public virtual void DoFrameUpdateLogic() 
    { 
        if (enemyBase.IsAggroed)
        {
            enemyBase.StateMachine.ChangeState(enemyBase.ChaseState);
        }
    }
    public virtual void DoPhysicsUpdateLogic() { }
    public virtual void DoAnimationTriggerEventLogic(EnemyBase.AnimationTriggerType triggerType) { }
    public virtual void ResetValues() { }
}
