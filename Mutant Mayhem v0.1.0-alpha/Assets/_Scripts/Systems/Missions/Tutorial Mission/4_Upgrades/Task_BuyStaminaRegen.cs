using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Task_BuyStaminaRegen : Task
{
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
        
        progress = (float)upgradeManager.playerStatsUpgLevels[PlayerStatsUpgrade.StaminaRegen] / levelToReach;

        if (upgradeManager.playerStatsUpgLevels[PlayerStatsUpgrade.StaminaRegen] >= levelToReach)
        {
            progress = 1;
            SetTaskComplete();
        }

        UpdateProgressText();
    }
}
