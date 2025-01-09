using System.Collections;
using System.Collections.Generic;
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

    bool jobDone = false;
    Coroutine actionCoroutine; // Used for states

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
            Move(target);
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
        while (!jobDone)
        {
            if (currentJob.jobType == DroneJobType.Build)
                if (ConstructionManager.Instance.BuildBlueprint(currentJob, buildSpeed))
                    jobDone = true;

            yield return new WaitForFixedUpdate();
        }
    }

    void Move(Vector2 target)
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
        currentJob = null;
        
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
    }
}

public enum DroneType
{
    Builder,
    Attacker
}