using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

public class ConstructionManager : MonoBehaviour
{
    [SerializeField] int maxDronesPerJob;
    // Lists track jobs and number of assigned drones
    List<KeyValuePair<DroneBuildJob, int>> buildJobs = new List<KeyValuePair<DroneBuildJob, int>>();
    Dictionary<Vector2, (DroneJob job, int count)> repairJobs = new Dictionary<Vector2, (DroneJob, int)>();

    TileManager tileManager;
    BuildingSystem buildingSystem;

    public static ConstructionManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    } 

    void Start()
    {
        tileManager = FindObjectOfType<TileManager>();
        buildingSystem = FindObjectOfType<BuildingSystem>();
    }

    #region Add Job

    public void AddBuildJob(DroneBuildJob buildJob)
    {
        foreach (var kvp in buildJobs)
        {
            if (kvp.Key.jobPosition == buildJob.jobPosition)
            {
                //Debug.LogWarning($"ContructionManager: Tired to add a BuildJob that already exists at {buildJob.jobPosition}!");
                return;
            }
        }

        buildJobs.Add(new KeyValuePair<DroneBuildJob, int>(buildJob, 0));
        Debug.Log($"ConstructionManager: Added buildJob at: {buildJob.jobPosition}");
    }

    public void AddRepairJob(DroneJob repairJob)
    {
        repairJobs[repairJob.jobPosition] = (repairJob, 0);
        //Debug.Log($"ConstructionManager: Added repairJob at: {repairJob.jobPosition}");
    }

    /*
    public void InsertRepairJob(DroneJob repairJob)
    {
        foreach (var kvp in repairJobs)
        {
            if (kvp.Key.jobPosition == repairJob.jobPosition)
            {
                Debug.LogError("ContructionManager: Tired to add a RepairJob that already exists in the queue!");
                return;
            }
        }

        repairJobs.Insert(0, new KeyValuePair<DroneJob, int>(repairJob, 0));
        Debug.Log($"ConstructionManager: Inserted repairJob at: {repairJob.jobPosition}");
    }
    */

    #endregion

    #region Remove Job

    public void TileRemoved(Vector2 pos)
    {
        // Remove jobs from the buildJobs list
        int buildJobsRemoved = buildJobs.RemoveAll(kvp => kvp.Key.jobPosition == pos);

        if (buildJobsRemoved > 1)
        {
            Debug.LogError($"ConstructionManager: Removed {buildJobsRemoved} build job(s) at position {pos}");
        }

        // Remove jobs from the repairJobs list
        repairJobs.Remove(pos);
    }

    public void RemoveBuildJob(DroneBuildJob jobToRemove)
    {
        // Remove all jobs matching the specified key
        int removedCount = buildJobs.RemoveAll(kvp => kvp.Key == jobToRemove);

        if (removedCount > 1)
            Debug.LogError($"ConstructionManager: Found and removed multiple building jobs for the same key: {jobToRemove}");

        if (removedCount == 0)
            Debug.LogError($"ConstructionManager: No building job found for the key: {jobToRemove}");
    }

    public void RemoveBuildJob(Vector2 pos)
    {
        // Remove all jobs matching the specified key
        int removedCount = buildJobs.RemoveAll(kvp => kvp.Key.jobPosition == pos);

        if (removedCount > 1)
            Debug.LogError($"ConstructionManager: Found and removed multiple building jobs at: {pos}");

        if (removedCount == 0)
            Debug.LogError($"ConstructionManager: No building job found to remove at: {pos}");
    }

    public void RemoveRepairJob(DroneJob jobToRemove)
    {
        repairJobs.Remove(jobToRemove.jobPosition);
    }

    public void RemoveRepairJob(Vector2 positionToRemove)
    {
        repairJobs.Remove(positionToRemove);
    }

    #endregion

    #region Get Job

    public DroneBuildJob GetBuildJob()
    {
        DroneBuildJob job = null;
        int leastDronesAssigned = int.MaxValue;

        for (int i = 0; i < buildJobs.Count; i++)
        {
            int dronesAssigned = buildJobs[i].Value;

            // Select the job with the fewest drones assigned
            if (dronesAssigned < leastDronesAssigned &&
                dronesAssigned < maxDronesPerJob)
            {
                job = buildJobs[i].Key;
                leastDronesAssigned = dronesAssigned;
            }
        }

        // Increment assigned drones
        if (job != null)
        {
            IncrementAssignedDrones(job, 1);
        }

        Debug.Log("ConstructionManager: GetBuildJob job at: " + job?.jobPosition);
        return job;
    }

    public DroneJob GetRepairJob()
    {
        DroneJob job = new DroneJob(DroneJobType.None, Vector2.zero);
        int leastDronesAssigned = int.MaxValue;

        foreach (var repairJob in repairJobs)
        {
            int dronesAssigned = repairJob.Value.count;

            // Select the job with the fewest drones assigned
            if (dronesAssigned < leastDronesAssigned &&
                dronesAssigned < maxDronesPerJob)
            {
                job = repairJob.Value.job;
                leastDronesAssigned = dronesAssigned;
            }
        }

        // Increment the assigned drones count for the selected job
        if (job != null)
        {
            IncrementAssignedDrones(job, 1);
        }

        return job;
    }

    public void IncrementAssignedDrones(DroneJob job, int value)
    {
        if (job is DroneBuildJob buildJob)
        {
            for (int i = 0; i < buildJobs.Count; i++)
            {
                if (buildJobs[i].Key == job)
                {
                    buildJobs[i] = new KeyValuePair<DroneBuildJob, int>(buildJob, buildJobs[i].Value + value);
                    Debug.Log("ConstructionManager: Incremented assigned drones for build job at: " + job.jobPosition + " to " + buildJobs[i].Value);
                    break;
                }
            }
        }
        else
        {
            if (repairJobs.ContainsKey(job.jobPosition))
            {
                repairJobs[job.jobPosition] = (job, repairJobs[job.jobPosition].count + value);
                Debug.Log("ConstructionManager: Incremented assigned drones for repair job at: " + job.jobPosition + " to " + repairJobs[job.jobPosition].count);
            }
        }
    }

    /*
    public DroneJob GetNearestJob(Vector2 pos)
    {
        DroneJob closestJob = null;
        float closestDistance = Mathf.Infinity;

        // Combine build and repair jobs into a single loop for efficiency
        foreach (var kvp in buildJobs)
        {
            float distance = Vector2.Distance(pos, kvp.Key.jobPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestJob = kvp.Key;
            }
        }

        foreach (var kvp in repairJobs)
        {
            float distance = Vector2.Distance(pos, kvp.Key.jobPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestJob = kvp.Key;
            }
        }

        return closestJob;
    }
    */

    public bool CheckIfBuildJobExists(DroneBuildJob buildJob)
    {
        foreach (var kvp in buildJobs)
        {
            if (kvp.Key == buildJob)
                return true;
        }

        return false;
    }

    public bool CheckIfRepairJobExists(DroneJob repairJob)
    {
        foreach (var kvp in repairJobs)
        {
            if (kvp.Value.job == repairJob)
                return true;
        }

        return false;
    }

    #endregion

    #region Do Job

    public bool BuildBlueprint(Vector2 pos, float buildAmount, Vector2 hitDir)
    {
        
        Vector3Int gridPos = tileManager.WorldToGrid(pos);
        if (!tileManager.CheckBlueprintCellsAreClear(gridPos))
        {
            TextFly textFly = PoolManager.Instance.GetFromPool("TextFlyWorld_Health").GetComponent<TextFly>();
            textFly.transform.position = pos;
            textFly.Initialize("Blocked!", Color.red, 
                               1, hitDir.normalized, true, 1.2f);
            return false;
        }

        StatsCounterPlayer.AmountRepairedByDrones += buildAmount;

        if (tileManager.BuildBlueprintAt(pos, buildAmount, 1.2f, hitDir))
        {
            //RemoveBuildJob(buildJob);
            return true;
        }

        return false;
    }

    public bool RepairUntilComplete(Vector2 pos, float value, Vector2 hitDir)
    {
        Vector3Int gridPos = tileManager.WorldToGrid(pos);

        if (!tileManager.ContainsTileKey(gridPos))
            return true; // Job is missing or complete

        float repairCost = tileManager.GetRepairCostAt(pos, value);
        if (BuildingSystem.PlayerCredits >= repairCost)
        {
            // modify health
            tileManager.ModifyHealthAt(pos, value, 1f, hitDir);
            StatsCounterPlayer.AmountRepairedByDrones += value;
            BuildingSystem.PlayerCredits -= repairCost;
        }
        else
        {
            MessageBanner.PulseMessage("Not enough Credits to repair!", Color.red);
            // NEED LOGIG HERE for keeping the job but sending drones home....
            return false;
        }
        
        if (tileManager.GetTileHealthRatio(tileManager.GridToRootPos(gridPos)) <= 0)
            return true; // Job is complete
        
        return false;
    }

    #endregion
}
