using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMeleeSOBase : ScriptableObject
{
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
        // Handled by individual
    }
    public virtual void DoPhysicsUpdateLogic() { }
    public virtual void DoAnimationTriggerEventLogic(EnemyBase.AnimationTriggerType triggerType) { }
    public virtual void ResetValues() { }
}
