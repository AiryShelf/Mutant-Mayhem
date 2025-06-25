using System;
using System.Collections;
using UnityEngine;

public class Task_BuyGunStatsUpgrade : Task
{
    [Header("Buy Gun Stats Upgrade")]
    [SerializeField] GunType gunType;
    [SerializeField] GunStatsUpgrade gunStatsUpgrade;
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
        
        switch (gunType)
        {
            case GunType.Laser:
                progress = (float)upgradeManager.laserUpgLevels[gunStatsUpgrade] / levelToReach;
                break;
            case GunType.Bullet:
                progress = (float)upgradeManager.bulletUpgLevels[gunStatsUpgrade] / levelToReach;
                break;
            case GunType.RepairGun:
                progress = (float)upgradeManager.repairGunUpgLevels[gunStatsUpgrade] / levelToReach;
                break;
            default:
                Debug.LogError("Unknown gun type: " + gunType);
                return;
        }

        if (progress >= 1)
        {
            progress = 1;
            SetTaskComplete();
        }

        UpdateProgressText();
    }
}
