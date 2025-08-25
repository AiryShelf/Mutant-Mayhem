using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Chase-Direct Chase Fly to Target", 
                 menuName = "Enemy Logic/Chase Logic/Direct Chase Fly to Target")]
public class EnemyChaseFlyToTarget : EnemyChaseSOBase
{
    [SerializeField] private float moveSpeedMult = 1.2f;
    [SerializeField] LayerMask hitLayers;

    [Header("Flight Variation:")]
    [SerializeField] float sineAmplitude = 3;
    [SerializeField] float diveDistance = 4f; // Increases rotation speed
    [SerializeField] float diveScaleDivisor = 1.7f;

    float time;
    Vector3 moveDir;
    float sineVal;
    Vector3 perpendicular;
    Vector3 combinedDirection;
    Vector3 wavyDirection;
    float clampFactor;
    float totalRotationSpeed;

    public override void Initialize(GameObject gameObject, EnemyBase enemy)
    {
        base.Initialize(gameObject, enemy);
        localScaleStart = enemy.transform.localScale;
    }  

    public override void DoEnterLogic() 
    {
        base.DoEnterLogic();
        time = 0;
        enemyBase.meleeController.SetContactFilter(hitLayers);
    }

    public override void DoExitLogic() 
    {
        base.DoExitLogic();
        enemyBase.meleeController.ResetContactFilter();
        enemyBase.transform.localScale = localScaleStart;
    }

    public override void DoFrameUpdateLogic() 
    { 
        base.DoFrameUpdateLogic();
    }

    public override void DoPhysicsUpdateLogic() 
    {
        base.DoPhysicsUpdateLogic();

        // Move towards target
        time += Time.fixedDeltaTime;

        moveDir = enemyBase.targetPos - (Vector2)transform.position;
        //if (Mathf.Abs(moveDir.x) < distToStopChase && Mathf.Abs(moveDir.y) < distToStopChase)
        //{
            //enemyBase.StateMachine.ChangeState(enemyBase.IdleState);
        //}

        sineVal = SineCalculator.sine_Freq2_Val;
        //sineVal = Mathf.Sin(Time.time * sineFrequency);
        perpendicular = new Vector3(-moveDir.y, moveDir.x, 0f);
        combinedDirection = moveDir + perpendicular * sineAmplitude * sineVal;
        wavyDirection = combinedDirection != Vector3.zero ? combinedDirection.normalized : Vector3.zero;
        clampFactor = Mathf.Clamp(diveDistance - moveDir.sqrMagnitude / diveDistance, 1, diveDistance);

        enemyBase.transform.localScale = localScaleStart / Mathf.Clamp(clampFactor / diveScaleDivisor, 1, diveScaleDivisor); 
        totalRotationSpeed = rotateSpeedMultiplier * clampFactor;
        //Vector3 perpendicular = new Vector3(-moveDir.y, moveDir.x, 0f);
        //Vector3 wavyDirection = (moveDir + perpendicular * sineAmplitude * Mathf.Sin(Time.time * sineFrequency)).normalized;

        enemyBase.ChangeFacingDirection(wavyDirection, totalRotationSpeed);
        //enemyBase.ChangeFacingDirection(wavyDirection, rotateSpeedMultiplier * Mathf.Clamp((8 - moveDir.magnitude), 1, 8));
        enemyBase.MoveEnemy(enemyBase.facingDirection * (moveSpeedMult * _sprintFactor));
        
    }

    public override void DoAnimationTriggerEventLogic(EnemyBase.AnimationTriggerType triggerType) 
    {
        base.DoAnimationTriggerEventLogic(triggerType);
    }

    public override void ResetValues() 
    {
        base.ResetValues();
    }
}