using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChaseSOBase : ScriptableObject
{
    protected EnemyBase enemyBase;
    protected Transform transform;
    protected GameObject gameObject;

    [Header("Chase Variables")]
    [SerializeField] protected float timeToStopChase = 3;
    [SerializeField] protected float TimeToCheckDistance = 0.3f;
    [SerializeField] protected float rotateSpeedMultiplier = 1.5f;

    [Header("Sprint Variables")]
    [SerializeField] protected float DistToStartSprint = 8f;
    [SerializeField] protected float SprintSpeedMultiplier = 2f;
    [SerializeField] protected float TimeToFullSprint = 1f;
    [SerializeField] protected float TimeToStopSprint = 3f;

    protected Coroutine stopSprint;
    public bool isSprinting = false;
    protected Coroutine distanceCheck;
    protected Coroutine accelerate;
    protected float _sprintFactor = 1f;
    protected float stopTimer;
    
    protected Transform playerTransform;

    public virtual void Initialize(GameObject gameObject, EnemyBase enemyBase)
    {
        this.gameObject = gameObject;
        transform = gameObject.transform;
        this.enemyBase = enemyBase;

        playerTransform = FindObjectOfType<Player>().transform;
    }

    public virtual void DoEnterLogic() 
    {
        enemyBase.IsShotAggroed = false;
        stopTimer = 0;
        distanceCheck = enemyBase.StartCoroutine(DistanceToSprintCheck(
                        DistToStartSprint, SprintSpeedMultiplier, TimeToCheckDistance));
    }
    public virtual void DoExitLogic() { }
    public virtual void DoFrameUpdateLogic() { }
    public virtual void DoPhysicsUpdateLogic() 
    {
        // If aggroed, reset stop chase timer.
        if (enemyBase.IsAggroed)
        {
            stopTimer = 0;
        }
        else 
        {
            stopTimer += Time.fixedDeltaTime;

            if (stopTimer >= timeToStopChase)
            {
                enemyBase.StateMachine.ChangeState(enemyBase.IdleState);
                return;
            }
        }

        if (enemyBase.IsWithinShootDistance)
        {
            enemyBase.StateMachine.ChangeState(enemyBase.ShootState);
        }


    }
    public virtual void DoAnimationTriggerEventLogic(EnemyBase.AnimationTriggerType triggerType) { }
    public virtual void ResetValues() { }

    public IEnumerator DistanceToSprintCheck(float distToStartSprint, 
        float sprintSpeedMultiplier, float timeToCheckDistance)
    {
        while (true)
        {   
            if (enemyBase.IsAggroed)
            {
                // if in sprint range. sqrMagnitude for efficiency
                if ((playerTransform.position - transform.position).sqrMagnitude 
                    < distToStartSprint*distToStartSprint)
                {
                    // Sprint
                    if (accelerate == null && _sprintFactor < sprintSpeedMultiplier - 0.01f)
                    {
                        isSprinting = true;
                        if (stopSprint != null)
                        {
                            enemyBase.StopCoroutine(stopSprint);
                        }

                        stopSprint = enemyBase.StartCoroutine(StopSprint());
                        accelerate = enemyBase.StartCoroutine(
                                    LerpSpeed(_sprintFactor, sprintSpeedMultiplier, TimeToFullSprint));
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
            yield return new WaitForSeconds(timeToCheckDistance);
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

    public IEnumerator LerpSpeed(float start, float end, float timeToFullSprint)
    {
        bool increase;
        float timeElapsed = Time.deltaTime;

        if (start < end)
            increase = true;
        else
            increase = false;

        while (timeElapsed < timeToFullSprint)
        {
            _sprintFactor = Mathf.Lerp(start, end, timeElapsed / timeToFullSprint);
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
