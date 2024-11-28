using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Task_RotateStructure : Task
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

        if (buildingSystem.currentRotation != 0)
        {
            progress = 1;
            SetTaskComplete();
        }

        UpdateProgressText();
    }
}
