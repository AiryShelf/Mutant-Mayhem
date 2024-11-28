using System;
using System.Collections;
using UnityEngine;

public class Task_SelectStructure : Task
{
    BuildingSystem buildingSystem;

    void Start()
    {
        buildingSystem = FindObjectOfType<BuildingSystem>();
        UpdateProgressText();
    }

    void FixedUpdate()
    {
        if (isComplete) 
            return;

        if (buildingSystem.structureInHand.actionType == ActionType.Build)
        {
            progress = 1;
            SetTaskComplete();
        }

        UpdateProgressText();
    }
}
