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
            Drone freeDrone = null;
            foreach (Drone dockedDrone in dockedDrones)
            {
                if (dockedDrone.currentJob.jobType != DroneJobType.None)
                    continue;

                job = GetJob(dockedDrone);

                if (job == null)
                        continue;

                freeDrone = dockedDrone;
                break;                
            }
            if (freeDrone != null)
            {
                LaunchDrone(freeDrone);
                freeDrone.SetJob(job);
            }
            yield return new WaitForSeconds(1);
        }
    } 

    public bool LookForJobInArea(Drone drone, Vector2 pos)
    {
        DroneJob closestJob = null;

        if (drone.droneType == DroneType.Builder)
            closestJob = ConstructionManager.Instance.GetNearestJob(pos);

        if (closestJob != null)
        {
            drone.SetJob(closestJob);
            return true;
        }

        return false;
    }

    public DroneJob GetDroneJob(DroneType droneType)
    {
        DroneJob job = null;

        if (droneType == DroneType.Builder)
        {
            job = ConstructionManager.Instance.GetBuildJob();
            if (job == null)
                job = ConstructionManager.Instance.GetRepairJob();
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

    public void LandDrone(Drone drone)
    {
        dockedDrones.Add(drone);
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
    public StructureSO structure;

    public DroneBuildJob(DroneJobType jobType, Vector3 position, int rotation, StructureSO structure) : base(jobType, position)
    {
        this.rotation = rotation;
        this.structure = structure;
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
