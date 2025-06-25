using System;
using System.Collections;
using UnityEngine;

public class Task_BuyPlayerStatsUpgrade : Task
{
    [Header("Buy Player Stats Upgrade")]
    [SerializeField] PlayerStatsUpgrade playerStatsUpgrade;
    [SerializeField] int levelToReach;
    UpgradeManager upgradeManager;

    void Start()
    {
        upgradeManager = FindObjectOfType<UpgradeManager>();
        if (upgradeManager == null)
        {
            Debug.LogError("BuyStaminaRegen Objective could not find the UpgradeManager");
            return;
        }
        UpdateProgressText();
    }

    void FixedUpdate()
    {
        if (isComplete) 
            return;
        
        progress = (float)upgradeManager.playerStatsUpgLevels[playerStatsUpgrade] / levelToReach;

        if (upgradeManager.playerStatsUpgLevels[playerStatsUpgrade] >= levelToReach)
        {
            progress = 1;
            SetTaskComplete();
        }

        UpdateProgressText();
    }
}
