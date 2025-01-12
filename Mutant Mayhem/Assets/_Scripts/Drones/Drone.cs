using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour
{
    public DroneType droneType = DroneType.Builder;
    public float moveSpeed = 3;
    public float buildSpeed = 0.1f;  // For upgrades, most of stats should be moved to PlayerStats
    public float actionDelay = 1;
    public float repairSpeed = 1;
    public DroneJob currentJob;
    [SerializeField] Rigidbody2D myRB;
    [SerializeField] float minJobDist = 1f;

    public DroneHangar myHangar;
    bool jobDone = false;
    Coroutine actionCoroutine; // Used for states

    public Drone(DroneType type, DroneHangar hangar)
    {
        droneType = type;
        myHangar = hangar;
    }

    void Start()
    {
        SetJobDone();
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
        StartCoroutine(AlignToPos(currentJob.jobPosition));

        if (currentJob.jobType == DroneJobType.Build)
            SetNewAction(Build);
        else if (currentJob.jobType == DroneJobType.Repair)
            SetNewAction(Repair);
        else
            SetJobDone();
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

            if (ConstructionManager.Instance.BuildBlueprint(jobPos, buildSpeed, GameTools.GetRandomDirection()))
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

        // Repair

        while (true)
        {
            if (ConstructionManager.Instance.RepairTile(jobPos, repairSpeed))
            {
                break;
            }

            yield return new WaitForSeconds(actionDelay);
        }

        SetJobDone();
    }

    IEnumerator AlignToPos(Vector2 pos)
    {
        bool aligned;
        aligned = Vector2.Distance(transform.position, pos) < 0.3f;
        if (!aligned)
        {
            MoveTowards(pos, 1f);
            if (aligned)
                Debug.Log("Drone aligned");
        }
        yield return new WaitForSeconds(0.05f);
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

    IEnumerator LandInHangar()
    {
        yield return new WaitForFixedUpdate();
        myHangar.LandDrone(this);
        gameObject.SetActive(false);
    }

    void MoveTowards(Vector2 target, float forceFactor)
    {
        Vector2 dir = target - (Vector2)myRB.transform.position;
        myRB.AddForce(dir.normalized * moveSpeed);
    }

    void SetNewAction(System.Func<IEnumerator> coroutineMethod)
    {
        StopAllCoroutines();
        
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
        Destroy(gameObject);
    }
}

public enum DroneType
{
    Builder,
    Attacker
}