using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Tilemaps;

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

    public void AddBuildJob(DroneBuildJob buildJob)
    {
        foreach (var kvp in buildJobs)
        {
            if (kvp.Key == buildJob)
            {
                Debug.LogError("ContructionManager: Tired to add a BuildJob that already exists in the queue!");
                return;
            }
        }

        buildJobs.Add(new KeyValuePair<DroneBuildJob, int>(buildJob, 0));
    }

    public void AddRepairJob(DroneJob repairJob)
    {
        foreach (var kvp in buildJobs)
        {
            if (kvp.Key == repairJob)
            {
                Debug.LogError("ContructionManager: Tired to add a BuildJob that already exists in the queue!");
                return;
            }
        }

        repairJobs.Add(new KeyValuePair<DroneJob, int>(repairJob, 0));
    }

    public void TileRemoved(Vector2 pos)
    {
        // Remove jobs from the buildJobs list
        int buildJobsRemoved = buildJobs.RemoveAll(kvp => kvp.Key.jobPosition == pos);

        if (buildJobsRemoved > 0)
        {
            Debug.Log($"ConstructionManager: Removed {buildJobsRemoved} build job(s) at position {pos}");
        }

        // Remove jobs from the repairJobs list
        int repairJobsRemoved = repairJobs.RemoveAll(kvp => kvp.Key.jobPosition == pos);

        if (repairJobsRemoved > 0)
        {
            Debug.Log($"ConstructionManager: Removed {repairJobsRemoved} repair job(s) at position {pos}");
        }

        // If no jobs were found in either list
        if (buildJobsRemoved == 0 && repairJobsRemoved == 0)
        {
            Debug.LogWarning($"ConstructionManager: No jobs found at position {pos} to remove.");
        }
    }

    public DroneBuildJob GetBuildJob()
    {
        DroneBuildJob job = null;
        int leastDronesAssigned = int.MaxValue;

        // Iterate from the start of the list (oldest jobs first)
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

        if (repairJobs.Count > 0)
        {
            //job = repairJobs[0];
            //repairJobs.Remove(job);
            return job;
        }
        else
            return job;
    }

    public bool BuildBlueprint(DroneBuildJob buildJob, float buildAmount)
    {
        if (tileManager.BuildBlueprintAt(buildJob, buildAmount))
        {
            RemoveBuildJob(buildJob);
            return true;
        }

        return false;
    }

    public bool RepairTileAt(Vector2 pos, float value)
    {
        tileManager.ModifyHealthAt(pos, value, 2, GameTools.GetRandomDirection());
        if (tileManager.GetTileHealthRatio(tileManager.GridToRootPos(tileManager.WorldToGrid(pos))) <= 0)
            return true;
        
        return false;
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

    public void RemoveRepairJobAt(DroneJob jobToRemove)
    {
        int removedCount = repairJobs.RemoveAll(kvp => kvp.Key == jobToRemove);

        if (removedCount > 1)
            Debug.LogError($"ConstructionManager: Found and removed multiple repair jobs for the same key: {jobToRemove}");

        if (removedCount == 0)
            Debug.LogError($"ConstructionManager: No repair job found for the key: {jobToRemove}");
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
}
