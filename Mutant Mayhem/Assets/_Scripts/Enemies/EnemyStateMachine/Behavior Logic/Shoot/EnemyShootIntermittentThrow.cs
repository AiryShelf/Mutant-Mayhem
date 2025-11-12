using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Shoot-Intermittent Throw", menuName = "Enemy Logic/Shoot Logic/Intermittent Throw")]
// This behavior causes the enemy to shoot intermittently for a randomized amount of time, then return to chase
public class EnemyShootIntermittentThrow : EnemyShootSOBase
{
    [Header("Thrown Object Settings")]
    [SerializeField] string thrownObjectPoolName;  
    [SerializeField] float bulletSpeed = 10f;
    [SerializeField] float curveHeight = 2f;
    [SerializeField] float peakScale = 1.2f;
    [SerializeField] float maxRange = 10f;


    [Header("Intermittent Shooting")]
    [SerializeField] float timeBetweenShotsMin = 1f;
    [SerializeField] float timeBetweenShotsMax = 2f;
    [SerializeField] float timeTillExitMin = 3f;
    [SerializeField] float timeTillExitMax = 5f;

    float timeBetweenShots;
    float shotTimer;
    Coroutine waitToExitCoroutine;
    Vector2 previousPos;

    public override void Initialize(GameObject gameObject, EnemyBase enemy)
    {
        base.Initialize(gameObject, enemy);
    }

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();

        //Debug.Log("Entering Intermittent Throw State");

        waitToExitCoroutine = enemyBase.StartCoroutine(StateChangeCheck());
        // Shoot next physicsUpdate
        timeBetweenShots = Random.Range(timeBetweenShotsMin, timeBetweenShotsMax);
        shotTimer = 0f;
        previousPos = enemyBase.targetPos;
    }
    public override void DoExitLogic() 
    {
        base.DoExitLogic();

        enemyBase.StopCoroutine(waitToExitCoroutine);
    }
    public override void DoFrameUpdateLogic() 
    { 
        base.DoFrameUpdateLogic();
    }

    public override void DoPhysicsUpdateLogic()
    {
        base.DoPhysicsUpdateLogic();

        enemyBase.targetPos = enemyBase.targetTransform != null ? (Vector2)enemyBase.targetTransform.position : enemyBase.targetPos;

        float pathLength = GameTools.EstimateParabolaArcLength(enemyBase.transform.position, enemyBase.targetPos, curveHeight);
        float leadTime = pathLength / bulletSpeed;
        Vector2 predictedPos = GameTools.GetPredictedPosition(previousPos,
                                                enemyBase.targetPos - previousPos, leadTime);
        Vector2 dir = (predictedPos - (Vector2)enemyBase.transform.position).normalized;
        enemyBase.ChangeFacingDirection(dir, rotateSpeedMultiplier);

        shotTimer += Time.fixedDeltaTime;
        if (shotTimer > timeBetweenShots)
        {
            shotTimer = 0f;
            ThrowObject(predictedPos);
        }

        if (enemyBase.targetTransform != null)
            previousPos = enemyBase.targetTransform.position;
        else
            previousPos = enemyBase.targetPos;
    }
    
    public override void DoAnimationTriggerEventLogic(EnemyBase.AnimationTriggerType triggerType) 
    {
        base.DoAnimationTriggerEventLogic(triggerType);
    }
    
    public override void ResetValues() 
    {
        base.ResetValues();
    }

    IEnumerator StateChangeCheck()
    {
        float timeTillExit = Random.Range(timeTillExitMin, timeTillExitMax);
        float timer = 0f;
        while (timer < timeTillExit)
        {
            if (timer > minTimeBeforeExit && !enemyBase.IsWithinShootDistance)
            {
                enemyBase.StateMachine.ChangeState(enemyBase.ChaseState);
            }
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        enemyBase.StateMachine.ChangeState(enemyBase.ChaseState);
    }

    void ThrowObject(Vector2 predictedPos)
    {
        EnemyThrownObject thrownObject = PoolManager.Instance.GetFromPool(thrownObjectPoolName).GetComponent<EnemyThrownObject>();
        if (thrownObject == null)
        {
            Debug.LogError("Thrown object pool returned null or non-EnemyThrownObject.");
            return;
        }

        // Apply damage based on Mutant or EnemyBase
        if (enemyMutant != null)
        {
            // Modify thrown object based on mutant genome
            Genome g = enemyMutant.individual.genome;
            thrownObject.damage *= g.headGene.scale * g.bodyGene.scale;
            // Debug.Log($"Mutant throwing object with damage: {thrownObject.damage}");
        }
        else
        {
            thrownObject.damage *= enemyBase.transform.localScale.x * enemyBase.transform.localScale.y;
        }

        thrownObject.transform.position = enemyBase.meleeController.transform.position;
        thrownObject.StartCoroutine(thrownObject.ThrowTowardsTarget(enemyBase.transform.position, predictedPos,
                                                                    bulletSpeed, curveHeight, peakScale, maxRange));
    }
}

