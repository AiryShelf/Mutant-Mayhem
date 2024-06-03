using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class UpgradeSystem : MonoBehaviour
{
    public Player player;
    public TileManager tileManager;

    public Dictionary<UpgradeType, int> upgradeLevels = 
        new Dictionary<UpgradeType, int>();
    private Dictionary<UpgradeType, int> upgradeBaseCosts = 
        new Dictionary<UpgradeType, int>();
    public Dictionary<UpgradeType, int> upgradeCurrentCosts = 
        new Dictionary<UpgradeType, int>();

    void Awake()
    {
        // Base Upgrade Costs
        upgradeBaseCosts[UpgradeType.MoveSpeed] = 100;
        upgradeBaseCosts[UpgradeType.StrafeSpeed] = 100;
        upgradeBaseCosts[UpgradeType.SprintFactor] = 100;
        upgradeBaseCosts[UpgradeType.ReloadSpeed] = 100;
        upgradeBaseCosts[UpgradeType.MeleeDamage] = 100;
        upgradeBaseCosts[UpgradeType.MeleeKnockback] = 100;
        upgradeBaseCosts[UpgradeType.MeleeAttackRate] = 100;
        upgradeBaseCosts[UpgradeType.StaminaMax] = 100;
        upgradeBaseCosts[UpgradeType.StaminaRegen] = 100;
        upgradeBaseCosts[UpgradeType.HealthMax] = 100;
        upgradeBaseCosts[UpgradeType.HealthRegen] = 100;
        upgradeBaseCosts[UpgradeType.Accuracy] = 100;

        // Initialize currentCosts
        foreach (KeyValuePair<UpgradeType, int> kvp in upgradeBaseCosts)
        {
            upgradeCurrentCosts.Add(kvp.Key, kvp.Value);
        }

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
            case UpgradeType.MeleeKnockback:
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
        int cost = upgradeCurrentCosts[upgradeType];
        if (BuildingSystem.PlayerCredits >= cost)
        {
            // Buy upgrade
            BuildingSystem.PlayerCredits -= cost;
            upgradeLevels[upgradeType]++;

            // Apply the upgrade to stats
            int currentLevel = upgradeLevels[upgradeType];
            upgrade.Apply(player.stats, upgradeLevels[upgradeType]);
            
            // Set cost to next level
            int baseCost = upgradeBaseCosts[upgradeType];
            upgradeCurrentCosts[upgradeType] = 
                upgrade.CalculateCost(baseCost, currentLevel + 1);

            Debug.Log("Upgrade Applied of type: " + upgradeType);
        }
        else
        {
            Debug.Log("Not enough Credits");
        }
    }
}
