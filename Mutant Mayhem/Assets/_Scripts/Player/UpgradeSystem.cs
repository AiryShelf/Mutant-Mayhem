using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;



public class UpgradeSystem : MonoBehaviour
{
    public Player player;
    public TileManager tileManager;

    private Dictionary<UpgradeType, int> upgradeLevels = new Dictionary<UpgradeType, int>();
    private Dictionary<UpgradeType, int> upgradeBaseCosts = new Dictionary<UpgradeType, int>();

    void Awake()
    {
        // Upgrade Costs
        upgradeBaseCosts[UpgradeType.MoveSpeed] = 100;
        upgradeBaseCosts[UpgradeType.StrafeSpeed] = 100;
        upgradeBaseCosts[UpgradeType.SprintFactor] = 100;
        upgradeBaseCosts[UpgradeType.ReloadSpeed] = 100;
        upgradeBaseCosts[UpgradeType.MeleeDamage] = 100;
        upgradeBaseCosts[UpgradeType.Knockback] = 100;
        upgradeBaseCosts[UpgradeType.MeleeAttackRate] = 100;
        upgradeBaseCosts[UpgradeType.StaminaMax] = 100;
        upgradeBaseCosts[UpgradeType.StaminaRegen] = 100;
        upgradeBaseCosts[UpgradeType.HealthMax] = 100;
        upgradeBaseCosts[UpgradeType.HealthRegen] = 100;
        upgradeBaseCosts[UpgradeType.Accuracy] = 100;
    }

    void Start()
    {
        // Initialize upgrade levels
        foreach(UpgradeType type in Enum.GetValues(typeof(UpgradeType)))
        {
            upgradeLevels[type] = 0;
        }
    }

    Upgrade CreateUpgrade(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.MoveSpeed:
                return new MoveSpeedUpgrade();
            case UpgradeType.StrafeSpeed:
                return new StrafeSpeedUpgrade();
            case UpgradeType.SprintFactor:
                return new SprintFactorUpgrade();
            case UpgradeType.ReloadSpeed:
                return new ReloadSpeedUpgrade();
            case UpgradeType.MeleeDamage:
                return new MeleeDamageUpgrade();
            case UpgradeType.Knockback:
                return new KnockbackUpgrade();
            case UpgradeType.MeleeAttackRate:
                return new MeleeAttackRateUpgrade();
            case UpgradeType.StaminaMax:
                return new StaminaMaxUpgrade();
            case UpgradeType.StaminaRegen:
                return new StaminaRegenUpgrade();
            case UpgradeType.HealthMax:
                return new HealthMaxUpgrade();
            case UpgradeType.HealthRegen:
                return new HealthRegenUpgrade();
            case UpgradeType.Accuracy:
                return new AccuracyUpgrade();
            default:
                return null;
        }
    }

    public void OnUpgradeButtonClicked(UpgradeType upgradeType)
    {
        // Null check
        Upgrade upgrade = CreateUpgrade(upgradeType);
        if (upgrade == null)
        {
            Debug.LogError("Upgrade not found for type: " + upgradeType);
            return;
        }

        // Check if player can afford upgrade
        int currentLevel = upgradeLevels[upgradeType];
        int baseCost = upgradeBaseCosts[upgradeType];
        int cost = upgrade.CalculateCost(baseCost, currentLevel);

        if (BuildingSystem.PlayerCredits >= cost)
        {
            BuildingSystem.PlayerCredits -= cost;

            upgradeLevels[upgradeType]++;

            upgrade.Apply(player.stats, upgradeLevels[upgradeType]);
        }
        else
        {
            Debug.Log("Not enough Credits");
        }
        

        // Apply the upgrade

    }
}
