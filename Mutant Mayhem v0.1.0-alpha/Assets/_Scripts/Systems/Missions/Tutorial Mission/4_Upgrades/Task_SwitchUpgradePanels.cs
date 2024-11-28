using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Task_SwitchUpgradePanels : Task
{
    PanelSwitcher panelSwitcher;

    void Start()
    {
        panelSwitcher = FindObjectOfType<QCubeController>().panelSwitcher;
        if (panelSwitcher == null)
        {
            Debug.LogError("Objective could not find PanelSwitcher");
            return;
        }
        UpdateProgressText();
    }

    void FixedUpdate()
    {
        if (isComplete) 
            return;

        if (panelSwitcher.currentPanelIndex != 0)
        {
            progress = 1;
            SetTaskComplete();
        }

        UpdateProgressText();
    }
}
