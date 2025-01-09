using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Drone : MonoBehaviour
{
    public DroneType droneType = DroneType.Builder;
    public float moveSpeed = 3;
    public float buildSpeed = 0.5f;  // For upgrades, most of stats should be moved to PlayerStats
    public Vector2 jobPosition;
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

    void Awake()
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
    }

    IEnumerator MoveToJob()
    {
        Vector2 target = currentJob.jobPosition;
        bool arrived = false;
        while (!arrived)
        {
            MoveTowards(target, 1);
            yield return new WaitForFixedUpdate();
            if (Vector2.Distance(transform.position, target) < minJobDist)
            {
                arrived = true;
                SetNewAction(DoJob);
            }
        }
    }

    IEnumerator DoJob()
    {
        Vector2 jobPos = currentJob.jobPosition;
        bool aligned = false;
        while (!jobDone)
        {
            aligned = Vector2.Distance(transform.position, jobPos) < 0.2f;
            if (!aligned)
            {
                MoveTowards(currentJob.jobPosition, 0.3f);
                if (aligned)
                    Debug.Log("Drone aligned");
            }

            if (currentJob is DroneBuildJob buildJob)
                if (ConstructionManager.Instance.BuildBlueprint(buildJob, buildSpeed))
                    jobDone = true;

            yield return new WaitForFixedUpdate();
        }

        SetJobDone();
    }

    IEnumerator FlyToHangar()
    {
        if (myHangar.LookForJobInArea(this, transform.position))
            yield break;

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
                if (myHangar.LookForJobInArea(this, transform.position))
                    yield break;
            }
            if (Vector2.Distance(transform.position, target) < minJobDist)
                arrived = true;
        }

        SetNewAction(LandInHangar);
    }

    IEnumerator LandInHangar()
    {
        yield return new WaitForFixedUpdate();
    }

    void MoveTowards(Vector2 target, float forceFactor)
    {
        Vector2 dir = target - (Vector2)myRB.transform.position;
        myRB.AddForce(dir.normalized * moveSpeed);
    }

    void SetNewAction(System.Func<IEnumerator> coroutineMethod)
    {
        if (actionCoroutine != null)
            StopCoroutine(actionCoroutine);
        
        actionCoroutine = StartCoroutine(coroutineMethod());
    }

    void SetJobDone()
    {
        currentJob = new DroneJob(DroneJobType.None, Vector3.zero);
        SetNewAction(FlyToHangar);
    }

    

    void Die()
    {
        // This should be on a Health script attached to the Drone
        CancelJob();
    }

    void CancelJob()
    {
        if (currentJob is DroneBuildJob buildJob)
            ConstructionManager.Instance.buildJobs.Insert(0, buildJob);
        else if (currentJob.jobType == DroneJobType.Repair)
            ConstructionManager.Instance.repairJobs.Add(currentJob);

        SetJobDone();
    }
}

public enum DroneType
{
    Builder,
    Attacker
}