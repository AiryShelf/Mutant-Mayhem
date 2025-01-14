using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Task_CloseUpgrades : Task
{
    QCubeController qCubeController;
    bool wasOpen;

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
            wasOpen = true;
        }
        else if (wasOpen)
        {
            progress = 1;
            SetTaskComplete();
        }

        UpdateProgressText();
    }
}
