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
            DroneJob buildJob = ConstructionManager.Instance.GetBuildJob();
            DroneJob repairJob = ConstructionManager.Instance.GetRepairJob();
            // Can add here for attack or other jobs

            foreach (Drone drone in dockedDrones)
            {
                if (drone.currentJob != null)
                    continue;

                if (drone.droneType == DroneType.Builder)
                {
                    // Assign build job over repair
                    var job = repairJob;
                    if (buildJob != null)
                        job = buildJob;
                    
                    if (job == null)
                        continue;

                    LaunchDrone(drone);
                    AssignJob(drone, job);
                }
            }
            yield return new WaitForSeconds(1);
        }
    } 

    void LaunchDrone(Drone drone)
    {
        if (dockedDrones.Contains(drone))
        {
            drone.Launch();
            dockedDrones.Remove(drone);
        }
        else
            Debug.LogError("DroneHangar: No drone found to launch!");
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

    void AssignJob(Drone drone, DroneJob job)
    {
        if (job.jobType == DroneJobType.Build)
            ConstructionManager.Instance.RemoveBuildJobAt(job.jobPosition);
        else if (job.jobType == DroneJobType.Repair)
            ConstructionManager.Instance.RemoveRepairJobAt(job.jobPosition);

        drone.SetJob(job);
    }
}

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
