using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Task_CloseBuildMenu : Task
{
    BuildingSystem buildingSystem;
    bool buildWasOpen = false;

    void Start()
    {
        buildingSystem = FindObjectOfType<BuildingSystem>();
        UpdateProgressText();
    }

    void FixedUpdate()
    {
        if (isComplete) 
            return;

        if (buildingSystem.isInBuildMode)
        {
            buildWasOpen = true;
        }
        else if (buildWasOpen)
        {
            progress = 1;
            SetTaskComplete();
        }

        UpdateProgressText();
    }
}
