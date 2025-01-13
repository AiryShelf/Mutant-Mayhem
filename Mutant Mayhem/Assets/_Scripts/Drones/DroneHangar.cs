using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneHangar : MonoBehaviour
{
    public List<Drone> controlledDrones;
    public List<Drone> dockedDrones;
    public List<Drone> dronesToSpawnAtStart;
    public int maxDrones;
    public float detectionRange;

    void Start()
    {
        SpawnStartDrones();
        StartCoroutine(LookForJobs());
    }

    void SpawnStartDrones()
    {
        foreach (Drone startDrone in dronesToSpawnAtStart)
        {
            DroneManager.Instance.SpawnDroneInHangar(startDrone.droneType, this);
        }
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

                if (job.jobType == DroneJobType.None)
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
        DroneJob newJob = new DroneJob(DroneJobType.None, Vector2.zero);

        if (droneType == DroneType.Builder)
        {
            DroneJob job = ConstructionManager.Instance.GetBuildJob();
            if (job == null)
                job = ConstructionManager.Instance.GetRepairJob();
            
            if (job != null)
                newJob = job;
        }

        return newJob;
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

    public bool AddDrone(Drone drone)
    {
        if (drone == null)
        {
            Debug.LogError("DroneHangar: Tried to Add a null Drone to the hangar");
            return false;
        }

        if (controlledDrones.Count >= maxDrones)
        {
            Debug.Log("DrongHanger: Already full when adding drone");
            return false;
        }
        
        drone.transform.position = transform.position;
        controlledDrones.Add(drone);
        LandDrone(drone);
        return true;
    }

    public void LandDrone(Drone drone)
    {
        drone.myHangar = this;
        drone.currentJob = new DroneJob(DroneJobType.None, Vector3.zero);
        dockedDrones.Add(drone);
        drone.gameObject.SetActive(false);
        //drone.SetNewAction(drone.LandInHangar);
    }

    public void RemoveDrone(Drone drone)
    {
        if (controlledDrones.Contains(drone))
            controlledDrones.Remove(drone);
        if (dockedDrones.Contains(drone))
            dockedDrones.Remove(drone);
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

        if (drone.droneType == DroneType.Attacker)
        {
            
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
