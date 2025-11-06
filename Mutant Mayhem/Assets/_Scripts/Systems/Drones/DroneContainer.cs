using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneContainer : MonoBehaviour
{
    public List<Drone> controlledDrones = new List<Drone>();
    public List<Drone> dockedDrones = new List<Drone>();
    public int maxDrones;
    [SerializeField] float launchDelay = 0.5f;
    public bool hasPower = true;
    [SerializeField] int numberOfAttackJobs = 0;

    [SerializeField] CircleCollider2D detectionCollider;

    // Tracks num assigned drones
    [SerializeField] List<KeyValuePair<DroneAttackJob, int>> attackJobs = new List<KeyValuePair<DroneAttackJob, int>>();

    void Start()
    {
        StartCoroutine(LookForJobs());
        StartCoroutine(RepairDrones());
        StartCoroutine(EnergyConsumptionAndRecharge());
        DroneManager.Instance.droneContainers.Add(this);
    }

    void OnDestroy()
    {
        StopAllCoroutines();

        List<Drone> dronesToRemove = new List<Drone>(controlledDrones);
        foreach (Drone drone in dronesToRemove)
        {
            if (drone.isDocked)
                drone.Die();
            else
                drone.droneHealth.Die();
        }
        DroneManager.Instance.droneContainers.Remove(this);
    }

    IEnumerator EnergyConsumptionAndRecharge()
    {
        Drone droneBeingRecharged = null;
        while (true)
        {
            yield return new WaitForSeconds(1);

            foreach (Drone drone in controlledDrones)
            {
                if (!drone.hasPower)
                    continue;

                // If docked, recharge energy of one drone at a time
                if (drone.isDocked)
                {
                    if (droneBeingRecharged != null && drone != droneBeingRecharged)
                        continue;

                    drone.SetJob(new DroneJob(DroneJobType.Recharge, Vector2.zero));
                    droneBeingRecharged = drone;
                    drone.energy += DroneManager.Instance.droneHangarRechargeSpeed;
                    if (drone.energy >= drone.energyMax)
                    {
                        drone.energy = drone.energyMax;
                        drone.SetJobDone();
                        droneBeingRecharged = null;
                    }
                }
                else
                {
                    // Consume energy
                    drone.energy -= 1;
                    if (drone.energy <= 0)
                    {
                        drone.energy = 0;
                        if (drone.currentJob.jobType != DroneJobType.None)
                        {
                            drone.SetJobDone();
                        }

                        if (drone.myHangar.hasPower)
                        {
                            drone.PowerOn();
                        }
                        else
                        {
                            drone.PowerOff();
                        }
                    }
                }
            }
        }
    }

    void RefreshColliderRange()
    {
        detectionCollider.radius = DroneManager.Instance.droneHangarRange;
    }

    #region Drones

    public Drone GetDroneToSell(DroneType droneType)
    {
        if (controlledDrones.Count == 0)
            return null;

        // Can only sell docked drones
        if (dockedDrones.Count > 0)
        {
            foreach (Drone drone in dockedDrones)
            {
                if (drone.droneType == droneType)
                    return drone;
            }
        }

        return null;
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

        Debug.Log("DroneContainer: Landed drone of type: " + drone.droneType + ". Job type set to None.");
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

    public int GetTotalDroneCount()
    {
        return controlledDrones.Count;
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

            if (dockedDrones.Count > 0)
            {
                float repairPerDrone = DroneManager.Instance.droneHangarRepairSpeed / dockedDrones.Count;
                // Spread repair among docked drones
                foreach (var drone in dockedDrones)
                {
                    if (drone.droneHealth.GetHealth() < drone.droneHealth.GetMaxHealth())
                    {
                        drone.droneHealth.ModifyHealth(repairPerDrone, 1, Vector2.zero, gameObject);
                        break;
                    }
                }
            }
        }
    }

    #endregion

    #region Jobs

    IEnumerator LookForJobs()
    {
        while (true)
        {
            if (hasPower && dockedDrones.Count > 0)
            {
                RefreshColliderRange();

                DroneJob job = null;
                Drone droneToAssign = null;

                foreach (Drone dockedDrone in dockedDrones)
                {
                    if (dockedDrone.currentJob.jobType != DroneJobType.None || !dockedDrone.hasPower || dockedDrone.energy <= 0)
                        continue;

                    job = GetDroneJob(dockedDrone.droneType);

                    if (job.jobType == DroneJobType.None)
                        continue;

                    droneToAssign = dockedDrone;
                    break;
                }
                if (droneToAssign != null)
                {
                    Debug.Log("DroneContainer: Launching then assigning job of type: " + job.jobType + " to drone of type: " + droneToAssign.droneType);
                    LaunchDrone(droneToAssign);
                    droneToAssign.SetJob(job);
                }
            }

            yield return new WaitForSeconds(launchDelay);
        }
    }

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

        Debug.Log("DroneContainer: GetDroneJob returning job of type: " + newJob.jobType + " for drone type: " + droneType);
        return newJob;
    }

    DroneAttackJob GetAttackJob()
    {
        DroneAttackJob job = new DroneAttackJob(DroneJobType.None, null, Vector3.zero);
        int leastDronesAssigned = int.MaxValue;

        CleanUpAttackJobs();

        for (int i = 0; i < attackJobs.Count; i++)
        {
            int dronesAssigned = attackJobs[i].Value;

            if (attackJobs[i].Key.targetTrans == null)
                continue;

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

    void CleanUpAttackJobs()
    {
        for (int i = attackJobs.Count - 1; i >= 0; i--)
        {
            var key = attackJobs[i].Key;
            // Remove if the job itself is null, the target is gone, or the target is inactive
            if (key == null || key.targetTrans == null || !key.targetTrans.gameObject.activeInHierarchy)
            {
                attackJobs.RemoveAt(i);
                numberOfAttackJobs--;
            }
        }
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
        {
            attackJobs.Add(new KeyValuePair<DroneAttackJob, int>(new DroneAttackJob(DroneJobType.Attack, otherCollider.transform, otherCollider.transform.position), 0));
            numberOfAttackJobs++;
        }
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