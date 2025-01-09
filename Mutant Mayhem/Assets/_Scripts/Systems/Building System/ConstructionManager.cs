using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

public class ConstructionManager : MonoBehaviour
{
    public List<DroneBuildJob> buildJobs = new List<DroneBuildJob>();
    public List<DroneJob> repairJobs = new List<DroneJob>();

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

    public DroneBuildJob GetBuildJob()
    {
        DroneBuildJob job;

        if (buildJobs.Count > 0)
        {
            job = buildJobs[0];
            //buildJobs.Remove(job);
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
            //repairJobs.Remove(job);
            return job;
        }
        else
            return null;
    }

    public bool BuildBlueprint(DroneBuildJob buildJob, float buildAmount)
    {
        if (tileManager.BuildBlueprintAt(buildJob, buildAmount))
        {
            return true;
        }

        return false;
    }

    public void RemoveBuildJobAt(Vector2 pos)
    {
        List<DroneBuildJob> jobs = new List<DroneBuildJob>();
        foreach (DroneBuildJob job in buildJobs)
        {
            if (job.jobPosition == pos)
            {
                jobs.Add(job);
            }
        }

        if (jobs.Count > 1)
            Debug.LogError($"ConstructionManager: Found multiple building jobs at {pos}");

        if (jobs.Count == 0)
            Debug.LogError($"ConstructionManager: no building job found at {pos} to remove");

        foreach (DroneBuildJob found in jobs)
            buildJobs.Remove(found);
    }

    public void RemoveRepairJobAt(Vector2 pos)
    {
        List<DroneJob> jobs = new List<DroneJob>();;
        foreach (DroneJob job in repairJobs)
        {
            if (job.jobPosition == pos)
            {
                jobs.Add(job);
            }
        }

        if (jobs.Count > 1)
            Debug.LogError($"ConstructionManager: Found multiple repair jobs at {pos}");

        if (jobs.Count == 0)
            Debug.LogError($"ConstructionManager: no repair job found at {pos} to remove");

        foreach (DroneBuildJob found in jobs)
            repairJobs.Remove(found);
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
