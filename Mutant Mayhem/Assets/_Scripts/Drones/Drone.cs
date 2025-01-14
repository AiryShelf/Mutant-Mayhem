using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Drone : MonoBehaviour
{
    public string objectPoolName = "";
    public DroneType droneType = DroneType.Builder;
    public float moveSpeed = 3;
    public float buildSpeed = 0.1f;  // For upgrades, most of stats should be moved to PlayerStats
    public float actionDelay = 1;
    public float repairSpeed = 1;
    public DroneJob currentJob;
    [SerializeField] Rigidbody2D myRB;
    [SerializeField] float minJobDist = 1f;
    [SerializeField] float hoverEffectTime = 1;
    [SerializeField] float hoverEffectVariationFactor = 0.2f;
    [SerializeField] float hoverEffectForceFactor = 0.2f;
    [SerializeField] float hoverScaleFactor = 0.05f;
    [SerializeField] float rotationSpeed = 0.0025f;
    [SerializeField] float launchOrLandMinScale = 0.3f;
    [SerializeField] float launchOrLandScaleSpeed = 0.05f;
    [SerializeField] float jobHeightMinScale = 0.6f;

    public DroneHangar myHangar;
    public bool isFlying = false;
    public float heightScaleStart = 1;
    float heightScale = 1;
    bool jobDone = false;
    Coroutine actionCoroutine; // Used for states
    Coroutine hoverCoroutine;
    Coroutine alignCoroutine;
    Coroutine jobHeightCoroutine;
    Coroutine jobCheckCoroutine;
    DroneHealth droneHealth; 

    public void Initialize()
    {
        droneHealth = GetComponent<DroneHealth>();
        if (droneHealth == null)
        {
            Debug.LogError("Drone: Could not find DroneHealth on self!");
            return;
        }
        droneHealth.SetHealth(droneHealth.GetMaxHealth());
    }

    #region Launch / Land

    public void Launch()
    {
        gameObject.SetActive(true);

        heightScale = heightScaleStart * launchOrLandMinScale;
        transform.localScale = new Vector3(heightScale, heightScale, 1);
        hoverCoroutine = StartCoroutine(HoverEffect());
    }

    IEnumerator LowerToJob()
    {
        yield return null;

        isFlying = false;

        while (heightScale > jobHeightMinScale)
        {
            yield return new WaitForSeconds(0.05f);
            heightScale -= launchOrLandScaleSpeed;
        }
    }

    IEnumerator RaiseFromJob()
    {
        yield return null;

        while (heightScale < heightScaleStart)
        {
            yield return new WaitForSeconds(0.05f);
            heightScale += launchOrLandScaleSpeed;
        }

        isFlying = true;
    }

    IEnumerator LandInHangar()
    {
        yield return null;
        isFlying = false;

        while (heightScale > launchOrLandMinScale)
        {
            yield return new WaitForSeconds(0.05f);
            heightScale -= launchOrLandScaleSpeed;
        }

        yield return new WaitForFixedUpdate();
        
        myHangar.LandDrone(this);
        StopAllCoroutines();
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

    void SetNewAction(System.Func<IEnumerator> coroutineMethod)
    {
        //StopAllCoroutines();
        if (alignCoroutine != null)
            StopCoroutine(alignCoroutine);
        if (actionCoroutine != null)
            StopCoroutine(actionCoroutine);
        if (jobCheckCoroutine != null)
            StopCoroutine(jobCheckCoroutine);
        if (jobHeightCoroutine != null)
            StopCoroutine(jobHeightCoroutine);

        actionCoroutine = StartCoroutine(coroutineMethod());
    }

    IEnumerator MoveToJob()
    {
        yield return null;
        jobCheckCoroutine = StartCoroutine(CheckIfJobDone());
        jobHeightCoroutine = StartCoroutine(RaiseFromJob());
        isFlying = true;
        Vector2 target = Vector2.zero;
        if (currentJob != null)
            target = currentJob.jobPosition;

        bool arrived = false;
        while (!arrived)
        {
            MoveTowards(target, 1);
            yield return new WaitForFixedUpdate();
            if (Vector2.Distance(transform.position, target) < minJobDist)
            {
                arrived = true;
                DoJob();
            }
        }
    }

    void MoveTowards(Vector2 target, float forceFactor)
    {
        Vector2 dir = target - (Vector2)myRB.transform.position;
        myRB.AddForce(dir.normalized * moveSpeed);

        float desiredAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        float currentAngle = myRB.rotation;

        float angleDiff = Mathf.DeltaAngle(currentAngle, desiredAngle);
        float torque = angleDiff * rotationSpeed;

        myRB.AddTorque(torque, ForceMode2D.Force);
    }

    void DoJob()
    {
        
        if (currentJob.jobType == DroneJobType.Build)
            SetNewAction(Build);
        else if (currentJob.jobType == DroneJobType.Repair)
            SetNewAction(Repair);
        else
            SetJobDone();

        if (currentJob == null)
            return;

        alignCoroutine = StartCoroutine(AlignToPos(currentJob.jobPosition));
        jobHeightCoroutine = StartCoroutine(LowerToJob());
    }

    IEnumerator Build()
    {
        yield return null;
        jobCheckCoroutine = StartCoroutine(CheckIfJobDone());
        isFlying = false;
        if (currentJob is not DroneBuildJob)
        {
            Debug.LogError("Drone: Tried to build when current job is not a DroneBuildJob");
            yield break;
        }

        Vector2 jobPos = currentJob.jobPosition;
        while (true)
        {
            Vector2 hitDir = jobPos - (Vector2)transform.position;
            if (ConstructionManager.Instance.BuildBlueprint(jobPos, buildSpeed, hitDir))
            {
                SetJob(ConstructionManager.Instance.GetRepairJob());
                yield break;
            }

            yield return new WaitForSeconds(actionDelay);
        }
    }

    IEnumerator Repair()
    {
        yield return null;
        jobCheckCoroutine = StartCoroutine(CheckIfJobDone());
        isFlying = false;
        Vector2 jobPos = currentJob.jobPosition;

        while (true)
        {
            Vector2 hitDir = jobPos - (Vector2)transform.position;
            if (ConstructionManager.Instance.RepairTile(jobPos, repairSpeed, hitDir))
            {
                break;
            }

            yield return new WaitForSeconds(actionDelay);
        }

        SetJobDone();
    }

    IEnumerator FlyToHangar()
    {
        yield return null;

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
        if (Vector2.Distance(transform.position, target) < minJobDist)
                arrived = true;

        while (!arrived)
        {
            MoveTowards(target, 1);
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
            if (Vector2.Distance(transform.position, target) < minJobDist)
                arrived = true;
        }

        SetNewAction(LandInHangar);
    }

    #endregion

    #region Secondary Action

    IEnumerator AlignToPos(Vector2 pos)
    {
        yield return null;
        while (true)
        {
            yield return new WaitForFixedUpdate();

            bool aligned = Vector2.Distance(transform.position, pos) < minJobDist;
            if (!aligned)
            {
                MoveTowards(pos, 1f);
                if (aligned)
                    Debug.Log("Drone aligned");
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
                myRB.AddForce(randomDir * currentForce, ForceMode2D.Force);

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

    void SetJobDone()
    {
        jobDone = true;
        currentJob = new DroneJob(DroneJobType.None, Vector3.zero);
        SetNewAction(FlyToHangar);
    }

    IEnumerator CheckIfJobDone()
    {
        yield return null;

        while (!jobDone)
        {
            if (currentJob == null)
            {
                Debug.Log("Drone: CurrentJob found null");
                SetJobDone();
                yield break;
            }
            if (currentJob.jobType == DroneJobType.None)
            {
                SetJobDone();
                yield break;
            }

            if (currentJob is DroneBuildJob buildJob)
            {
                if (!ConstructionManager.Instance.CheckIfBuildJobExists(buildJob))
                {
                    Debug.Log("Drone: Build job no longer exists");
                    SetJobDone();
                    yield break;
                }
            }
            else if (!ConstructionManager.Instance.CheckIfRepairJobExists(currentJob))
            {
                Debug.Log("Drone: Repair job no longer exists");
                SetJobDone();
                yield break;
            }

            yield return new WaitForSeconds(1);
        }
    }

    #endregion
}

public enum DroneType
{
    Builder,
    Attacker
}