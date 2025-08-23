using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Chase-Jump Chase Target",
                 menuName = "Enemy Logic/Chase Logic/Jump Chase Target")]

// This behavior makes the enemy chase a target and jump towards it intermittently
public class EnemyChaseJumpToTarget : EnemyChaseSOBase
{
    [SerializeField] private float moveSpeedMult = 1.2f;
    [SerializeField] float distToStopChase = 0.2f;

    [Header("Jump Settings")]
    [SerializeField] float jumpChancePerFixedFrame = 0.001f;
    [SerializeField] float distToNotJump = 1f;
    [SerializeField] float jumpMaxDistance = 5f;
    [SerializeField] float jumpStartDelay = 0.5f;
    [SerializeField] float jumpCurveHeight = 2;
    [SerializeField] float jumpHeightScale = 1.5f;
    [SerializeField] float jumpDuration = 1f;
    [SerializeField] float jumpAccuracyRadius = 2f;
    [SerializeField] float jumpCooldown = 2f;

    float localScaleStart;
    bool isJumping = false;
    bool jumpCoroutineStarted = false;
    bool isInAir = false;
    Coroutine jumpCoroutine;
    float jumpCooldownTimer = 0;

    public override void Initialize(GameObject gameObject, EnemyBase enemy)
    {
        base.Initialize(gameObject, enemy);
    }

    public override void DoEnterLogic()
    {
        base.DoEnterLogic();
        isJumping = false;
        jumpCoroutineStarted = false;
        isInAir = false;
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();

        if (jumpCoroutine != null)
        {
            enemyBase.StopCoroutine(jumpCoroutine);
        }
    }

    public override void DoFrameUpdateLogic()
    {
        base.DoFrameUpdateLogic();
    }

