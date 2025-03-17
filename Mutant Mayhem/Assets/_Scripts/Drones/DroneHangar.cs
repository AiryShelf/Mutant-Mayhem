using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DroneHangar : MonoBehaviour
{
    public LineRendererCircle droneRangeCircle;
    public List<Drone> controlledDrones;
    public List<Drone> dockedDrones;
    public int maxConstructionDrones;
    public int maxAttackDrones;
    [SerializeField] float launchDelay = 0.5f;
    public float detectionRadius = 6f;
    [SerializeField] int repairAmountPerSec = 10;

    [SerializeField] Collider2D detectionCollider;

    // Tracks num assigned drones
    [SerializeField] List<KeyValuePair<DroneAttackJob, int>> attackJobs = new List<KeyValuePair<DroneAttackJob, int>>(); 

    void Start()
    {
        SpawnStartDrones();
        StartCoroutine(LookForJobs());
        StartCoroutine(RepairDrones());
    }

    public void ShowRangeCircle(bool show)
    {
        droneRangeCircle.EnableCircle(show);
    }

    #region Drones

    void SpawnStartDrones()
    {
        Player player = FindObjectOfType<Player>();
        for (int i = 0; i < player.stats.numStartBuilderDrones; i++)
        {
            DroneManager.Instance.SpawnDroneInHangar(DroneType.Builder, this);
        }
        for (int i = 0; i < player.stats.numStartAttackDrones; i++)
        {
            DroneManager.Instance.SpawnDroneInHangar(DroneType.Attacker, this);
        }
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

    public void AddDrone(Drone drone)
    {
        if (drone == null)
        {
            Debug.LogError("DroneHangar: Tried to Add a null Drone to the hangar");
            return;
        }
        
        drone.transform.position = transform.position;
        controlledDrones.Add(drone);
        LandDrone(drone);
    }

    public void LandDrone(Drone drone)
    {
        drone.myHangar = this;
        drone.isDocked = true;
        drone.currentJob = new DroneJob(DroneJobType.None, Vector3.zero);
        dockedDrones.Add(drone);
        drone.rb.simulated = false;
        drone.sr.enabled = false;
        drone.lights.SetActive(false);
        //drone.SetNewAction(drone.LandInHangar);
    }

    public void RemoveDrone(Drone drone)
    {
        if (controlledDrones.Contains(drone))
            controlledDrones.Remove(drone);
        if (dockedDrones.Contains(drone))
            dockedDrones.Remove(drone);

        RemoveDroneFromJob(drone);
    }

    public void RemoveDroneFromJob(Drone drone)
    {
        if (drone.currentJob == null)
            return;

        switch (drone.droneType)
        {
            case DroneType.Builder:
                ConstructionManager.Instance.IncrementAssignedDrones(drone.currentJob, -1);
                break;
            case DroneType.Attacker:
                IncrementAssignedDrones_Attack(drone.currentJob, -1);
                break;
        }
    }

    public int GetDroneCount(DroneType droneType)
    {
        int count = 0;

        foreach (Drone drone in controlledDrones)
        {
            if (drone.droneType == droneType)
                count++;
        }

        return count;
    }

    IEnumerator RepairDrones()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            // Heal one drone at a time
            foreach(var drone in dockedDrones)
            {
                if (drone.droneHealth.GetHealth() < drone.droneHealth.GetMaxHealth())
                {
                    drone.droneHealth.ModifyHealth(repairAmountPerSec, 1, Vector2.zero, gameObject);
                    break;
                }
            }
        }
    }

    #endregion

#region Jobs

    public DroneJob GetDroneJob(DroneType droneType)
    {
        DroneJob newJob = new DroneJob(DroneJobType.None, Vector2.zero);

        switch (droneType)
        {
            case DroneType.Builder:
                DroneJob job = ConstructionManager.Instance.GetBuildJob();
                if (job == null)
                    job = ConstructionManager.Instance.GetRepairJob();
                
                if (job != null)
                    newJob = job;
                break;

            case DroneType.Attacker:
                newJob = GetAttackJob();
                break;
        }

        return newJob;
    }

    DroneAttackJob GetAttackJob()
    {
        DroneAttackJob job = new DroneAttackJob(DroneJobType.None, null, Vector3.zero);
        int leastDronesAssigned = int.MaxValue;

        for (int i = 0; i < attackJobs.Count; i++)
        {
            int dronesAssigned = attackJobs[i].Value;

            if (attackJobs[i].Key.targetTrans == null)
                return job;

            // Select the job with the fewest drones assigned
            if (dronesAssigned < leastDronesAssigned)
            {
                job = attackJobs[i].Key;
                leastDronesAssigned = dronesAssigned;
            }
        }

        // Increment assigned drones
        IncrementAssignedDrones_Attack(job, 1);

        return job;
    }

    void IncrementAssignedDrones_Attack(DroneJob job, int value)
    {
        if (job.jobType != DroneJobType.None)
        {
            if (job is DroneAttackJob attackJob)
            {
                for (int i = 0; i < attackJobs.Count; i++)
                {
                    if (attackJobs[i].Key == job)
                    {
                        attackJobs[i] = new KeyValuePair<DroneAttackJob, int>(attackJob, attackJobs[i].Value + value);
                        break;
                    }
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D otherCollider)
    {
        if (otherCollider.GetComponent<EnemyBase>() != null)
            attackJobs.Add(new KeyValuePair<DroneAttackJob, int>(new DroneAttackJob(DroneJobType.Attack, otherCollider.transform, otherCollider.transform.position), 0));
    }

    void OnTriggerExit2D(Collider2D otherCollider)
    {
        if (otherCollider.GetComponent<EnemyBase>() == null)
            return;

        List<KeyValuePair<DroneAttackJob, int>> pairsToRemove = new List<KeyValuePair<DroneAttackJob, int>>();
    
        foreach (var kvp in attackJobs)
        {
            if (otherCollider.transform == kvp.Key.targetTrans)
            {
                pairsToRemove.Add(kvp);
                UnassignDrones_Attack(kvp.Key);
            }
        }

        foreach (var item in pairsToRemove)
        {
            attackJobs.Remove(item);
        }
    }

    void UnassignDrones_Attack(DroneJob job)
    {
        foreach (Drone drone in controlledDrones)
        {
            if (drone.currentJob == job)
            {
                drone.SetJobDone();
            }
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
                if (dockedDrone.currentJob.jobType != DroneJobType.None || !dockedDrone.hasPower)
                    continue;

                job = GetDroneJob(dockedDrone.droneType);

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
            yield return new WaitForSeconds(launchDelay);
        }
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
    public DroneBuildJob(DroneJobType jobType, Vector3 position) : base(jobType, position)
    { }
}

[System.Serializable]
public class DroneAttackJob : DroneJob
{
    public Transform targetTrans;
    public DroneAttackJob(DroneJobType jobType, Transform targetTrans, Vector3 position) : base(jobType, position)
    {
        this.jobType = jobType;
        this.targetTrans = targetTrans;
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

#endregion