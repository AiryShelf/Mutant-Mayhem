using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShootSOBase : ScriptableObject
{
    public float distanceToStartShooting = 6f;
    [Range(0, 0.1f)]
    public float chanceToStartShootingPerFrame = 0.01f;
    [SerializeField] protected float rotateSpeedMultiplier = 1.5f;
    [SerializeField] protected float minTimeBeforeExit = 2f;
    protected EnemyBase enemyBase;
    protected Transform transform;
    protected GameObject gameObject;
    protected EnemyMutant enemyMutant = null;

    public virtual void Initialize(GameObject gameObject, EnemyBase enemyBase)
    {
        this.gameObject = gameObject;
        transform = gameObject.transform;
        
        this.enemyBase = enemyBase;
        if (enemyBase is EnemyMutant)
        {
            enemyMutant = enemyBase as EnemyMutant;
        }
        else
        {
            enemyMutant = null;
        }
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
