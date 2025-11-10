using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionDrone : Drone
{
    internal IEnumerator Build()
    {
        //Debug.LogError("BUILD STARTED");
        yield return null;
        alignCoroutine = StartCoroutine(AlignToPos(currentJob.jobPosition));
        if (jobHeightCoroutine != null)
            StopCoroutine(jobHeightCoroutine);
        jobHeightCoroutine = StartCoroutine(LowerToJob());
        jobCheckCoroutine = StartCoroutine(CheckIfJobDone());
        isFlying = false;
        if (currentJob is not DroneBuildJob)
        {
            Debug.LogError("Construction Drone: Tried to build when current job is not a DroneBuildJob");
            yield break;
        }

        Vector2 jobPos = currentJob.jobPosition;
        while (true)
        {
            Vector2 hitDir = jobPos - (Vector2)transform.position;
            switch (ConstructionManager.Instance.BuildBlueprint(jobPos, -shooter.currentGunSO.damage, hitDir))
            {
                case ConstructionManager.BuildResult.Complete:
                    SetJobDone();
                    yield break;
                case ConstructionManager.BuildResult.NoSupplies:
                    //Debug.Log("Construction Drone: Stopping build, no supplies");
                    myHangar.RemoveDroneFromJob(this);
                    DroneBuildJob nextJob = myHangar.GetNextBuildJob(currentJob as DroneBuildJob);
                    if (nextJob != null && nextJob != currentJob)
                    {
                        SetJob(nextJob);
                        yield break;
                    }
                    break;
                default:
                    break;
            }

            yield return new WaitForSeconds(shooter.currentGunSO.shootSpeed);
        }
    }

    internal IEnumerator Repair()
    {
        yield return null;
        alignCoroutine = StartCoroutine(AlignToPos(currentJob.jobPosition));
        if (jobHeightCoroutine != null)
            StopCoroutine(jobHeightCoroutine);
        jobHeightCoroutine = StartCoroutine(LowerToJob());
        jobCheckCoroutine = StartCoroutine(CheckIfJobDone());
        isFlying = false;
        Vector2 jobPos = currentJob.jobPosition;
        
        while (true)
        {
            Vector2 hitDir = jobPos - (Vector2)transform.position;
            if (ConstructionManager.Instance.RepairUntilComplete(jobPos, -shooter.currentGunSO.damage, hitDir))
            {
                break;
            }

            yield return new WaitForSeconds(shooter.currentGunSO.shootSpeed);
        }

        SetJobDone();
    }

    protected override IEnumerator CheckIfJobDone()
    {
        yield return null;

        while (!jobDone)
        {
            if (currentJob == null)
            {
                Debug.Log("Construction Drone: CurrentJob found null");
                SetJobDone();
                yield break;
            }
            if (currentJob.jobType == DroneJobType.None)
            {
                Debug.Log("Construction Drone: CurrentJob type is None");
                SetJobDone();
                yield break;
            }

            if (currentJob is DroneBuildJob buildJob)
            {
                if (!ConstructionManager.Instance.CheckIfBuildJobExists(buildJob))
                {
                    Debug.Log("Construction Drone: Build job no longer exists");
                    SetJobDone();
                    yield break;
                }
            }
            else if (!ConstructionManager.Instance.CheckIfRepairJobExists(currentJob))
            {
                Debug.Log("Construction Drone: Repair job no longer exists");
                SetJobDone();
                yield break;
            }

            yield return new WaitForSeconds(1);
        }
    }
}
