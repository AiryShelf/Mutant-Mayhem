using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionManager : MonoBehaviour
{
    // Lists track jobs and number of assigned drones
    List<KeyValuePair<DroneBuildJob, int>> buildJobs = new List<KeyValuePair<DroneBuildJob, int>>();
    List<KeyValuePair<DroneJob, int>> repairJobs = new List<KeyValuePair<DroneJob, int>>();

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
                Debug.LogWarning($"ContructionManager: Tired to add a BuildJob that already exists at {buildJob.jobPosition}!");
                return;
            }
        }

        buildJobs.Add(new KeyValuePair<DroneBuildJob, int>(buildJob, 0));
        Debug.Log($"ConstructionManager: Added buildJob at: {buildJob.jobPosition}");
    }

    public void AddRepairJob(DroneJob repairJob)
    {
        foreach (var kvp in repairJobs)
        {
            if (kvp.Key.jobPosition == repairJob.jobPosition)
            {
                Debug.LogWarning($"ContructionManager: Tired to add a RepairJob that already exists at {repairJob.jobPosition}!");
                return;
            }
        }

        repairJobs.Add(new KeyValuePair<DroneJob, int>(repairJob, 0));
        Debug.Log($"ConstructionManager: Added repairJob at: {repairJob.jobPosition}");
    }

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
        int repairJobsRemoved = repairJobs.RemoveAll(kvp => kvp.Key.jobPosition == pos);

        if (repairJobsRemoved > 1)
        {
            Debug.LogError($"ConstructionManager: Removed {repairJobsRemoved} repair job(s) at position {pos}");
        }

        // If no jobs were found in either list
        if (buildJobsRemoved == 0 && repairJobsRemoved == 0)
        {
            Debug.LogWarning($"ConstructionManager: No jobs found at position {pos} to remove.");
        }
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
        int removedCount = repairJobs.RemoveAll(kvp => kvp.Key == jobToRemove);

        if (removedCount > 1)
            Debug.LogError($"ConstructionManager: Found and removed multiple repair jobs for the same key: {jobToRemove}");

        if (removedCount == 0)
            Debug.LogError($"ConstructionManager: No repair job found for the key: {jobToRemove}");
    }

    public void RemoveRepairJob(Vector2 positionToRemove)
    {
        // Remove all jobs matching the specified position
        int removedCount = repairJobs.RemoveAll(kvp => kvp.Key.jobPosition == positionToRemove);

        if (removedCount > 1)
            Debug.LogError($"ConstructionManager: Found and removed multiple repair jobs at the same position: {positionToRemove}");

        if (removedCount == 0)
            Debug.LogError($"ConstructionManager: No repair job found to remove at the position: {positionToRemove}");
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
            if (dronesAssigned < leastDronesAssigned)
            {
                job = buildJobs[i].Key;
                leastDronesAssigned = dronesAssigned;
            }
        }

        // Increment assigned drones
        if (job != null)
        {
            for (int i = 0; i < buildJobs.Count; i++)
            {
                if (buildJobs[i].Key == job)
                {
                    buildJobs[i] = new KeyValuePair<DroneBuildJob, int>(job, buildJobs[i].Value + 1);
                    break;
                }
            }
        }

        return job;
    }

    public DroneJob GetRepairJob()
    {
        DroneJob job = null;
        int leastDronesAssigned = int.MaxValue;

        for (int i = 0; i < repairJobs.Count; i++)
        {
            int dronesAssigned = repairJobs[i].Value;

            // Select the job with the fewest drones assigned
            if (dronesAssigned < leastDronesAssigned)
            {
                job = repairJobs[i].Key;
                leastDronesAssigned = dronesAssigned;
            }
        }

        // Increment the assigned drones count for the selected job
        if (job != null)
        {
            for (int i = 0; i < repairJobs.Count; i++)
            {
                if (repairJobs[i].Key == job)
                {
                    repairJobs[i] = new KeyValuePair<DroneJob, int>(job, repairJobs[i].Value + 1);
                    break;
                }
            }
        }

        return job;
    }

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
            if (kvp.Key == repairJob)
                return true;
        }

        return false;
    }

    #endregion

    #region Do Job

    public bool BuildBlueprint(Vector2 pos, float buildAmount, Vector2 hitDir)
    {
        Vector3Int gridPos = tileManager.WorldToGrid(pos);
        if (tileManager.CheckGridIsClear(gridPos, buildingSystem.layersForBuildClearCheck, false) &&
            tileManager.BuildBlueprintAt(pos, buildAmount, 2, hitDir))
        {
            //RemoveBuildJob(buildJob);
            return true;
        }

        return false;
    }

    public bool RepairTile(Vector2 pos, float value)
    {
        tileManager.ModifyHealthAt(pos, value, 2, GameTools.GetRandomDirection());
        Vector3Int gridPos = tileManager.WorldToGrid(pos);
        if (tileManager.ContainsTileKey(gridPos) && tileManager.GetTileHealthRatio(tileManager.GridToRootPos(gridPos)) <= 0)
        {
            //RemoveRepairJob(job);
            return true;
        }
        
        return false;
    }

    #endregion
}
