using System;
using System.Collections;
using UnityEngine;

public class Task_BuyStructureStatsUpgrade : Task
{
    [Header("Buy Structure Stats Upgrade")]
    [SerializeField] StructureStatsUpgrade structureStatsUpgrade;
    [SerializeField] int levelToReach;
    UpgradeManager upgradeManager;

    void Start()
    {
        upgradeManager = FindObjectOfType<UpgradeManager>();
        if (upgradeManager == null)
        {
            Debug.LogError("Objective could not find the UpgradeManager");
            return;
        }
        UpdateProgressText();
    }

    void FixedUpdate()
    {
        if (isComplete) 
            return;
        
        progress = (float)upgradeManager.structureStatsUpgLevels[structureStatsUpgrade] / levelToReach;

        if (upgradeManager.structureStatsUpgLevels[structureStatsUpgrade] >= levelToReach)
        {
            progress = 1;
            SetTaskComplete();
        }

        UpdateProgressText();
    }
}