    public override void DoPhysicsUpdateLogic()
    {
        if (isJumping)
        {
            if (!jumpCoroutineStarted)
            {
                jumpCoroutineStarted = true;
                jumpCoroutine = enemyBase.StartCoroutine(JumpTowardsTarget());
            }

            // Does not move if jumping
            return;
        }
        else
        {
            // Jump Cooldown and chance of jumping
            if (jumpCooldownTimer > 0)
            {
                jumpCooldownTimer -= Time.fixedDeltaTime;
            }
            else if (Random.value < jumpChancePerFixedFrame)
            {
                isJumping = true;
            }
        }

        // Skips update logic if jumping
        base.DoPhysicsUpdateLogic();

        // Move towards target
        Vector2 moveDir = targetPos - transform.position;
        if (Mathf.Abs(moveDir.x) < distToStopChase && Mathf.Abs(moveDir.y) < distToStopChase)
        {
            enemyBase.StateMachine.ChangeState(enemyBase.IdleState);
        }
        moveDir = moveDir.normalized;
        enemyBase.ChangeFacingDirection(moveDir, rotateSpeedMultiplier);
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

    #region Jump Logic

[SerializeField] float maxLeadDistance = 12f;     // Safety clamp on how far we can lead

IEnumerator JumpTowardsTarget()
{
    // Sample target position to predict movement
    Vector2 firstPos = targetTransform != null ? (Vector2)targetTransform.position : targetPos;
    float sampledTime = 0f;

    // Switch to sitting animation
    enemyBase.animControllerEnemy.SetSitAnimation(true);

    // CHANGED: replace single WaitForSeconds with per-fixed-step sampling
    while (sampledTime < jumpStartDelay)
    {
        Vector2 lastPos1 = targetTransform != null ? (Vector2)targetTransform.position : targetPos;
        float dt1 = Mathf.Max(sampledTime, 0.0001f);
        Vector2 measuredVel1 = (lastPos1 - firstPos) / dt1;      // units per second

        // We predict where the target will be when we LAND, i.e., jumpDuration seconds later.
        float leadTime1 = jumpDuration;
        Vector2 predictedPos1 = (targetTransform != null ? (Vector2)targetTransform.position : targetPos) + measuredVel1 * leadTime1;
        targetPos = predictedPos1; // Update targetPos to the predicted position

        enemyBase.ChangeFacingDirection(targetPos - transform.position, rotateSpeedMultiplier);

        yield return new WaitForFixedUpdate();
        sampledTime += Time.fixedDeltaTime;
    }

    Vector2 lastPos = targetTransform != null ? (Vector2)targetTransform.position : targetPos;
    float dt = Mathf.Max(sampledTime, 0.0001f);
    Vector2 measuredVel = (lastPos - firstPos) / dt;      // units per second

    // Predict where the target will be when we LAND, i.e., jumpDuration seconds later.
    float leadTime = jumpDuration;
    Vector2 predictedPos = (targetTransform != null ? (Vector2)targetTransform.position : targetPos) + measuredVel * leadTime;

    // Clamp the lead so we don’t overshoot wildly if the target teleports/dashes
    Vector2 nowToPred = predictedPos - (targetTransform != null ? (Vector2)targetTransform.position : targetPos);
    if (nowToPred.sqrMagnitude > maxLeadDistance * maxLeadDistance)
        predictedPos = (targetTransform != null ? (Vector2)targetTransform.position : targetPos) + nowToPred.normalized * maxLeadDistance;


    Vector2 fromSelfToPred = predictedPos - (Vector2)transform.position;

    // If target is too close, exit jump
    if (fromSelfToPred.sqrMagnitude < distToNotJump * distToNotJump)
    {
        isJumping = false;
        jumpCoroutineStarted = false;
        jumpCoroutine = null;
        yield break;
    }

    // Find new position until no obstacle found
    Vector3 startPos = transform.position;
    Vector3 endPos;
    Vector3 randomOffset;
    int iterationCount = 0;
    while (true)
    {
        // Pick a random spot around the predicted landing center
        randomOffset = Random.insideUnitCircle * (jumpAccuracyRadius + iterationCount * 0.5f);
        endPos = predictedPos + (Vector2)randomOffset;

        // If endPos is too far, jump max dist
        if ((endPos - startPos).sqrMagnitude > jumpMaxDistance * jumpMaxDistance)
        {
            Vector3 jumpVector = (endPos - startPos).normalized * jumpMaxDistance; // CHANGED: recompute from endPos
            endPos = startPos + jumpVector;
        }

        // Check for obstacles at endPos  ***** Fix
        Vector3Int gridPos = TileManager.Instance.WorldToGrid(endPos);
        if (TileManager.Instance.ContainsTileKey(gridPos) && TileManager.Instance.GetStructureAt(endPos).structureType == StructureType.ThreeByThreePlatform)
        {
            break;
        }
        if (!TileManager.Instance.ContainsTileKey(gridPos))
            break;

        iterationCount++;
        yield return new WaitForFixedUpdate();
    }

    // Perform the jump arc and scale effect
    isInAir = true;
    enemyBase.meleeController.isElevated = true;
    enemyBase.gameObject.layer = LayerMask.NameToLayer("FlyingEnemies");
    enemyBase.animControllerEnemy.SetSitAnimation(false);
    enemyBase.animControllerEnemy.SetJumpAnimation(true);
    localScaleStart = transform.localScale.x;

    float elapsed = 0;
    while (elapsed < jumpDuration)
    {
        elapsed += Time.fixedDeltaTime;
        float t = elapsed / jumpDuration;

        // Position on parabola
        float x = Mathf.Lerp(startPos.x, endPos.x, t);
        float y = jumpCurveHeight * (t - t * t) + Mathf.Lerp(startPos.y, endPos.y, t);
        transform.position = new Vector3(x, y, transform.position.z);

        // Scale: 1.0 → jumpHeightScale at mid → 1.0
        float jumpScale = 4f * t * (1f - t); // 0→1→0
        float scale = Mathf.Lerp(localScaleStart, jumpHeightScale * localScaleStart, jumpScale);
        transform.localScale = Vector3.one * scale;

        yield return new WaitForFixedUpdate();
    }

    // Reset jump variables
    isJumping = false;
    jumpCoroutineStarted = false;
    jumpCoroutine = null;
    enemyBase.meleeController.isElevated = false;
    enemyBase.gameObject.layer = LayerMask.NameToLayer("Enemies");
    enemyBase.animControllerEnemy.SetJumpAnimation(false);

    // Set cooldown timer
    jumpCooldownTimer = jumpCooldown;
}


    #endregion
}