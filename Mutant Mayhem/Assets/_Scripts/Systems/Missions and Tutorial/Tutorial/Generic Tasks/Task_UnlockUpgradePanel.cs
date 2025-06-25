using System;
using System.Collections;
using UnityEngine;

public class Task_UnlockUpgradePanel : Task
{
    [Header("Unlock Upgrade Panel")]
    [SerializeField] string upgPanel_techUnlockMessageName;
    [Header("Optional")]
    [SerializeField] string OR_UpgPanel_techUnlockMessageName;
    QCubeController qCubeController;
    PanelSwitcher panelSwitcher;
    UiUpgradePanel panelToUnlock;
    UiUpgradePanel OR_panelTounlock;
    bool useOR = false;

    void Start()
    {
        qCubeController = FindObjectOfType<QCubeController>();
        panelSwitcher = qCubeController.panelSwitcher;

        if (!string.IsNullOrEmpty(OR_UpgPanel_techUnlockMessageName))
            useOR = true;

        // Find panel index to unlock
        bool found = false;
        for (int i = 0; i < panelSwitcher.panels.Length; i++)
        {
            if (panelSwitcher.panels[i] is UiUpgradePanel upgPanel)
            {
                if (upgPanel.techUnlockMessageName == upgPanel_techUnlockMessageName)
                {
                    panelToUnlock = upgPanel;
                    found = true;
                }
                else if (useOR && upgPanel.techUnlockMessageName == OR_UpgPanel_techUnlockMessageName)
                {
                    OR_panelTounlock = upgPanel;
                    found = true;
                }
            }
        }

        if (!found)
            Debug.LogError("Objective_UnlockUpgradePanel could not find panel to unlock");
        
        UpdateProgressText();
    }

    void FixedUpdate()
    {
        if (isComplete) 
            return;

        
        if (panelToUnlock.isUnlocked)
        {
            progress = 1;
            SetTaskComplete();
        }
        else if (useOR)
        {
            if (OR_panelTounlock.isUnlocked)
            {
                progress = 1;
                SetTaskComplete();
            }
        }

        UpdateProgressText();
    }
}
