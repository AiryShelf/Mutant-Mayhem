using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Task_OpenUpgrades : Task
{
    QCubeController qCubeController;

    void Start()
    {
        qCubeController = FindObjectOfType<QCubeController>();
        if (qCubeController == null)
        {
            Debug.LogError("OpenUpgrades Objective could not find the Cube!");
            return;
        }
        UpdateProgressText();
    }

    void FixedUpdate()
    {
        if (isComplete) 
            return;

        if (qCubeController.isUpgradesOpen)
        {
            progress = 1;
            SetTaskComplete();
        }

        UpdateProgressText();
    }
}
