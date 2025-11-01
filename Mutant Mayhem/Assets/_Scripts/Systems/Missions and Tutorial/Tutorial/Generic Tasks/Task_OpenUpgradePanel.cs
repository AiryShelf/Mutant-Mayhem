using System;
using System.Collections;
using UnityEngine;

public class Task_OpenUpgradePanel : Task
{
    [Header("Task Settings")]
    public StructureType structureToOpen;

    void Start()
    {
        UpdateProgressText();
    }

    void FixedUpdate()
    {
        if (isComplete)
            return;

        if (UpgradePanelManager.Instance.isOpen &&
            UpgradePanelManager.Instance.currentPanel != null &&
            UpgradePanelManager.Instance.currentPanel.structureToBuildForUnlock == structureToOpen)
        {
            progress = 1;
            SetTaskComplete();
        }

        UpdateProgressText();
    }
}
