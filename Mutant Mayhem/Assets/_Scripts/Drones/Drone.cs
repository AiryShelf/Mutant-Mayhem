using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour
{
    public Shooter shooter;
    public string objectPoolName = "";
    public DroneType droneType = DroneType.Builder;
    public float moveSpeed = 3;
    public float buildSpeed = 0.1f;  // For upgrades, most of stats should be moved to PlayerStats
    public float actionDelay = 1;
    public float repairSpeed = 1;
    public DroneJob currentJob;
    public Rigidbody2D rb;
    public SpriteRenderer sr;
    public GameObject lights;
    [SerializeField] protected float minJobDist = 1f;
    [SerializeField] float hoverEffectTime = 1;
    [SerializeField] float hoverEffectVariationFactor = 0.2f;
    [SerializeField] float hoverEffectForceFactor = 0.2f;
    [SerializeField] float hoverScaleFactor = 0.05f;
    [SerializeField] float rotationSpeed = 0.0025f;
    [SerializeField] float launchOrLandMinScale = 0.3f;
    [SerializeField] float launchOrLandScaleSpeed = 0.05f;
    [SerializeField] float flyingAlpha = 0.5f;
    [SerializeField] float jobHeightMinScale = 0.6f;

    public DroneHangar myHangar;
    public bool isFlying = false;
    public float heightScaleStart = 1;
    float heightScale = 1;
    protected bool jobDone = false;
    Coroutine actionCoroutine; // Used for states
    Coroutine hoverCoroutine;
    protected Coroutine alignCoroutine;
    protected Coroutine jobHeightCoroutine;
    protected Coroutine jobCheckCoroutine;
    DroneHealth droneHealth; 

    public virtual void Initialize(TurretGunSO droneGun)
    {
        droneHealth = GetComponent<DroneHealth>();
        if (droneHealth == null)
        {
            Debug.LogError("Drone: Could not find DroneHealth on self!");
            return;
        }
        droneHealth.SetHealth(droneHealth.GetMaxHealth());

        shooter.gunList[0] = droneGun;
        shooter.SwitchGuns(0);

        if (this is AttackDrone attackDrone)
            attackDrone.shooter.StartChargingGuns();
    }

    public virtual void RefreshStats() { }

    #region Launch / Land

    public void Launch()
    {
        rb.simulated = true;
        sr.enabled = true;
        lights.SetActive(true);

        heightScale = heightScaleStart * launchOrLandMinScale;
        transform.localScale = new Vector3(heightScale, heightScale, 1);
        jobHeightCoroutine = StartCoroutine(RaiseFromJob());
        hoverCoroutine = StartCoroutine(HoverEffect());
    }

    protected IEnumerator LowerToJob()
    {
        //Debug.Log("LOWER FROM JOB STARTED");
        yield return null;

        isFlying = false;

        while (heightScale > jobHeightMinScale)
        {
            yield return new WaitForSeconds(0.05f);
            heightScale -= launchOrLandScaleSpeed;
            UpdateAlphaBasedOnHeight();
        }
    }

    protected IEnumerator RaiseFromJob()
    {
        //Debug.Log("RAISE FROM JOB STARTED");
        yield return null;

        while (heightScale < heightScaleStart)
        {
            yield return new WaitForSeconds(0.05f);
            heightScale += launchOrLandScaleSpeed;
            UpdateAlphaBasedOnHeight();
        }

        isFlying = true;
    }

    IEnumerator LandInHangar()
    {
        //Debug.Log("LAND IN HANGAR STARTED");
        yield return null;
        if (jobHeightCoroutine != null)
            StopCoroutine(jobHeightCoroutine);
        isFlying = false;

        while (heightScale > launchOrLandMinScale)
        {
            yield return new WaitForSeconds(0.05f);
            heightScale -= launchOrLandScaleSpeed;
            UpdateAlphaBasedOnHeight();
        }

        yield return new WaitForFixedUpdate();
        
        myHangar.LandDrone(this);
        StopAllCoroutines();
    }

    void UpdateAlphaBasedOnHeight()
    {
        // Ensure we don't divide by zero
        float heightRange = heightScaleStart - jobHeightMinScale;
        if (Mathf.Approximately(heightRange, 0)) return;

        // Normalize the height scale
        float t = Mathf.Clamp01((heightScale - jobHeightMinScale) / heightRange);

        // Interpolate alpha
        float alpha = Mathf.Lerp(1, flyingAlpha, t);

        // Update the SpriteRenderer's alpha
        Color color = sr.color;
        color.a = alpha;
        sr.color = color;
    }

    public void Die()
    {
        StopAllCoroutines();
        if (myHangar != null)
            myHangar.RemoveDrone(this);

        DroneManager.Instance.RemoveDrone(this);
    }

    #endregion

    #region Main Actions

    protected virtual void SetNewAction(System.Func<IEnumerator> coroutineMethod)
    {
        //StopAllCoroutines();
        if (alignCoroutine != null)
            StopCoroutine(alignCoroutine);
        if (actionCoroutine != null)
            StopCoroutine(actionCoroutine);
        if (jobCheckCoroutine != null)
            StopCoroutine(jobCheckCoroutine);
        //if (jobHeightCoroutine != null)
            //StopCoroutine(jobHeightCoroutine);

        actionCoroutine = StartCoroutine(coroutineMethod());
    }

    IEnumerator MoveToJob()
    {
        yield return null;
        jobCheckCoroutine = StartCoroutine(CheckIfJobDone());
        if (jobHeightCoroutine != null)
            StopCoroutine(jobHeightCoroutine);
        jobHeightCoroutine = StartCoroutine(RaiseFromJob());
        isFlying = true;
        Vector2 target = Vector2.zero;
        if (currentJob != null)
        {
            if (currentJob.jobType == DroneJobType.Build ||
                currentJob.jobType == DroneJobType.Repair)
            {
                target = currentJob.jobPosition;
            }   
        }

        bool arrived = false;
        while (!arrived)
        {
            if (currentJob is DroneAttackJob attackJob)
                target = attackJob.targetTrans.position;
            MoveTowards(target, 1);
            RotateTowards(target);
            yield return new WaitForFixedUpdate();
            if (Vector2.Distance(transform.position, target) < minJobDist)
            {
                arrived = true;
                DoJob();
            }
        }
    }

    protected void MoveTowards(Vector2 target, float forceFactor)
    {
        Vector2 dir = target - (Vector2)rb.transform.position;
        rb.AddForce(dir.normalized * moveSpeed * forceFactor);
    }

    protected void RotateTowards(Vector2 target)
    {
        Vector2 dir = target - (Vector2)transform.position;
        float desiredAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
        float currentAngle = transform.eulerAngles.z + 90;

        float angleDiff = Mathf.DeltaAngle(currentAngle, desiredAngle);
        angleDiff = Mathf.Clamp(angleDiff, -15, 15);
        float torque = angleDiff * rotationSpeed;

        rb.AddTorque(torque, ForceMode2D.Force);
    }

    IEnumerator FlyToHangar()
    {
        yield return null;

        if (jobHeightCoroutine != null)
            StopCoroutine(jobHeightCoroutine);
        jobHeightCoroutine = StartCoroutine(RaiseFromJob());
        DroneJob newJob = myHangar.GetDroneJob(droneType);
        if (newJob.jobType != DroneJobType.None)
        {
            SetJob(newJob);
            yield break;
        }

        Vector2 target = myHangar.transform.position;

        float checkTimer = 0;
        bool arrived = false;
        if (Vector2.Distance(transform.position, target) < 1)
                arrived = true;

        while (!arrived)
        {
            MoveTowards(target, 1);
            RotateTowards(target);
            yield return new WaitForFixedUpdate();

            checkTimer += Time.fixedDeltaTime;
            if (checkTimer >= 1)
            {
                checkTimer = 0;
                newJob = myHangar.GetDroneJob(droneType);
                if (newJob.jobType != DroneJobType.None)
                {
                    SetJob(newJob);
                    yield break;
                }
            }
            if (Vector2.Distance(transform.position, target) < 1)
                arrived = true;
        }

        SetNewAction(LandInHangar);
    }

    #endregion

    #region 2nd Actions

    protected IEnumerator AlignToPos(Vector2 pos)
    {
        yield return null;
        while (true)
        {
            yield return new WaitForFixedUpdate();

            bool aligned = Vector2.Distance(transform.position, pos) < minJobDist;
            if (!aligned)
            {
                MoveTowards(pos, 1f);
                RotateTowards(pos);
                //if (aligned)
                    //Debug.Log("Drone aligned");
            }
        }
    }

    IEnumerator HoverEffect()
    {
        yield return null;
        while (true)
        {
            // pick how long we apply force in cycle
            float directionDuration = Random.Range(
                hoverEffectTime * (1 - hoverEffectVariationFactor),
                hoverEffectTime * (1 + hoverEffectVariationFactor)
            );

            // pick a random direction (unit circle normalized)
            Vector2 randomDir = Random.insideUnitCircle.normalized;

            float timer = 0f;
            //float forceFactor = Random.Range(hoverEffectForceFactor * (1 - hoverEffectVariationFactor),
            //                                 hoverEffectForceFactor * (1 + hoverEffectVariationFactor));
            while (timer < directionDuration)
            {
                // Normalized progress from 0..1
                float t = timer / directionDuration;

                // Force ramps up from 0 to hoverEffectForceFactor
                float sineValue = Mathf.Sin(Mathf.PI * t);
                float currentForce = hoverEffectForceFactor * sineValue;

                // Apply force each frame
                rb.AddForce(randomDir * currentForce, ForceMode2D.Force);

                float scaleFactor = heightScale + hoverScaleFactor * sineValue * (1 + hoverEffectVariationFactor);
                transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);

                // Wait until next frame
                yield return new WaitForFixedUpdate();
                timer += Time.fixedDeltaTime;
            }
        }
    }

    #endregion

    #region Jobs

    public void SetJob(DroneJob job)
    {
        if (job.jobType == DroneJobType.None)
        {
            jobDone = true;
            currentJob = job;
            SetNewAction(FlyToHangar);
        }
        else
        {
            jobDone = false;
            currentJob = job;
            SetNewAction(MoveToJob);
        }
        
        Debug.Log($"Drone: Job type set to: {job.jobType}");
    }

    void DoJob()
    {
        if (this is ConstructionDrone constructionDrone)
        {
            if (currentJob.jobType == DroneJobType.Build)
                SetNewAction(constructionDrone.Build);
            else if (currentJob.jobType == DroneJobType.Repair)
                SetNewAction(constructionDrone.Repair);
            else
                SetJobDone();
        }
        else if (this is AttackDrone attackDrone && currentJob is DroneAttackJob attackJob)
        {
            if (attackJob.targetTrans != null)
            {
                attackDrone.targetTrans = attackJob.targetTrans;
                SetNewAction(attackDrone.Attack);
            }
        }
        else
            SetJobDone();

        if (currentJob == null)
            return;
    }

    public void SetJobDone()
    {
        jobDone = true;
        currentJob = new DroneJob(DroneJobType.None, Vector3.zero);
        SetNewAction(FlyToHangar);
    }

    protected virtual IEnumerator CheckIfJobDone() { yield return null; }

    #endregion
}

public enum DroneType
{
    Builder,
    Attacker
}