using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Scripting.APIUpdating;

[CreateAssetMenu(fileName = "Idle-Fly Towards Player", 
                 menuName = "Enemy Logic/Idle Logic/Fly Towards Player")]
public class EnemyIdleFlyTowardsCube : EnemyIdleSOBase
{
    [SerializeField] private float movementSpeedMult = 1f;

    [Header("Flight Variation:")]
    [SerializeField] float sineAmplitude = 3;
    [SerializeField] float sineFrequency = 2;
    [SerializeField] float diveDistance = 4;
    [SerializeField] float diveScaleDivisor = 1.6f;
    float time;
    
    private Vector3 _targetPos;
    private Vector3 moveDir;
    float sineVal;
    Vector3 perpendicular;
    Vector3 combinedDirection;
    Vector3 wavyDirection;
    float clampFactor;
    float totalRotationSpeed;
    Vector3 localScaleStart;

    public override void Initialize(GameObject gameObject, EnemyBase enemy)
    {
        base.Initialize(gameObject, enemy);

        _targetPos = FindObjectOfType<QCubeController>().transform.position;
        localScaleStart = enemy.transform.localScale;
    }

    public override void DoEnterLogic() 
    {
        base.DoEnterLogic();
        time = 0;
    }

    public override void DoExitLogic() 
    {
        base.DoExitLogic();
    }

    public override void DoFrameUpdateLogic() 
    { 
        base.DoFrameUpdateLogic();
    }

    public override void DoPhysicsUpdateLogic() 
    {
        base.DoPhysicsUpdateLogic();
        time += Time.fixedDeltaTime;

        moveDir = _targetPos - transform.position;

        sineVal = Mathf.Sin(time * sineFrequency);
        perpendicular = new Vector3(-moveDir.y, moveDir.x, 0f);
        combinedDirection = moveDir + perpendicular * sineAmplitude * sineVal;
        wavyDirection = combinedDirection != Vector3.zero ? combinedDirection.normalized : Vector3.zero;
        clampFactor = Mathf.Clamp(diveDistance - moveDir.sqrMagnitude / diveDistance, 1, diveDistance);

        enemyBase.transform.localScale = localScaleStart / Mathf.Clamp(clampFactor / diveScaleDivisor, 1, diveScaleDivisor);
        totalRotationSpeed = rotateSpeedMultiplier * clampFactor;
        enemyBase.ChangeFacingDirection(wavyDirection, totalRotationSpeed);

        //Vector3 perpendicular = new Vector3(-moveDir.y, moveDir.x, 0f);
        //Vector3 wavyDirection = (moveDir + perpendicular * sineAmplitude * Mathf.Sin(time * sineFrequency)).normalized;

        // Rotate faster when moving slower
        //enemyBase.ChangeFacingDirection(wavyDirection, rotateSpeedMultiplier * (enemyBase.moveSpeedBase - enemyBase.rb.velocity.magnitude + 1));
        enemyBase.MoveEnemy(enemyBase.facingDirection * movementSpeedMult);
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

