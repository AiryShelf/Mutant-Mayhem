using System.Collections;
using System.Collections.Generic;
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

    public DroneHangar myHangar;
    bool jobDone = false;
    Coroutine actionCoroutine; // Used for states
    Coroutine hoverCoroutine;
    

    public Drone(DroneType type, DroneHangar hangar)
    {
        droneType = type;
        myHangar = hangar;
    }

    void Awake()
    {
        //jobDone = true;
        //currentJob = new DroneJob(DroneJobType.None, Vector3.zero);
    }

    void Start()
    {
        //SetJobDone();
    }

    public void Launch()
    {
        gameObject.SetActive(true);

    }
    
    public void SetJob(DroneJob job)
    {
        jobDone = false;
        currentJob = job;
        SetNewAction(MoveToJob);
        Debug.Log("Drone: New job set");
    }

    IEnumerator MoveToJob()
    {
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

    void DoJob()
    {
        if (currentJob.jobType == DroneJobType.Build)
            SetNewAction(Build);
        else if (currentJob.jobType == DroneJobType.Repair)
            SetNewAction(Repair);
        else
            SetJobDone();

        StartCoroutine(AlignToPos(currentJob.jobPosition));
    }

    IEnumerator Build()
    {
        DroneBuildJob buildJob;
        if (currentJob is DroneBuildJob)
            buildJob = (DroneBuildJob)currentJob;
        else
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
        DroneJob newJob = myHangar.GetDroneJob(droneType);
        if (newJob != null)
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
                if (newJob != null)
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

    public IEnumerator LandInHangar()
    {
        // Landing effect here **

        yield return new WaitForFixedUpdate();
        myHangar.LandDrone(this);
        StopAllCoroutines();
        gameObject.SetActive(false);
    }

    IEnumerator AlignToPos(Vector2 pos)
    {
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

                float scaleFactor = 1 + hoverScaleFactor * sineValue * (1 + hoverEffectVariationFactor);
                transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);

                // Wait until next frame
                yield return new WaitForFixedUpdate();
                timer += Time.fixedDeltaTime;
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

    public void SetNewAction(System.Func<IEnumerator> coroutineMethod)
    {
        StopAllCoroutines();
        
        hoverCoroutine = StartCoroutine(HoverEffect());
        actionCoroutine = StartCoroutine(coroutineMethod());
        StartCoroutine(CheckIfJobDone());
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

        while (jobDone == false)
        {
            if (currentJob == null)
            {
                Debug.Log("Drone: CurrentJob found null");
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

    public void Die()
    {
        // This should be on a Health script attached to the Drone
        StopAllCoroutines();
        myHangar.RemoveDrone(this);
        PoolManager.Instance.ReturnToPool(objectPoolName, gameObject);
    }
}

public enum DroneType
{
    Builder,
    Attacker
}