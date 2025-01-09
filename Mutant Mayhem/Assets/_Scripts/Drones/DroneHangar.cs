using System.Collections;
using System.Collections.Generic;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;

public class DroneHangar : MonoBehaviour
{
    public List<Drone> controlledDrones;
    public List<Drone> dockedDrones;
    public int maxDrones;
    public float detectionRange;

    void Start()
    {
        StartCoroutine(LookForJobs());
    }   

    IEnumerator LookForJobs()
    {
        while (true)
        {
            DroneJob job = null;
            Drone foundDrone = null;
            foreach (Drone drone in dockedDrones)
            {
                if (drone.currentJob.jobType != DroneJobType.None)
                    continue;

                job = GetJob(drone);

                if (job == null)
                        continue;

                foundDrone = drone;
                break;                
            }
            if (foundDrone != null)
            {
                LaunchDrone(foundDrone);
                AssignJob(foundDrone, job);
            }
            yield return new WaitForSeconds(1);
        }
    } 

    public bool LookForJobInArea(Drone drone, Vector2 pos)
    {
        DroneJob closestJob = null;

        if (drone.droneType == DroneType.Builder)
            closestJob = SearchForConstructionJobs(pos);

        if (closestJob != null)
        {
            AssignJob(drone, closestJob);
            return true;
        }

        return false;
    }

    DroneJob SearchForConstructionJobs(Vector2 pos)
    {
        DroneJob closestJob = null;
        float closestDistance = Mathf.Infinity;

        // Check build jobs first (priority)
        foreach (DroneBuildJob buildJob in ConstructionManager.Instance.buildJobs)
        {
            float distance = Vector2.Distance(pos, buildJob.jobPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestJob = buildJob;
            }
        }

        // If a build job is found nearby, skip checking repair jobs
        if (closestJob != null)
        {
            return closestJob;
        }

        // If no build job is nearby, check repair jobs
        foreach (DroneJob repairJob in ConstructionManager.Instance.repairJobs)
        {
            float distance = Vector2.Distance(pos, repairJob.jobPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestJob = repairJob;
            }
        }

        return closestJob;
    }

    public DroneJob GetDroneJob(DroneType droneType)
    {
        DroneJob job = null;

        if (droneType == DroneType.Builder)
        {
            job = ConstructionManager.Instance.GetBuildJob();
        }

        return job;
    }

    void LaunchDrone(Drone drone)
    {
        Debug.Log("Drone Launch attempted");
        if (dockedDrones.Contains(drone))
        {
            drone.Launch();
            dockedDrones.Remove(drone);
        }
        else
            Debug.LogError("DroneHangar: No drone found to launch!");
    }

    void AssignJob(Drone drone, DroneJob job)
    {
        if (job.jobType == DroneJobType.Build)
            ConstructionManager.Instance.RemoveBuildJobAt(job.jobPosition);
        else if (job.jobType == DroneJobType.Repair)
            ConstructionManager.Instance.RemoveRepairJobAt(job.jobPosition);

        drone.SetJob(job);
    }

    DroneJob GetJob(Drone drone)
    {
        DroneJob job = null;
        DroneBuildJob buildJob = ConstructionManager.Instance.GetBuildJob();
        DroneJob repairJob = ConstructionManager.Instance.GetRepairJob();

        if (drone.droneType == DroneType.Builder)
        {
            Debug.Log("Builder drone found");
            // Assign build job over repair
            job = buildJob;
            if (buildJob == null)
                job = repairJob;
        }

        return job;
    }
}

[System.Serializable]
public class DroneJob
{
    public DroneJobType jobType;
    public Vector2 jobPosition;

    public DroneJob(DroneJobType jobType, Vector3 position)
    {
        this.jobType = jobType;
        this.jobPosition = position;
    }
}

[System.Serializable]
public class DroneBuildJob : DroneJob
{
    public int rotation;

    public DroneBuildJob(DroneJobType jobType, Vector3 position, int rotation) : base(jobType, position)
    {
        this.rotation = rotation;
    }
}

public enum DroneJobType
{
    None,
    Recharge,
    SelfRepair,
    Build,
    Repair,
    Attack,
}
