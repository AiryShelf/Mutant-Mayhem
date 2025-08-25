using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChaseSOBase : ScriptableObject
{
    protected EnemyBase enemyBase;
    protected Transform transform;
    protected GameObject gameObject;
    public bool canExit = true;         // Used to lock in Chase state until actions complete, ie. jumping

    [SerializeField] protected float distanceToStartShooting = 0f;
    [Range(0, 0.1f)]
    [SerializeField] protected float chanceToStartShootingPerFrame = 0.01f;

    [Header("Chase Settings")]
    [SerializeField] protected float timeToStopChase = 3;           // Time to stop chasing after losing the target
    [SerializeField] protected float TimeToCheckDistance = 0.3f;    // Time between distance checks
    [SerializeField] protected float rotateSpeedMultiplier = 1.5f;

    [Header("Sprint Settings")]
    [SerializeField] protected float DistToStartSprint = 8f;        // Distance to target to trigger sprint
    [SerializeField] protected float SprintSpeedMultiplier = 2f;    // Multiplies moveSpeed during sprint
    [SerializeField] protected float TimeToFullSprint = 1f;         // Time it takes to reach full sprint
    [SerializeField] protected float TotalSprintTime = 3f;          // Time spent sprinting
    protected Coroutine stopSprint;
    public bool isSprinting = false;
    protected Coroutine distanceCheck;
    protected Coroutine accelerate;
    protected float _sprintFactor = 1f;
    protected float stopTimer;

    [Header("Jump Settings")] // Triggered by child classes
    [Range(0, .1f)]
    [SerializeField] protected float jumpChancePerFixedFrame = 0.001f;
    [SerializeField] float jumpStartDelay = 0.75f;      // Time to track target before Jump
    [SerializeField] float freezeBeforeJump = 0.2f;     // Time to freeze before jump
    [SerializeField] float distToNotJump = 1f;
    [SerializeField] float jumpMaxDistance = 5f;
    [SerializeField] float maxLeadDistance = 12f;       // Safety clamp on how far we can lead
    [SerializeField] float jumpCurveHeight = 2;
    [SerializeField] float jumpHeightScale = 1.5f;
    [SerializeField] float jumpDuration = 1f;
    [SerializeField] float jumpAccuracyRadius = 2f;
    [SerializeField] float jumpCooldown = 2f;
    protected bool isJumping = false;
    protected bool jumpCoroutineStarted = false;
    protected float jumpCooldownTimer = 0;
    protected Coroutine jumpCoroutine;
    protected Vector2 localScaleStart;

    [Header("Airborne Knockback Settings")]
    public bool isInAir = false;                        // True while in jump arc
    [SerializeField] float airMassMult = 0.5f;          // Multiplies mass while airborne
    [SerializeField] float airDrag = 3.0f;              // Higher = fades knockback faster
    [SerializeField] float airMaxSpeed = 12.0f;         // Safety cap on how fast the offset can move

    // Runtime state during jump
    protected Vector2 airVel;     // velocity of the additive offset (m/s)
    protected Vector2 airOffset;  // integrated offset added to the jump parabola

    #region State Machine

    public virtual void Initialize(GameObject gameObject, EnemyBase enemyBase)
    {
        this.gameObject = gameObject;
        transform = gameObject.transform;
        this.enemyBase = enemyBase;
    }

    public virtual void DoEnterLogic()
    {
        isJumping = false;
        jumpCoroutineStarted = false;
        canExit = true;
        isInAir = false;
        
        if (enemyBase.EnemyShootSOBaseInstance != null)
            distanceToStartShooting = enemyBase.EnemyShootSOBaseInstance.distanceToStartShooting;

        // Set targetPos
            if (enemyBase.targetTransform != null)
                enemyBase.targetPos = enemyBase.targetTransform.position;

        // Clear target if aggroed by bullet
        if (enemyBase.IsShotAggroed)
        {
            enemyBase.IsShotAggroed = false;
            //enemyBase.targetTransform = null; **** Could be causing issue
        }

        stopTimer = 0;
        distanceCheck = enemyBase.StartCoroutine(DistanceToSprintCheck(
                        DistToStartSprint, SprintSpeedMultiplier, TimeToCheckDistance));
    }
    public virtual void DoExitLogic()
    {
        if (jumpCoroutine != null)
        {
            enemyBase.StopCoroutine(jumpCoroutine);
        }

        if (distanceCheck != null)
        {
            enemyBase.StopCoroutine(distanceCheck);
            distanceCheck = null;
        }

        //enemyBase.IsAggroed = false;
        //enemyBase.targetTransform = null;
        ResetJumpVariables();
    }
    public virtual void DoFrameUpdateLogic() { }
    public virtual void DoPhysicsUpdateLogic()
    {
        // If aggroed, reset stop chase timer.
        if (enemyBase.IsAggroed)
        {
            // Refresh enemyBase.targetPos
            stopTimer = 0;
            if (enemyBase.targetTransform != null)
            {
                enemyBase.targetPos = enemyBase.targetTransform.position;
                RandomChanceToStartShooting();
            }
        }
        else
        {
            stopTimer += Time.fixedDeltaTime;

            if (stopTimer >= timeToStopChase)
            {
                enemyBase.StateMachine.ChangeState(enemyBase.IdleState);
                stopTimer = 0;
                return;
            }
        }

        // Random chance to start shooting if in range
        
    }

    void RandomChanceToStartShooting()
    {
        float squaredDist = (enemyBase.targetPos - (Vector2)transform.position).sqrMagnitude;
        if (squaredDist < distanceToStartShooting * distanceToStartShooting && canExit)
        {
            if (Random.value < chanceToStartShootingPerFrame)
            {
                enemyBase.StateMachine.ChangeState(enemyBase.ShootState);
            }
        }
    }

    public virtual void DoAnimationTriggerEventLogic(EnemyBase.AnimationTriggerType triggerType) { }
    public virtual void ResetValues() { }

    #endregion

    #region Distance Check

    public IEnumerator DistanceToSprintCheck(float distToStartSprint,
                        float sprintSpeedMultiplier, float timeToCheckDistance)
    {
        while (true)
        {
            if (enemyBase.IsAggroed)
            {
                // if in sprint range. sqrMagnitude for efficiency
                if ((enemyBase.targetPos - (Vector2)transform.position).sqrMagnitude
                    < distToStartSprint * distToStartSprint)
                {
                    // Sprint
                    if (accelerate == null && _sprintFactor < sprintSpeedMultiplier - 0.01f)
                    {
                        isSprinting = true;
                        if (stopSprint != null)
                        {
                            enemyBase.StopCoroutine(stopSprint);
                            stopSprint = null;
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

    #endregion

    #region Sprint Control

    public void StartSprint()
    {
        isSprinting = true;
        if (accelerate != null)
        {
            enemyBase.StopCoroutine(accelerate);
            accelerate = null;
        }
        if (stopSprint != null)
        {
            enemyBase.StopCoroutine(stopSprint);
            stopSprint = null;
        }
        stopSprint = enemyBase.StartCoroutine(StopSprint());
        accelerate = enemyBase.StartCoroutine(
                                LerpSpeed(_sprintFactor, SprintSpeedMultiplier, TimeToFullSprint));
    }

    public IEnumerator StopSprint()
    {
        yield return new WaitForSeconds(TotalSprintTime);
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

            timeElapsed += Time.deltaTime * 1 / 0.1f;
        }

        accelerate = null;
    }

    #endregion

    #region Jump Control

    protected IEnumerator JumpTowardsTarget()
    {
        // Switch to sitting animation
        enemyBase.animControllerEnemy.SetSitAnimation(true);

        // Sample target position to predict movement
        Vector2 previousPos = enemyBase.targetPos;

        // Aim at predicted target position while sitting, before jump
        Vector2 predictedPos = enemyBase.targetPos;
        float count = 0f;
        while (count < jumpStartDelay)
        {
            previousPos = enemyBase.targetTransform != null ? (Vector2)enemyBase.targetTransform.position : enemyBase.targetPos;

            yield return new WaitForFixedUpdate();

            enemyBase.targetPos = enemyBase.targetTransform != null ? (Vector2)enemyBase.targetTransform.position : enemyBase.targetPos;

            float sampledTime = Time.fixedDeltaTime;
            predictedPos = GameTools.GetPredictedPosition(previousPos, sampledTime,enemyBase.targetPos, jumpDuration);
            enemyBase.ChangeFacingDirection((Vector3)predictedPos - transform.position, rotateSpeedMultiplier);

            count += sampledTime;
        }

        // Clamp the lead so we don’t overshoot wildly if the target teleports/dashes
        Vector2 nowToPred = predictedPos - (enemyBase.targetTransform != null ? (Vector2)enemyBase.targetTransform.position :enemyBase.targetPos);
        if (nowToPred.sqrMagnitude > maxLeadDistance * maxLeadDistance)
            predictedPos = (enemyBase.targetTransform != null ? (Vector2)enemyBase.targetTransform.position : enemyBase.targetPos) + nowToPred.normalized * maxLeadDistance;

        // If target is too close, exit jump
        Vector2 fromSelfToPredicted = predictedPos - (Vector2)transform.position;
        if (fromSelfToPredicted.sqrMagnitude < distToNotJump * distToNotJump)
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
        Vector3Int gridPos;
        int iterationCount = 0;
        while (true)
        {
            // Pick a random spot around the predicted landing center
            randomOffset = Random.insideUnitCircle * (jumpAccuracyRadius + iterationCount * 0.5f);
            endPos = predictedPos + (Vector2)randomOffset;

            // If endPos is too far, jump the max dist
            if ((endPos - startPos).sqrMagnitude > jumpMaxDistance * jumpMaxDistance)
            {
                Vector3 jumpVector = (endPos - startPos).normalized * jumpMaxDistance; // CHANGED: recompute from endPos
                endPos = startPos + jumpVector;
            }

            // Check for obstacles at endPos
            gridPos = TileManager.Instance.WorldToGrid(endPos);
            if (TileManager.Instance.ContainsTileKey(gridPos) && TileManager.Instance.GetStructureAt(endPos).structureType == StructureType.ThreeByThreePlatform)
            {
                break;
            }
            if (!TileManager.Instance.ContainsTileKey(gridPos))
                break;

            iterationCount++;
            yield return new WaitForFixedUpdate();
        }

        yield return new WaitForSeconds(freezeBeforeJump);  // A quick grace-freeze before jump

        // Reset airborne knockback state
        airVel = Vector2.zero;
        airOffset = Vector2.zero;

        // Perform the jump arc and scale effect
        canExit = false;
        isInAir = true;
        enemyBase.meleeController.isElevated = true;
        enemyBase.gameObject.layer = LayerMask.NameToLayer("FlyingEnemies");
        enemyBase.animControllerEnemy.SetSitAnimation(false);
        enemyBase.animControllerEnemy.SetJumpAnimation(true);
        localScaleStart = transform.localScale;

        float elapsed = 0;
        while (elapsed < jumpDuration)
        {
            // Use fixed dt because we’re in FixedUpdate cadence
            float dt = Time.fixedDeltaTime;
            elapsed += dt;
            float t = elapsed / jumpDuration;

            // Base parabola (unchanged)
            float x = Mathf.Lerp(startPos.x, endPos.x, t);
            float y = jumpCurveHeight * (t - t * t) + Mathf.Lerp(startPos.y, endPos.y, t);
            Vector3 basePos = new Vector3(x, y, transform.position.z);

            // Integrate airborne knockback offset
            // Linear drag towards zero velocity
            if (airVel.sqrMagnitude > 0f)
            {
                // move velocity toward zero by airDrag * dt (critically damp-ish)
                float vMag = airVel.magnitude;
                float decel = airDrag * dt;
                float newMag = Mathf.Max(0f, vMag - decel);
                airVel = (vMag > 0f) ? airVel * (newMag / vMag) : Vector2.zero;
            }

            // Integrate offset
            airOffset += airVel * dt;

            // Safety clamp to avoid insane offsets
            if (airOffset.sqrMagnitude > (jumpMaxDistance * jumpMaxDistance))
                airOffset = airOffset.normalized * jumpMaxDistance;

            // FINAL position = parabola + offset
            Vector3 finalPos = basePos + (Vector3)airOffset;
            transform.position = finalPos;

            // Scale: 1.0 → jumpHeightScale at mid → 1.0
            float jumpScale = 4f * t * (1f - t); // 0→1→0
            float scale = Mathf.Lerp(localScaleStart.x, jumpHeightScale * localScaleStart.x, jumpScale);
            transform.localScale = Vector3.one * scale;

            yield return new WaitForFixedUpdate();
        }

        ResetJumpVariables();

        // Set cooldown timer
        jumpCooldownTimer = jumpCooldown;
    }

    void ResetJumpVariables()
    {
        canExit = true;
        isInAir = false;
        isJumping = false;
        jumpCoroutineStarted = false;
        jumpCoroutine = null;
        enemyBase.meleeController.isElevated = false;
        enemyBase.gameObject.layer = LayerMask.NameToLayer("Enemies");
        enemyBase.animControllerEnemy.SetJumpAnimation(false);
        enemyBase.animControllerEnemy.SetSitAnimation(false);
    }

    public void ApplyAirImpulse(Vector2 impulse)
    {
        // impulse is (force * deltaTime) or “instant” velocity change you choose
        if (!isInAir) return; // let your ground knockback system handle ground

        // Δv = impulse / mass
        airVel += impulse / Mathf.Max(enemyBase.rb.mass * airMassMult, 0.0001f);
        // Optional: clamp instantaneous spike
        if (airVel.sqrMagnitude > airMaxSpeed * airMaxSpeed)
            airVel = airVel.normalized * airMaxSpeed;
    }
    
    #endregion

}
