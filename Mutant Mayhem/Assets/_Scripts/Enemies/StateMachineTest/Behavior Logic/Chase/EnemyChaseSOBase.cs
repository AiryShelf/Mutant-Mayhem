using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChaseSOBase : ScriptableObject
{
    [SerializeField] protected float rotateSpeed = 10f;

    protected EnemyBase enemyBase;
    protected Transform transform;
    protected GameObject gameObject;

    #region Sprint Variables

    protected Coroutine distanceCheck;
    protected Coroutine accelerate;
    [SerializeField] protected float _sprintFactor = 1f;
    [SerializeField] protected float TimeToCheckDistance = 0.3f;
    [SerializeField] protected float DistToStartSprint = 8f;
    [SerializeField] protected float SprintSpeedMultiplier = 2f;
    [SerializeField] protected float TimeToFullSprint = 1f;
    [SerializeField] protected float TimeToStopSprint = 3f;
    protected Coroutine stopSprint;
    public bool isSprinting = false;

    #endregion
    
    protected Transform playerTransform;

    public virtual void Initialize(GameObject gameObject, EnemyBase enemyBase)
    {
        this.gameObject = gameObject;
        transform = gameObject.transform;
        this.enemyBase = enemyBase;

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public virtual void DoEnterLogic() 
    {
        distanceCheck = enemyBase.StartCoroutine(
            DistanceToSprintCheck(DistToStartSprint, SprintSpeedMultiplier, TimeToCheckDistance));
    }
    public virtual void DoExitLogic() { }
    public virtual void DoFrameUpdateLogic() { }
    public virtual void DoPhysicsUpdateLogic() 
    {
        if (enemyBase.IsWithinShootDistance)
        {
            enemyBase.StateMachine.ChangeState(enemyBase.ShootState);
        }


    }
    public virtual void DoAnimationTriggerEventLogic(EnemyBase.AnimationTriggerType triggerType) { }
    public virtual void ResetValues() { }

    public IEnumerator DistanceToSprintCheck(float DistToStartSprint, 
        float SprintSpeedMultiplier, float TimeToCheckDistance)
    {
        while (true)
        {   
            if (enemyBase.IsAggroed)
            {
                // if in sprint range. sqrMagnitude for efficiency
                if ((playerTransform.position - transform.position).sqrMagnitude 
                    < DistToStartSprint*DistToStartSprint)
                {
                    // Sprint
                    if (accelerate == null && _sprintFactor < SprintSpeedMultiplier - 0.01f)
                    {
                        isSprinting = true;
                        if (stopSprint != null)
                        {
                            enemyBase.StopCoroutine(stopSprint);
                        }

                        stopSprint = enemyBase.StartCoroutine(StopSprint());
                        accelerate = enemyBase.StartCoroutine(
                                    LerpSpeed(_sprintFactor, SprintSpeedMultiplier, TimeToFullSprint));
                    }
                }
                else
                {
                    isSprinting = false;
                    // No sprint
                    if (accelerate == null && stopSprint == null && _sprintFactor > 1.01f)
                        accelerate = enemyBase.StartCoroutine(
                                    LerpSpeed(_sprintFactor, 1, TimeToFullSprint));
                }

                //Debug.Log("sqrMagnitude: " + (playerTransform.position 
                //                              - transform.position).sqrMagnitude);
            }    
            yield return new WaitForSeconds(TimeToCheckDistance);
            //Debug.Log("Enemy isAggroed.  sprintFactor: " + _sprintFactor);
        }
    }

    public void StartSprint()
    {
        isSprinting = true;
        if (accelerate != null)
        {
            enemyBase.StopCoroutine(accelerate);
        }
        if (stopSprint != null)
        {
            enemyBase.StopCoroutine(stopSprint);
        }
        stopSprint = enemyBase.StartCoroutine(StopSprint());
        accelerate = enemyBase.StartCoroutine(
                                LerpSpeed(_sprintFactor, SprintSpeedMultiplier, TimeToFullSprint));
    }

    public IEnumerator StopSprint()
    {
        yield return new WaitForSeconds(TimeToStopSprint);
        stopSprint = null;
    }

    public IEnumerator LerpSpeed(float start, float end, float TimeToFullSprint)
    {
        bool increase;
        float timeElapsed = Time.deltaTime;

        if (start < end)
            increase = true;
        else
            increase = false;

        while (timeElapsed < TimeToFullSprint)
        {
            _sprintFactor = Mathf.Lerp(start, end, timeElapsed / TimeToFullSprint);
            if (increase)
                _sprintFactor = Mathf.Clamp(_sprintFactor, start, end);
            else
                _sprintFactor = Mathf.Clamp(_sprintFactor, end, start);

            yield return new WaitForSeconds(0.1f);
            
            timeElapsed += Time.deltaTime * 1/0.1f;
        }

        accelerate = null;
    }

}
