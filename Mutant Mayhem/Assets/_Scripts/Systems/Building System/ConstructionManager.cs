using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

public class ConstructionManager : MonoBehaviour
{
    public List<DroneBuildJob> buildJobs;
    public List<DroneJob> repairJobs;

    TileManager tileManager;
    BuildingSystem buildingSystem;

    public static ConstructionManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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

    public DroneBuildJob GetBuildJob()
    {
        DroneBuildJob job;

        if (buildJobs.Count > 0)
        {
            job = buildJobs[0];
            buildJobs.Remove(job);
            return job;
        }
        else 
            return null;
    }

    public DroneJob GetRepairJob()
    {
        DroneJob job;

        if (repairJobs.Count > 0)
        {
            job = repairJobs[0];
            repairJobs.Remove(job);
            return job;
        }
        else
            return null;
    }

    public bool BuildBlueprint(DroneJob buildJob, float buildAmount)
    {
        if (buildJob is DroneBuildJob job)
        if (tileManager.BuildBlueprintAt(job.jobPosition, buildAmount))
        {
            return true;
        }

        return false;
    }

    public void RemoveBuildJobAt(Vector2 pos)
    {
        bool found = false;
        foreach (DroneBuildJob job in buildJobs)
        {
            if (job.jobPosition == pos)
            {
                if (found == true)
                    Debug.LogError($"ConstructionManager: Found multiple building jobs at {pos}");
                found = true;
                buildJobs.Remove(job);
            }
        }

        if (found == false)
            Debug.LogError($"ConstructionManager: no building job found at {pos} to remove");
    }

    public void RemoveRepairJobAt(Vector2 pos)
    {
        bool found = false;
        foreach (DroneJob job in repairJobs)
        {
            if (job.jobPosition == pos)
            {
                if (found == true)
                    Debug.LogError($"ConstructionManager: Found multiple repair jobs at {pos}");
                found = true;
                repairJobs.Remove(job);
            }
        }

        if (found == false)
            Debug.LogError($"ConstructionManager: no repair job found at {pos} to remove");
    }

    public DroneBuildJob GetBuildJobAt(Vector2 pos)
    {
        foreach (DroneBuildJob job in buildJobs)
        {
            if (job.jobPosition == pos)
                return job;
        }

        return null;
    }
}
