using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyState
{
    // Protect acts like private from outside classes, 
    // but is public to classes derived from this
    protected EnemyBase enemyBase;
    protected EnemyStateMachine enemyStateMachine;

    // Constructor
    public EnemyState(EnemyBase enemyBase, EnemyStateMachine enemyStateMachine)
    {
        this.enemyBase = enemyBase;
        this.enemyStateMachine = enemyStateMachine;
    }

    // Virtual means we can override in an actual state later on.  
    // Not forced to override like with abstract
    public virtual void EnterState() { }
    public virtual void ExitState() { }
    public virtual void FrameUpdate() { }
    public virtual void PhysicsUpdate() { }
    public virtual void AnimationTriggerEvent(EnemyBase.AnimationTriggerType triggerType) { }
}
