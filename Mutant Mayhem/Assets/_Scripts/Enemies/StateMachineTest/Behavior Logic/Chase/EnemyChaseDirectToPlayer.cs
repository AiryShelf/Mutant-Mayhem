using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Chase-Direct Chase Player", menuName = "Enemy Logic/Chase Logic/Direct Chase Player")]
public class EnemyChaseDirectToPlayer : EnemyChaseSOBase
{
    [SerializeField] private float moveSpeedMultiplier = 1.2f;
    [SerializeField] float TimeToCheckDistance = 0.3f;
    [SerializeField] float DistToStartSprint = 8f;
    [SerializeField] float SprintSpeedMultiplier = 2f;
    [SerializeField] float TimeToFullSprint = 1f;

    Coroutine distanceCheck;
    Coroutine accelerate;
    float sprintFactor = 1f;

    public override void Initialize(GameObject gameObject, EnemyBase enemy)
    {
        base.Initialize(gameObject, enemy);
    }  

    public override void DoEnterLogic() 
    {
        base.DoEnterLogic();

        distanceCheck = enemyBase.StartCoroutine(DistanceCheck());
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

        // Move towards player
        Vector2 moveDirection = (playerTransform.position - transform.position).normalized;
        enemyBase.ChangeFacingDirection(moveDirection, rotateSpeed);
        enemyBase.MoveEnemy(enemyBase.FacingDirection * (moveSpeedMultiplier * sprintFactor));
    }

    public override void DoAnimationTriggerEventLogic(EnemyBase.AnimationTriggerType triggerType) 
    {
        base.DoAnimationTriggerEventLogic(triggerType);
    }

    public override void ResetValues() 
    {
        base.ResetValues();
    }

    IEnumerator DistanceCheck()
    {
        while (true)
        {   

            // sqrMagnitude for efficiency             
            if ((playerTransform.position - transform.position).sqrMagnitude 
                < DistToStartSprint*DistToStartSprint)
            {
                // Sprint
                if (accelerate == null && sprintFactor != SprintSpeedMultiplier)
                    accelerate = enemyBase.StartCoroutine(LerpSpeed(sprintFactor, SprintSpeedMultiplier));                   
            }
            else
            {
                // No sprint
                if (accelerate == null && sprintFactor != 1)
                    accelerate = enemyBase.StartCoroutine(LerpSpeed(sprintFactor, 1));
            }

            //Debug.Log("sqrMagnitude: " + (playerTransform.position - transform.position).sqrMagnitude);
            
            yield return new WaitForSeconds(TimeToCheckDistance);
            //Debug.Log("sprintFactor: " + sprintFactor);
        }
    }

    IEnumerator LerpSpeed(float start, float end)
    {
        bool increase;
        float timeElapsed = Time.deltaTime;

        if (start < end)
            increase = true;
        else
            increase = false;

        while (timeElapsed < TimeToFullSprint)
        {
            sprintFactor = Mathf.Lerp(start, end, timeElapsed / TimeToFullSprint);
            if (increase)
                sprintFactor = Mathf.Clamp(sprintFactor, start, end);
            else
                sprintFactor = Mathf.Clamp(sprintFactor, end, start);

            yield return new WaitForSeconds(0.1f);
            
            timeElapsed += Time.deltaTime * 1/0.1f;
        }

        accelerate = null;
    }

}