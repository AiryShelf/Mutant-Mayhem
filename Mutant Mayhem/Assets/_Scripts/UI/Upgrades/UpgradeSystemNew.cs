using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class UpgradeSystem : MonoBehaviour
{
    [SerializeField] List<UpgradeType> playerStatsEnums;
    [SerializeField] List<UpgradeType> laserPistolEnums;
    [SerializeField] List<UpgradeType> SMGEnums;
    public Player player;
    public PlayerShooter playerShooter;
    public TileManager tileManager;

    MessagePanel messagePanel;

    #region Upgrade Dicts

    // PlayerStats
    private Dictionary<UpgradeType, int> playerStatsUpgMaxLevels = 
        new Dictionary<UpgradeType, int>();
    public Dictionary<UpgradeType, int> playerStatsUpgLevels = 
        new Dictionary<UpgradeType, int>();
    private Dictionary<UpgradeType, int> playerStatsUpgBaseCosts = 
        new Dictionary<UpgradeType, int>();
    public Dictionary<UpgradeType, int> playerStatsUpgCurrCosts = 
        new Dictionary<UpgradeType, int>();

    // Laser Pistol stats
    private Dictionary<UpgradeType, int> laserPistolUpgMaxLevels = 
        new Dictionary<UpgradeType, int>();
    public Dictionary<UpgradeType, int> laserPistolUpgLevels = 
        new Dictionary<UpgradeType, int>();
    private Dictionary<UpgradeType, int> laserPistolUpgBaseCosts = 
        new Dictionary<UpgradeType, int>();
    public Dictionary<UpgradeType, int> laserPistolUpgCurrCosts = 
        new Dictionary<UpgradeType, int>();

    // SMG stats
    private Dictionary<UpgradeType, int> SMGUpgMaxLevels = 
        new Dictionary<UpgradeType, int>();
    public Dictionary<UpgradeType, int> SMGUpgLevels = 
        new Dictionary<UpgradeType, int>();
    private Dictionary<UpgradeType, int> SMGUpgBaseCosts = 
        new Dictionary<UpgradeType, int>();
    public Dictionary<UpgradeType, int> SMGUpgCurrCosts = 
        new Dictionary<UpgradeType, int>();
    
    #endregion

    void Awake()
    {
        messagePanel = FindObjectOfType<MessagePanel>();

        #region Upgrade Init

        // Max upgrade levels
        // PlayerStats
        playerStatsUpgMaxLevels[UpgradeType.MoveSpeed] = 100;
        playerStatsUpgMaxLevels[UpgradeType.StrafeSpeed] = 100;
        playerStatsUpgMaxLevels[UpgradeType.SprintFactor] = 100;
        playerStatsUpgMaxLevels[UpgradeType.PlayerReloadSpeed] = 100;
        playerStatsUpgMaxLevels[UpgradeType.PlayerAccuracy] = 10;
        playerStatsUpgMaxLevels[UpgradeType.MeleeDamage] = 100;
        playerStatsUpgMaxLevels[UpgradeType.MeleeKnockback] = 100;
        playerStatsUpgMaxLevels[UpgradeType.MeleeAttackRate] = 100;
        playerStatsUpgMaxLevels[UpgradeType.StaminaMax] = 100;
        playerStatsUpgMaxLevels[UpgradeType.StaminaRegen] = 100;
        playerStatsUpgMaxLevels[UpgradeType.HealthMax] = 100;
        playerStatsUpgMaxLevels[UpgradeType.HealthRegen] = 100;

        // Gun Stats
        laserPistolUpgMaxLevels[UpgradeType.GunDamage] = 100;
        laserPistolUpgMaxLevels[UpgradeType.GunKnockback] = 100;
        laserPistolUpgMaxLevels[UpgradeType.ShootSpeed] = 10;
        laserPistolUpgMaxLevels[UpgradeType.ClipSize] = 10;
        laserPistolUpgMaxLevels[UpgradeType.ChargeDelay] = 10;
        laserPistolUpgMaxLevels[UpgradeType.GunAccuracy] = 10;
        laserPistolUpgMaxLevels[UpgradeType.GunRange] = 10;
        laserPistolUpgMaxLevels[UpgradeType.Recoil] = 10;

        SMGUpgMaxLevels[UpgradeType.GunDamage] = 100;
        SMGUpgMaxLevels[UpgradeType.GunKnockback] = 100;
        SMGUpgMaxLevels[UpgradeType.ShootSpeed] = 10;
        SMGUpgMaxLevels[UpgradeType.ClipSize] = 10;
        SMGUpgMaxLevels[UpgradeType.ChargeDelay] = 10;
        SMGUpgMaxLevels[UpgradeType.GunAccuracy] = 10;
        SMGUpgMaxLevels[UpgradeType.GunRange] = 10;
        SMGUpgMaxLevels[UpgradeType.Recoil] = 10;

        // Base upgrade costs
        // PlayerStats
        playerStatsUpgBaseCosts[UpgradeType.MoveSpeed] = 100;
        playerStatsUpgBaseCosts[UpgradeType.StrafeSpeed] = 100;
        playerStatsUpgBaseCosts[UpgradeType.SprintFactor] = 150;
        playerStatsUpgBaseCosts[UpgradeType.PlayerReloadSpeed] = 150;
        playerStatsUpgBaseCosts[UpgradeType.PlayerAccuracy] = 200;
        playerStatsUpgBaseCosts[UpgradeType.MeleeDamage] = 100;
        playerStatsUpgBaseCosts[UpgradeType.MeleeKnockback] = 150;
        playerStatsUpgBaseCosts[UpgradeType.MeleeAttackRate] = 100;
        playerStatsUpgBaseCosts[UpgradeType.StaminaMax] = 100;
        playerStatsUpgBaseCosts[UpgradeType.StaminaRegen] = 200;
        playerStatsUpgBaseCosts[UpgradeType.HealthMax] = 100;
        playerStatsUpgBaseCosts[UpgradeType.HealthRegen] = 200;

        // Gun Stats
        laserPistolUpgBaseCosts[UpgradeType.GunDamage] = 100;
        laserPistolUpgBaseCosts[UpgradeType.GunKnockback] = 150;
        laserPistolUpgBaseCosts[UpgradeType.ShootSpeed] = 200;
        laserPistolUpgBaseCosts[UpgradeType.ClipSize] = 200;
        laserPistolUpgBaseCosts[UpgradeType.ChargeDelay] = 200;
        laserPistolUpgBaseCosts[UpgradeType.GunAccuracy] = 200;
        laserPistolUpgBaseCosts[UpgradeType.GunRange] = 150;
        laserPistolUpgBaseCosts[UpgradeType.Recoil] = 100; 

        SMGUpgBaseCosts[UpgradeType.GunDamage] = 100;
        SMGUpgBaseCosts[UpgradeType.GunKnockback] = 150;
        SMGUpgBaseCosts[UpgradeType.ShootSpeed] = 200;
        SMGUpgBaseCosts[UpgradeType.ClipSize] = 200;
        SMGUpgBaseCosts[UpgradeType.ChargeDelay] = 200;
        SMGUpgBaseCosts[UpgradeType.GunAccuracy] = 200;
        SMGUpgBaseCosts[UpgradeType.GunRange] = 150;
        SMGUpgBaseCosts[UpgradeType.Recoil] = 100; 

        // Initialize currentCosts
        foreach (KeyValuePair<UpgradeType, int> kvp in playerStatsUpgBaseCosts)
        {
            playerStatsUpgCurrCosts.Add(kvp.Key, kvp.Value);
        }

        foreach (KeyValuePair<UpgradeType, int> kvp in laserPistolUpgBaseCosts)
        {
            laserPistolUpgCurrCosts.Add(kvp.Key, kvp.Value);
        }

        foreach (KeyValuePair<UpgradeType, int> kvp in SMGUpgBaseCosts)
        {
            SMGUpgCurrCosts.Add(kvp.Key, kvp.Value);
        }

        // Initialize upgrade levels
        foreach(UpgradeType type in playerStatsEnums)
        {
            playerStatsUpgLevels[type] = 0;
        }

        foreach(UpgradeType type in laserPistolEnums)
        {
            laserPistolUpgLevels[type] = 0;
        }
        foreach(UpgradeType type in SMGEnums)
        {
            SMGUpgLevels[type] = 0;
        }
    }

    #endregion Upgrade Properties

    Upgrade CreateUpgrade(UpgradeType type)
    {
        switch (type)
        {
            // PlayerStats
            case UpgradeType.MoveSpeed:
                return new MoveSpeedUpgrade();
            case UpgradeType.StrafeSpeed:
                return new StrafeSpeedUpgrade();
            case UpgradeType.SprintFactor:
                return new SprintFactorUpgrade();
            case UpgradeType.PlayerReloadSpeed:
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
            case UpgradeType.PlayerAccuracy:
                return new PlayerAccuracyUpgrade();

            // GunSO stats
            case UpgradeType.GunDamage:
                return new GunDamageUpgrade();
            case UpgradeType.GunKnockback:
                return new GunKnockbackUpgrade();
            case UpgradeType.ShootSpeed:
                return new ShootSpeedUpgrade();
            case UpgradeType.ClipSize:
                return new ClipSizeUpgrade();
            case UpgradeType.ChargeDelay:
                return new ChargeDelayUpgrade();
            case UpgradeType.GunAccuracy:
                return new GunAccuracyUpgrade();
            case UpgradeType.GunRange:
                return new RangeUpgrade();
            case UpgradeType.Recoil:
                return new RecoilUpgrade();

            default:
                return null;
        }
    }

    public void OnUpgradeButtonClicked(UpgradeType upgradeType, bool isGunUpgrade, int gunIndex)
    {
        Upgrade upgrade = CreateUpgrade(upgradeType);
        if (upgrade == null)
        {
            Debug.LogError("Upgrade not found for type: " + upgradeType);
            return;
        }

        if (isGunUpgrade)
        {
            ApplyGunUpgrade(upgrade, upgradeType, gunIndex);
        }
        else
        {
            ApplyPlayerUpgrade(upgrade, upgradeType);
        }
    }

    private void ApplyPlayerUpgrade(Upgrade upgrade, UpgradeType upgradeType)
    {
        if (playerStatsUpgLevels[upgradeType] >= playerStatsUpgMaxLevels[upgradeType])
        {
            Debug.Log("Max level reached for: " + upgradeType);
            return;
        }

        int cost = playerStatsUpgCurrCosts[upgradeType];
        if (BuildingSystem.PlayerCredits >= cost)
        {
            BuildingSystem.PlayerCredits -= cost;
            upgrade.Apply(player.stats, playerStatsUpgLevels[upgradeType]);
            playerStatsUpgLevels[upgradeType]++;

            playerStatsUpgCurrCosts[upgradeType] = upgrade.CalculateCost(playerStatsUpgBaseCosts[upgradeType], playerStatsUpgLevels[upgradeType]);
            Debug.Log("Player upgrade applied: " + upgradeType);
        }
        else
        {
            Debug.Log("Not enough credits for: " + upgradeType);
        }
    }

    private void ApplyGunUpgrade(Upgrade upgrade, UpgradeType upgradeType, int gunIndex)
    {
        if (gunIndex < 0 || gunIndex >= playerShooter.gunList.Count)
        {
            Debug.LogError("Invalid gun index: " + gunIndex);
            return;
        }

        GunSO gun = playerShooter.gunList[gunIndex];
        Dictionary<UpgradeType, int> gunUpgLevels;
        Dictionary<UpgradeType, int> gunUpgMaxLevels;
        Dictionary<UpgradeType, int> gunUpgCurrCosts;
        Dictionary<UpgradeType, int> gunUpgBaseCosts;

        // Determine which gun's dictionaries to use
        if (gun.gunType == GunType.LaserPistol)
        {
            gunUpgLevels = laserPistolUpgLevels;
            gunUpgMaxLevels = laserPistolUpgMaxLevels;
            gunUpgCurrCosts = laserPistolUpgCurrCosts;
            gunUpgBaseCosts = laserPistolUpgBaseCosts;
        }
        else if (gun.gunType == GunType.SMG)
        {
            gunUpgLevels = SMGUpgLevels;
            gunUpgMaxLevels = SMGUpgMaxLevels;
            gunUpgCurrCosts = SMGUpgCurrCosts;
            gunUpgBaseCosts = SMGUpgBaseCosts;
        }
        else
        {
            Debug.LogError("Unsupported gun type: " + gun.gunType);
            return;
        }

        if (gunUpgLevels[upgradeType] >= gunUpgMaxLevels[upgradeType])
        {
            Debug.Log("Max level reached for: " + upgradeType);
            return;
        }

        int cost = gunUpgCurrCosts[upgradeType];
        if (BuildingSystem.PlayerCredits >= cost)
        {
            BuildingSystem.PlayerCredits -= cost;
            upgrade.Apply(gun, gunUpgLevels[upgradeType]);
            gunUpgLevels[upgradeType]++;

            gunUpgCurrCosts[upgradeType] = upgrade.CalculateCost(gunUpgBaseCosts[upgradeType], gunUpgLevels[upgradeType]);
            Debug.Log("Gun upgrade applied: " + upgradeType);
        }
        else
        {
            Debug.Log("Not enough credits for: " + upgradeType);
        }
    }
}