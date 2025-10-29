using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Task_SwitchUpgradePanels : Task
{
    UpgradePanelManager panelManager;

    void Start()
    {
        panelManager = FindObjectOfType<QCubeController>().panelSwitcher;
        if (panelManager == null)
        {
            Debug.LogError("Switch Upgrade Panels - task is depricated");
            return;
        }
        UpdateProgressText();
    }

    void FixedUpdate()
    {
        if (isComplete) 
            return;

        
            progress = 1;
            SetTaskComplete();
        

        UpdateProgressText();
    }
}
