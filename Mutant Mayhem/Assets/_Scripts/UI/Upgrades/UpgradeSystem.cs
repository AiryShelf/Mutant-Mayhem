using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class UpgradeSystem : MonoBehaviour
{
    [SerializeField] ParticleSystem playerUpgAppliedFX;
    [SerializeField] ParticleSystem uiUpgAppliedFX;
    [SerializeField] List<PlayerStatsUpgrade> playerStatsEnums;
    [SerializeField] List<QCubeStatsUpgrade> qCubeStatsEnums;
    [SerializeField] List<ConsumablesUpgrade> consumablesEnums;
    [SerializeField] List<GunStatsUpgrade> laserPistolEnums;
    [SerializeField] List<GunStatsUpgrade> SMGEnums;
    public Player player;
    public QCubeController qCubeController;
    public PlayerShooter playerShooter;
    public TileManager tileManager;

    MessagePanel messagePanel;

    #region Upgrade Dicts

    // PlayerStats
    public Dictionary<PlayerStatsUpgrade, int> playerStatsUpgMaxLevels = 
        new Dictionary<PlayerStatsUpgrade, int>();
    public Dictionary<PlayerStatsUpgrade, int> playerStatsUpgLevels = 
        new Dictionary<PlayerStatsUpgrade, int>();
    private Dictionary<PlayerStatsUpgrade, int> playerStatsUpgBaseCosts = 
        new Dictionary<PlayerStatsUpgrade, int>();
    public Dictionary<PlayerStatsUpgrade, int> playerStatsUpgCurrCosts = 
        new Dictionary<PlayerStatsUpgrade, int>();

    // QCubeStats
    public Dictionary<QCubeStatsUpgrade, int> qCubeStatsUpgMaxLevels = 
        new Dictionary<QCubeStatsUpgrade, int>();
    public Dictionary<QCubeStatsUpgrade, int> qCubeStatsUpgLevels = 
        new Dictionary<QCubeStatsUpgrade, int>();
    private Dictionary<QCubeStatsUpgrade, int> qCubeStatsUpgBaseCosts = 
        new Dictionary<QCubeStatsUpgrade, int>();
    public Dictionary<QCubeStatsUpgrade, int> qCubeStatsUpgCurrCosts = 
        new Dictionary<QCubeStatsUpgrade, int>();
    
    // Consumables
    public Dictionary<ConsumablesUpgrade, int> consumablesUpgMaxLevels = 
        new Dictionary<ConsumablesUpgrade, int>();
    public Dictionary<ConsumablesUpgrade, int> consumablesUpgLevels = 
        new Dictionary<ConsumablesUpgrade, int>();
    private Dictionary<ConsumablesUpgrade, int> consumablesUpgBaseCosts = 
        new Dictionary<ConsumablesUpgrade, int>();
    public Dictionary<ConsumablesUpgrade, int> consumablesUpgCurrCosts = 
        new Dictionary<ConsumablesUpgrade, int>();

    // GUN STATS
    // Laser Pistol stats
    public Dictionary<GunStatsUpgrade, int> laserPistolUpgMaxLevels = 
        new Dictionary<GunStatsUpgrade, int>();
    public Dictionary<GunStatsUpgrade, int> laserPistolUpgLevels = 
        new Dictionary<GunStatsUpgrade, int>();
    private Dictionary<GunStatsUpgrade, int> laserPistolUpgBaseCosts = 
        new Dictionary<GunStatsUpgrade, int>();
    public Dictionary<GunStatsUpgrade, int> laserPistolUpgCurrCosts = 
        new Dictionary<GunStatsUpgrade, int>();

    // SMG stats
    public Dictionary<GunStatsUpgrade, int> SMGUpgMaxLevels = 
        new Dictionary<GunStatsUpgrade, int>();
    public Dictionary<GunStatsUpgrade, int> SMGUpgLevels = 
        new Dictionary<GunStatsUpgrade, int>();
    private Dictionary<GunStatsUpgrade, int> SMGUpgBaseCosts = 
        new Dictionary<GunStatsUpgrade, int>();
    public Dictionary<GunStatsUpgrade, int> SMGUpgCurrCosts = 
        new Dictionary<GunStatsUpgrade, int>();
    
    #endregion

    void Awake()
    {
        messagePanel = FindObjectOfType<MessagePanel>();

        #region Upgrade Init

        // Max upgrade levels
        // PlayerStats
        playerStatsUpgMaxLevels[PlayerStatsUpgrade.MoveSpeed] = 100;
        playerStatsUpgMaxLevels[PlayerStatsUpgrade.StrafeSpeed] = 100;
        playerStatsUpgMaxLevels[PlayerStatsUpgrade.SprintFactor] = 100;
        playerStatsUpgMaxLevels[PlayerStatsUpgrade.PlayerReloadSpeed] = 100;
        playerStatsUpgMaxLevels[PlayerStatsUpgrade.PlayerAccuracy] = 10;
        playerStatsUpgMaxLevels[PlayerStatsUpgrade.MeleeDamage] = 100;
        playerStatsUpgMaxLevels[PlayerStatsUpgrade.MeleeKnockback] = 100;
        playerStatsUpgMaxLevels[PlayerStatsUpgrade.MeleeAttackRate] = 100;
        playerStatsUpgMaxLevels[PlayerStatsUpgrade.StaminaMax] = 100;
        playerStatsUpgMaxLevels[PlayerStatsUpgrade.StaminaRegen] = 100;
        playerStatsUpgMaxLevels[PlayerStatsUpgrade.HealthMax] = 100;
        playerStatsUpgMaxLevels[PlayerStatsUpgrade.HealthRegen] = 100;

        // QCubeStats
        qCubeStatsUpgMaxLevels[QCubeStatsUpgrade.QCubeMaxHealth] = int.MaxValue;

        //Consumables
        consumablesUpgMaxLevels[ConsumablesUpgrade.PlayerHeal] = int.MaxValue;
        consumablesUpgMaxLevels[ConsumablesUpgrade.QCubeRepair] = int.MaxValue;
        consumablesUpgMaxLevels[ConsumablesUpgrade.GrenadeBuyAmmo] = int.MaxValue;
        consumablesUpgMaxLevels[ConsumablesUpgrade.SMGBuyAmmo] = int.MaxValue;

        // Gun Stats
        laserPistolUpgMaxLevels[GunStatsUpgrade.GunDamage] = 100;
        laserPistolUpgMaxLevels[GunStatsUpgrade.GunKnockback] = 100;
        laserPistolUpgMaxLevels[GunStatsUpgrade.ShootSpeed] = 10;
        laserPistolUpgMaxLevels[GunStatsUpgrade.ClipSize] = 10;
        laserPistolUpgMaxLevels[GunStatsUpgrade.ChargeDelay] = 10;
        laserPistolUpgMaxLevels[GunStatsUpgrade.GunAccuracy] = 10;
        laserPistolUpgMaxLevels[GunStatsUpgrade.GunRange] = 10;
        laserPistolUpgMaxLevels[GunStatsUpgrade.Recoil] = 10;

        SMGUpgMaxLevels[GunStatsUpgrade.GunDamage] = 100;
        SMGUpgMaxLevels[GunStatsUpgrade.GunKnockback] = 100;
        SMGUpgMaxLevels[GunStatsUpgrade.ShootSpeed] = 10;
        SMGUpgMaxLevels[GunStatsUpgrade.ClipSize] = 10;
        SMGUpgMaxLevels[GunStatsUpgrade.ChargeDelay] = 10;
        SMGUpgMaxLevels[GunStatsUpgrade.GunAccuracy] = 10;
        SMGUpgMaxLevels[GunStatsUpgrade.GunRange] = 10;
        SMGUpgMaxLevels[GunStatsUpgrade.Recoil] = 10;

        // Base upgrade costs
        // PlayerStats
        playerStatsUpgBaseCosts[PlayerStatsUpgrade.MoveSpeed] = 100;
        playerStatsUpgBaseCosts[PlayerStatsUpgrade.StrafeSpeed] = 100;
        playerStatsUpgBaseCosts[PlayerStatsUpgrade.SprintFactor] = 150;
        playerStatsUpgBaseCosts[PlayerStatsUpgrade.PlayerReloadSpeed] = 150;
        playerStatsUpgBaseCosts[PlayerStatsUpgrade.PlayerAccuracy] = 200;
        playerStatsUpgBaseCosts[PlayerStatsUpgrade.MeleeDamage] = 100;
        playerStatsUpgBaseCosts[PlayerStatsUpgrade.MeleeKnockback] = 150;
        playerStatsUpgBaseCosts[PlayerStatsUpgrade.MeleeAttackRate] = 100;
        playerStatsUpgBaseCosts[PlayerStatsUpgrade.StaminaMax] = 100;
        playerStatsUpgBaseCosts[PlayerStatsUpgrade.StaminaRegen] = 200;
        playerStatsUpgBaseCosts[PlayerStatsUpgrade.HealthMax] = 100;
        playerStatsUpgBaseCosts[PlayerStatsUpgrade.HealthRegen] = 200;

        // QCubeStats
        qCubeStatsUpgBaseCosts[QCubeStatsUpgrade.QCubeMaxHealth] = 200;

        // Consumables
        consumablesUpgBaseCosts[ConsumablesUpgrade.PlayerHeal] = 100;
        consumablesUpgBaseCosts[ConsumablesUpgrade.QCubeRepair] = 200;
        consumablesUpgBaseCosts[ConsumablesUpgrade.GrenadeBuyAmmo] = 25;
        consumablesUpgBaseCosts[ConsumablesUpgrade.SMGBuyAmmo] = 50;

        // Gun Stats
        laserPistolUpgBaseCosts[GunStatsUpgrade.GunDamage] = 100;
        laserPistolUpgBaseCosts[GunStatsUpgrade.GunKnockback] = 150;
        laserPistolUpgBaseCosts[GunStatsUpgrade.ShootSpeed] = 200;
        laserPistolUpgBaseCosts[GunStatsUpgrade.ClipSize] = 200;
        laserPistolUpgBaseCosts[GunStatsUpgrade.ChargeDelay] = 200;
        laserPistolUpgBaseCosts[GunStatsUpgrade.GunAccuracy] = 200;
        laserPistolUpgBaseCosts[GunStatsUpgrade.GunRange] = 150;
        laserPistolUpgBaseCosts[GunStatsUpgrade.Recoil] = 100; 

        SMGUpgBaseCosts[GunStatsUpgrade.GunDamage] = 100;
        SMGUpgBaseCosts[GunStatsUpgrade.GunKnockback] = 150;
        SMGUpgBaseCosts[GunStatsUpgrade.ShootSpeed] = 200;
        SMGUpgBaseCosts[GunStatsUpgrade.ClipSize] = 200;
        SMGUpgBaseCosts[GunStatsUpgrade.ChargeDelay] = 200;
        SMGUpgBaseCosts[GunStatsUpgrade.GunAccuracy] = 200;
        SMGUpgBaseCosts[GunStatsUpgrade.GunRange] = 150;
        SMGUpgBaseCosts[GunStatsUpgrade.Recoil] = 100; 

        // Initialize currentCosts
        foreach (KeyValuePair<PlayerStatsUpgrade, int> kvp in playerStatsUpgBaseCosts)
        {
            playerStatsUpgCurrCosts.Add(kvp.Key, kvp.Value);
        }
        foreach (KeyValuePair<QCubeStatsUpgrade, int> kvp in qCubeStatsUpgBaseCosts)
        {
            qCubeStatsUpgCurrCosts.Add(kvp.Key, kvp.Value);
        }
        foreach (KeyValuePair<ConsumablesUpgrade, int> kvp in consumablesUpgBaseCosts)
        {
            consumablesUpgCurrCosts.Add(kvp.Key, kvp.Value);
        }
        foreach (KeyValuePair<GunStatsUpgrade, int> kvp in laserPistolUpgBaseCosts)
        {
            laserPistolUpgCurrCosts.Add(kvp.Key, kvp.Value);
        }
        foreach (KeyValuePair<GunStatsUpgrade, int> kvp in SMGUpgBaseCosts)
        {
            SMGUpgCurrCosts.Add(kvp.Key, kvp.Value);
        }

        // Initialize upgrade levels
        foreach(PlayerStatsUpgrade type in playerStatsEnums)
        {
            playerStatsUpgLevels[type] = 0;
        }
        foreach(QCubeStatsUpgrade type in qCubeStatsEnums)
        {
            qCubeStatsUpgLevels[type] = 0;
        }
        foreach(ConsumablesUpgrade type in consumablesEnums)
        {
            consumablesUpgLevels[type] = 0;
        }
        foreach(GunStatsUpgrade type in laserPistolEnums)
        {
            laserPistolUpgLevels[type] = 0;
        }
        foreach(GunStatsUpgrade type in SMGEnums)
        {
            SMGUpgLevels[type] = 0;
        }
    }

    #endregion Upgrade Properties

    Upgrade CreateUpgrade(PlayerStatsUpgrade type)
    {
        switch (type)
        {
            // PlayerStats
            case PlayerStatsUpgrade.MoveSpeed:
                return new MoveSpeedUpgrade();
            case PlayerStatsUpgrade.StrafeSpeed:
                return new StrafeSpeedUpgrade();
            case PlayerStatsUpgrade.SprintFactor:
                return new SprintFactorUpgrade();
            case PlayerStatsUpgrade.PlayerReloadSpeed:
                return new ReloadSpeedUpgrade();
            case PlayerStatsUpgrade.MeleeDamage:
                return new MeleeDamageUpgrade();
            case PlayerStatsUpgrade.MeleeKnockback:
                return new KnockbackUpgrade();
            case PlayerStatsUpgrade.MeleeAttackRate:
                return new MeleeAttackRateUpgrade();
            case PlayerStatsUpgrade.StaminaMax:
                return new StaminaMaxUpgrade();
            case PlayerStatsUpgrade.StaminaRegen:
                return new StaminaRegenUpgrade();
            case PlayerStatsUpgrade.HealthMax:
                return new HealthMaxUpgrade();
            case PlayerStatsUpgrade.HealthRegen:
                return new HealthRegenUpgrade();
            case PlayerStatsUpgrade.PlayerAccuracy:
                return new PlayerAccuracyUpgrade();

            default:
                return null;
        }
    }

    Upgrade CreateUpgrade(QCubeStatsUpgrade type)
    {
        switch (type)
        {
            // QCubeStats
            case QCubeStatsUpgrade.QCubeMaxHealth:
                return new QCubeMaxHealthUpgrade();

            default:
                return null;
                
        }
    }

    Upgrade CreateUpgrade(ConsumablesUpgrade type)
    {
        switch (type)
        {
            // Consumables
            case ConsumablesUpgrade.PlayerHeal:
                return new PlayerHealUpgrade();
            case ConsumablesUpgrade.QCubeRepair:
                return new QCubeRepairUpgrade();
            case ConsumablesUpgrade.GrenadeBuyAmmo:
                return new GrenadeBuyAmmoUpgrade();
            case ConsumablesUpgrade.SMGBuyAmmo:
                return new SMGBuyAmmoUpgrade();

            default:
                return null;
        }
    }

    Upgrade CreateUpgrade(GunStatsUpgrade type)
    {
        switch (type)
        {
            // GunSO stats
            case GunStatsUpgrade.GunDamage:
                return new GunDamageUpgrade();
            case GunStatsUpgrade.GunKnockback:
                return new GunKnockbackUpgrade();
            case GunStatsUpgrade.ShootSpeed:
                return new ShootSpeedUpgrade();
            case GunStatsUpgrade.ClipSize:
                return new ClipSizeUpgrade();
            case GunStatsUpgrade.ChargeDelay:
                return new ChargeDelayUpgrade();
            case GunStatsUpgrade.GunAccuracy:
                return new GunAccuracyUpgrade();
            case GunStatsUpgrade.GunRange:
                return new RangeUpgrade();
            case GunStatsUpgrade.Recoil:
                return new RecoilUpgrade();

            default:
                return null;
        }
    }

    #region Apply Upgrades

    public void OnUpgradeButtonClicked(PlayerStatsUpgrade upgType)
    {
        Upgrade upgrade = CreateUpgrade(upgType);
        ApplyPlayerUpgrade(upgrade, upgType);
    }

    public void OnUpgradeButtonClicked(QCubeStatsUpgrade upgType)
    {
        Upgrade upgrade = CreateUpgrade(upgType);
        ApplyQCubeUpgrade(upgrade, upgType);
    }

    public void OnUpgradeButtonClicked(ConsumablesUpgrade upgType)
    {
        Upgrade upgrade = CreateUpgrade(upgType);
        ApplyConsumableUpgrade(upgrade, upgType);
    }

    public void OnUpgradeButtonClicked(GunStatsUpgrade upgType, int gunIndex)
    {
        Upgrade upgrade = CreateUpgrade(upgType);
        ApplyGunUpgrade(upgrade, upgType, gunIndex);
    }

    private void ApplyPlayerUpgrade(Upgrade upgrade, PlayerStatsUpgrade upgType)
    {
        // Check max level
        if (playerStatsUpgLevels[upgType] >= playerStatsUpgMaxLevels[upgType])
        {
            Debug.Log("Max level reached for: " + upgType);
            messagePanel.ShowMessage("Max level reached!", Color.yellow);
            return;
        }

        // Buy and apply
        int cost = playerStatsUpgCurrCosts[upgType];
        if (BuildingSystem.PlayerCredits >= cost)
        {
            BuildingSystem.PlayerCredits -= cost;
            playerStatsUpgLevels[upgType]++;

            upgrade.Apply(player.stats, playerStatsUpgLevels[upgType]);
            PlayUpgradeEffects();
            
            playerStatsUpgCurrCosts[upgType] = upgrade.CalculateCost(
                playerStatsUpgBaseCosts[upgType], playerStatsUpgLevels[upgType] + 1);

            Debug.Log("PlayerStats upgrade applied: " + upgType);
            messagePanel.ShowMessage("Exosuit stat upgraded to level " + 
                                     playerStatsUpgLevels[upgType], Color.cyan);
        }
        else
        {
            Debug.Log("Not enough credits for: " + upgType);
            messagePanel.ShowMessage("Not enough Credits!", Color.red);
        }
    }

    private void ApplyQCubeUpgrade(Upgrade upgrade, QCubeStatsUpgrade upgType)
    {
        // Check max level
        if (qCubeStatsUpgLevels[upgType] >= qCubeStatsUpgMaxLevels[upgType])
        {
            Debug.Log("Max level reached for: " + upgType);
            messagePanel.ShowMessage("Max level reached!", Color.yellow);
            return;
        }

        // Buy and apply
        int cost = qCubeStatsUpgCurrCosts[upgType];
        if (BuildingSystem.PlayerCredits >= cost)
        {
            BuildingSystem.PlayerCredits -= cost;
            qCubeStatsUpgLevels[upgType]++;

            upgrade.Apply(qCubeController.qCubeStats, qCubeStatsUpgLevels[upgType]);
            PlayUpgradeEffects();
            
            qCubeStatsUpgCurrCosts[upgType] = upgrade.CalculateCost(
                qCubeStatsUpgBaseCosts[upgType], qCubeStatsUpgLevels[upgType] + 1);

            Debug.Log("QCubeStats upgrade applied: " + upgType);
            messagePanel.ShowMessage("Quantum Cube stat upgraded to level " + 
                                     qCubeStatsUpgLevels[upgType], Color.cyan);
        }
        else
        {
            Debug.Log("Not enough credits for: " + upgType);
            messagePanel.ShowMessage("Not enough Credits!", Color.red);
        }
    }

    private void ApplyConsumableUpgrade(Upgrade upgrade, ConsumablesUpgrade upgType)
    {
        // Buy and apply
        int cost = consumablesUpgCurrCosts[upgType];
        if (BuildingSystem.PlayerCredits >= cost)
        {
            // Check max level
            if (consumablesUpgLevels[upgType] >= consumablesUpgMaxLevels[upgType])
            {
                Debug.LogError("Consumable int maxed out: " + upgType);
                messagePanel.ShowMessage("Maxed. You either bought this over 32,000 times " + 
                                         "or there is a bug!  Let me know!", Color.yellow);
                return;
            }

            
            if (upgrade.Apply(player.stats))
            {
                BuildingSystem.PlayerCredits -= cost;
                consumablesUpgLevels[upgType]++;

                
                PlayUpgradeEffects();
                
                
                consumablesUpgCurrCosts[upgType] = upgrade.CalculateCost(
                    consumablesUpgBaseCosts[upgType], consumablesUpgLevels[upgType] + 1);

                Debug.Log("Consumable applied: " + upgType);
                messagePanel.ShowMessage("Consumabled applied!", Color.cyan);
            }
            else
            {
                Debug.Log(upgType + "already full");
                messagePanel.ShowMessage("It's already full", Color.yellow);
            }
        }
        else
        {
            Debug.Log("Not enough credits for: " + upgType);
            messagePanel.ShowMessage("Not enough Credits!", Color.red);
        }
    }

    private void ApplyGunUpgrade(Upgrade upgrade, GunStatsUpgrade upgType, int gunIndex)
    {
        // Null check
        if (gunIndex < 0 || gunIndex >= playerShooter.gunList.Count)
        {
            Debug.LogError("Invalid gun index: " + gunIndex);
            return;
        }

        GunSO gun = playerShooter.gunList[gunIndex];
        Dictionary<GunStatsUpgrade, int> gunUpgLevels;
        Dictionary<GunStatsUpgrade, int> gunUpgMaxLevels;
        Dictionary<GunStatsUpgrade, int> gunUpgCurrCosts;
        Dictionary<GunStatsUpgrade, int> gunUpgBaseCosts;

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

        if (gunUpgLevels[upgType] >= gunUpgMaxLevels[upgType])
        {
            Debug.Log("Max level reached for: " + upgType);
            messagePanel.ShowMessage("Max level reached!", Color.yellow);
            return;
        }

        // Buy and apply
        int cost = gunUpgCurrCosts[upgType];
        if (BuildingSystem.PlayerCredits >= cost)
        {
            BuildingSystem.PlayerCredits -= cost;
            gunUpgLevels[upgType]++;

            upgrade.Apply(gun, gunUpgLevels[upgType]);
            PlayUpgradeEffects();
            
            gunUpgCurrCosts[upgType] = upgrade.CalculateCost(
                gunUpgBaseCosts[upgType], gunUpgLevels[upgType] + 1);

            Debug.Log("Gun upgrade applied: " + upgType);
            messagePanel.ShowMessage(gun.uiName + " stat upgraded to level " + 
                                     gunUpgLevels[upgType], Color.cyan);
        }
        else
        {
            Debug.Log("Not enough credits for: " + upgType);
            messagePanel.ShowMessage("Not enough Credits!", Color.red);
        }
    }

    #endregion

    public void PlayUpgradeEffects()
    {
        playerUpgAppliedFX.transform.position = player.transform.position + new Vector3(0, -0.5f, 0);

        playerUpgAppliedFX.Play();

        uiUpgAppliedFX.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        uiUpgAppliedFX.Play();
    }
}