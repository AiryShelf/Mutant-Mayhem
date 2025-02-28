using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }
    
    [SerializeField] List<PlayerStatsUpgrade> playerStatsUpgrades;
    [SerializeField] List<StructureStatsUpgrade> structureStatsUpgrades;
    [SerializeField] List<ConsumablesUpgrade> consumablesUpgrades;
    [SerializeField] List<GunStatsUpgrade> laserPistolUpgrades;
    [SerializeField] List<GunStatsUpgrade> SMGUpgrades;
    [SerializeField] List<GunStatsUpgrade> repairGunUpgrades;
    [HideInInspector] public Player player;
    [HideInInspector] public PlayerShooter playerShooter;

    [Header("Dynamic vars, don't set here")]
    public float playerStatsCostMult = 1;
    public float structureStatsCostMult = 1;
    public float consumablesCostMult = 1;
    public float gunStatsCostMult = 1;
    public UpgradeEffects upgradeEffects;    

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

    // StructureStats
    public Dictionary<StructureStatsUpgrade, int> structureStatsUpgMaxLevels = 
        new Dictionary<StructureStatsUpgrade, int>();
    public Dictionary<StructureStatsUpgrade, int> structureStatsUpgLevels = 
        new Dictionary<StructureStatsUpgrade, int>();
    private Dictionary<StructureStatsUpgrade, int> structureStatsUpgBaseCosts = 
        new Dictionary<StructureStatsUpgrade, int>();
    public Dictionary<StructureStatsUpgrade, int> structureStatsUpgCurrCosts = 
        new Dictionary<StructureStatsUpgrade, int>();
    
    // Consumables
    public Dictionary<ConsumablesUpgrade, int> consumablesUpgMaxLevels = 
        new Dictionary<ConsumablesUpgrade, int>();
    public Dictionary<ConsumablesUpgrade, int> consumablesUpgLevels = 
        new Dictionary<ConsumablesUpgrade, int>();
    public Dictionary<ConsumablesUpgrade, int> consumablesUpgBaseCosts = 
        new Dictionary<ConsumablesUpgrade, int>();
    public Dictionary<ConsumablesUpgrade, int> consumablesUpgCurrCosts = 
        new Dictionary<ConsumablesUpgrade, int>();

    // GUN STATS
    // Laser Pistol stats
    public Dictionary<GunStatsUpgrade, int> laserUpgMaxLevels = 
        new Dictionary<GunStatsUpgrade, int>();
    public Dictionary<GunStatsUpgrade, int> laserUpgLevels = 
        new Dictionary<GunStatsUpgrade, int>();
    private Dictionary<GunStatsUpgrade, int> laserUpgBaseCosts = 
        new Dictionary<GunStatsUpgrade, int>();
    public Dictionary<GunStatsUpgrade, int> laserUpgCurrCosts = 
        new Dictionary<GunStatsUpgrade, int>();

    // SMG stats
    public Dictionary<GunStatsUpgrade, int> bulletUpgMaxLevels = 
        new Dictionary<GunStatsUpgrade, int>();
    public Dictionary<GunStatsUpgrade, int> bulletUpgLevels = 
        new Dictionary<GunStatsUpgrade, int>();
    private Dictionary<GunStatsUpgrade, int> bulletUpgBaseCosts = 
        new Dictionary<GunStatsUpgrade, int>();
    public Dictionary<GunStatsUpgrade, int> bulletUpgCurrCosts = 
        new Dictionary<GunStatsUpgrade, int>();

    // Repair Gun stats
    public Dictionary<GunStatsUpgrade, int> repairGunUpgMaxLevels = 
        new Dictionary<GunStatsUpgrade, int>();
    public Dictionary<GunStatsUpgrade, int> repairGunUpgLevels = 
        new Dictionary<GunStatsUpgrade, int>();
    private Dictionary<GunStatsUpgrade, int> repairGunUpgBaseCosts = 
        new Dictionary<GunStatsUpgrade, int>();
    public Dictionary<GunStatsUpgrade, int> repairGunUpgCurrCosts = 
        new Dictionary<GunStatsUpgrade, int>();


    #endregion

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    #region Upg Initialization

    public void Initialize()
    {
        playerStatsCostMult = 1;
        structureStatsCostMult = 1;
        consumablesCostMult = 1;
        gunStatsCostMult = 1;

        // Reset references
        player = FindObjectOfType<Player>();
        playerShooter = player.playerShooter;

        // Reset particle system references
        upgradeEffects.Initialize();

        // Clear dicts
        playerStatsUpgMaxLevels.Clear();
        playerStatsUpgLevels.Clear();
        playerStatsUpgBaseCosts.Clear();
        playerStatsUpgCurrCosts.Clear();
        structureStatsUpgMaxLevels.Clear();
        structureStatsUpgLevels.Clear();
        structureStatsUpgBaseCosts.Clear();
        structureStatsUpgCurrCosts.Clear();
        consumablesUpgMaxLevels.Clear();
        consumablesUpgLevels.Clear();
        consumablesUpgBaseCosts.Clear();
        consumablesUpgCurrCosts.Clear();
        laserUpgMaxLevels.Clear();
        laserUpgLevels.Clear();
        laserUpgBaseCosts.Clear();
        laserUpgCurrCosts.Clear();
        bulletUpgMaxLevels.Clear();
        bulletUpgLevels.Clear();
        bulletUpgBaseCosts.Clear();
        bulletUpgCurrCosts.Clear();
        repairGunUpgMaxLevels.Clear();
        repairGunUpgLevels.Clear();
        repairGunUpgBaseCosts.Clear();
        repairGunUpgCurrCosts.Clear();

        // Initialize

        #region Upg Levels
        // Set max levels
        // PlayerStats
        playerStatsUpgMaxLevels[PlayerStatsUpgrade.MoveSpeed] = 50;
        playerStatsUpgMaxLevels[PlayerStatsUpgrade.StrafeSpeed] = 50;
        playerStatsUpgMaxLevels[PlayerStatsUpgrade.SprintFactor] = 20;
        playerStatsUpgMaxLevels[PlayerStatsUpgrade.PlayerReloadSpeed] = 10; // Deprecated
        playerStatsUpgMaxLevels[PlayerStatsUpgrade.WeaponHandling] = 20;
        playerStatsUpgMaxLevels[PlayerStatsUpgrade.MeleeDamage] = 150;
        playerStatsUpgMaxLevels[PlayerStatsUpgrade.MeleeKnockback] = 150;
        playerStatsUpgMaxLevels[PlayerStatsUpgrade.StaminaMax] = 100;
        playerStatsUpgMaxLevels[PlayerStatsUpgrade.StaminaRegen] = 100;
        playerStatsUpgMaxLevels[PlayerStatsUpgrade.HealthMax] = 100;
        playerStatsUpgMaxLevels[PlayerStatsUpgrade.HealthRegen] = 100;
        playerStatsUpgMaxLevels[PlayerStatsUpgrade.CriticalHitChance] = 100;
        playerStatsUpgMaxLevels[PlayerStatsUpgrade.CriticalHitDamage] = 100;

        // Structures
        structureStatsUpgMaxLevels[StructureStatsUpgrade.QCubeMaxHealth] = int.MaxValue;
        structureStatsUpgMaxLevels[StructureStatsUpgrade.StructureMaxHealth] = int.MaxValue;
        structureStatsUpgMaxLevels[StructureStatsUpgrade.MaxTurrets] = 20;
        structureStatsUpgMaxLevels[StructureStatsUpgrade.TurretRotSpeed] = 20;
        structureStatsUpgMaxLevels[StructureStatsUpgrade.TurretSensors] = 20;

        //Consumables
        consumablesUpgMaxLevels[ConsumablesUpgrade.PlayerHeal] = int.MaxValue;
        consumablesUpgMaxLevels[ConsumablesUpgrade.QCubeRepair] = int.MaxValue;
        consumablesUpgMaxLevels[ConsumablesUpgrade.GrenadeBuyAmmo] = int.MaxValue;
        consumablesUpgMaxLevels[ConsumablesUpgrade.SMGBuyAmmo] = int.MaxValue;
        consumablesUpgMaxLevels[ConsumablesUpgrade.BuyConstructionDrone] = int.MaxValue;
        consumablesUpgMaxLevels[ConsumablesUpgrade.BuyAttackDrone] = int.MaxValue;

        // GunStats
        laserUpgMaxLevels[GunStatsUpgrade.GunDamage] = 100;
        laserUpgMaxLevels[GunStatsUpgrade.GunKnockback] = 100;
        laserUpgMaxLevels[GunStatsUpgrade.ShootSpeed] = 10;
        laserUpgMaxLevels[GunStatsUpgrade.ClipSize] = 10;
        laserUpgMaxLevels[GunStatsUpgrade.ChargeSpeed] = 10;
        laserUpgMaxLevels[GunStatsUpgrade.GunAccuracy] = 10;
        laserUpgMaxLevels[GunStatsUpgrade.GunRange] = 10;
        laserUpgMaxLevels[GunStatsUpgrade.Recoil] = 10; // Deprecated

        bulletUpgMaxLevels[GunStatsUpgrade.GunDamage] = 100;
        bulletUpgMaxLevels[GunStatsUpgrade.GunKnockback] = 100;
        bulletUpgMaxLevels[GunStatsUpgrade.ShootSpeed] = 10;
        bulletUpgMaxLevels[GunStatsUpgrade.ClipSize] = 10;
        bulletUpgMaxLevels[GunStatsUpgrade.ChargeSpeed] = 10;
        bulletUpgMaxLevels[GunStatsUpgrade.GunAccuracy] = 10;
        bulletUpgMaxLevels[GunStatsUpgrade.GunRange] = 10;
        bulletUpgMaxLevels[GunStatsUpgrade.Recoil] = 10; // Deprecated
        bulletUpgMaxLevels[GunStatsUpgrade.TurretReloadSpeed] = 10;

        repairGunUpgMaxLevels[GunStatsUpgrade.GunDamage] = 100;
        repairGunUpgMaxLevels[GunStatsUpgrade.GunKnockback] = 100;
        repairGunUpgMaxLevels[GunStatsUpgrade.ShootSpeed] = 10;
        repairGunUpgMaxLevels[GunStatsUpgrade.ClipSize] = 10;
        repairGunUpgMaxLevels[GunStatsUpgrade.ChargeSpeed] = 10;
        repairGunUpgMaxLevels[GunStatsUpgrade.GunAccuracy] = 10;
        repairGunUpgMaxLevels[GunStatsUpgrade.GunRange] = 10;
        repairGunUpgMaxLevels[GunStatsUpgrade.Recoil] = 10; // Deprecated

        // Initialize upgrade levels
        foreach(PlayerStatsUpgrade type in playerStatsUpgrades)
        {
            playerStatsUpgLevels[type] = 0;
        }
        foreach(StructureStatsUpgrade type in structureStatsUpgrades)
        {
            structureStatsUpgLevels[type] = 0;
        }
        foreach(ConsumablesUpgrade type in consumablesUpgrades)
        {
            consumablesUpgLevels[type] = 0;
        }
        foreach(GunStatsUpgrade type in laserPistolUpgrades)
        {
            laserUpgLevels[type] = 0;
        }
        foreach(GunStatsUpgrade type in SMGUpgrades)
        {
            bulletUpgLevels[type] = 0;
        }
        foreach(GunStatsUpgrade type in repairGunUpgrades)
        {
            repairGunUpgLevels[type] = 0;
        }

        #endregion Upg Levels

        #region Upg Costs
        // PlayerStats
        playerStatsUpgBaseCosts[PlayerStatsUpgrade.MoveSpeed] = 100;
        playerStatsUpgBaseCosts[PlayerStatsUpgrade.StrafeSpeed] = 100;
        playerStatsUpgBaseCosts[PlayerStatsUpgrade.SprintFactor] = 200;
        playerStatsUpgBaseCosts[PlayerStatsUpgrade.PlayerReloadSpeed] = 200; // Deprecated
        playerStatsUpgBaseCosts[PlayerStatsUpgrade.WeaponHandling] = 200;
        playerStatsUpgBaseCosts[PlayerStatsUpgrade.MeleeDamage] = 250;
        playerStatsUpgBaseCosts[PlayerStatsUpgrade.MeleeKnockback] = 150;
        playerStatsUpgBaseCosts[PlayerStatsUpgrade.StaminaMax] = 100;
        playerStatsUpgBaseCosts[PlayerStatsUpgrade.StaminaRegen] = 200;
        playerStatsUpgBaseCosts[PlayerStatsUpgrade.HealthMax] = 100;
        playerStatsUpgBaseCosts[PlayerStatsUpgrade.HealthRegen] = 200;
        playerStatsUpgBaseCosts[PlayerStatsUpgrade.CriticalHitChance] = 200;
        playerStatsUpgBaseCosts[PlayerStatsUpgrade.CriticalHitDamage] = 200;

        // StructureStats
        structureStatsUpgBaseCosts[StructureStatsUpgrade.QCubeMaxHealth] = 500;
        structureStatsUpgBaseCosts[StructureStatsUpgrade.StructureMaxHealth] = 500;
        structureStatsUpgBaseCosts[StructureStatsUpgrade.MaxTurrets] = 6000;
        structureStatsUpgBaseCosts[StructureStatsUpgrade.TurretRotSpeed] = 500;
        structureStatsUpgBaseCosts[StructureStatsUpgrade.TurretSensors] = 500;

        // Consumables
        consumablesUpgBaseCosts[ConsumablesUpgrade.PlayerHeal] = 100;
        consumablesUpgBaseCosts[ConsumablesUpgrade.QCubeRepair] = 200;
        consumablesUpgBaseCosts[ConsumablesUpgrade.GrenadeBuyAmmo] = 200;
        consumablesUpgBaseCosts[ConsumablesUpgrade.SMGBuyAmmo] = 100;
        consumablesUpgBaseCosts[ConsumablesUpgrade.BuyConstructionDrone] = 500;
        consumablesUpgBaseCosts[ConsumablesUpgrade.BuyAttackDrone] = 1500;

        // Gun Stats
        laserUpgBaseCosts[GunStatsUpgrade.GunDamage] = 250;
        laserUpgBaseCosts[GunStatsUpgrade.GunKnockback] = 150;
        laserUpgBaseCosts[GunStatsUpgrade.ShootSpeed] = 250;
        laserUpgBaseCosts[GunStatsUpgrade.ClipSize] = 200;
        laserUpgBaseCosts[GunStatsUpgrade.ChargeSpeed] = 200;
        laserUpgBaseCosts[GunStatsUpgrade.GunAccuracy] = 200;
        laserUpgBaseCosts[GunStatsUpgrade.GunRange] = 150;
        laserUpgBaseCosts[GunStatsUpgrade.Recoil] = 100; // Deprecated

        bulletUpgBaseCosts[GunStatsUpgrade.GunDamage] = 250;
        bulletUpgBaseCosts[GunStatsUpgrade.GunKnockback] = 150;
        bulletUpgBaseCosts[GunStatsUpgrade.ShootSpeed] = 250;
        bulletUpgBaseCosts[GunStatsUpgrade.ClipSize] = 200;
        bulletUpgBaseCosts[GunStatsUpgrade.ChargeSpeed] = 200;
        bulletUpgBaseCosts[GunStatsUpgrade.GunAccuracy] = 200;
        bulletUpgBaseCosts[GunStatsUpgrade.GunRange] = 150;
        bulletUpgBaseCosts[GunStatsUpgrade.Recoil] = 100; // Deprecated
        bulletUpgBaseCosts[GunStatsUpgrade.TurretReloadSpeed] = 500;

        repairGunUpgBaseCosts[GunStatsUpgrade.GunDamage] = 500;
        repairGunUpgBaseCosts[GunStatsUpgrade.GunKnockback] = 300;
        repairGunUpgBaseCosts[GunStatsUpgrade.ShootSpeed] = 500;
        repairGunUpgBaseCosts[GunStatsUpgrade.ClipSize] = 400;
        repairGunUpgBaseCosts[GunStatsUpgrade.ChargeSpeed] = 400;
        repairGunUpgBaseCosts[GunStatsUpgrade.GunAccuracy] = 400;
        repairGunUpgBaseCosts[GunStatsUpgrade.GunRange] = 300;
        repairGunUpgBaseCosts[GunStatsUpgrade.Recoil] = 100; // Deprecated
        repairGunUpgBaseCosts[GunStatsUpgrade.TurretReloadSpeed] = 500;

        // Initialize currentCosts
        foreach (KeyValuePair<PlayerStatsUpgrade, int> kvp in playerStatsUpgBaseCosts)
        {
            playerStatsUpgCurrCosts.Add(kvp.Key, kvp.Value);
        }
        foreach (KeyValuePair<StructureStatsUpgrade, int> kvp in structureStatsUpgBaseCosts)
        {
            structureStatsUpgCurrCosts.Add(kvp.Key, kvp.Value);
        }
        foreach (KeyValuePair<ConsumablesUpgrade, int> kvp in consumablesUpgBaseCosts)
        {
            consumablesUpgCurrCosts.Add(kvp.Key, kvp.Value);
        }
        foreach (KeyValuePair<GunStatsUpgrade, int> kvp in laserUpgBaseCosts)
        {
            laserUpgCurrCosts.Add(kvp.Key, kvp.Value);
        }
        foreach (KeyValuePair<GunStatsUpgrade, int> kvp in bulletUpgBaseCosts)
        {
            bulletUpgCurrCosts.Add(kvp.Key, kvp.Value);
        } 
        foreach (KeyValuePair<GunStatsUpgrade, int> kvp in repairGunUpgBaseCosts)
        {
            repairGunUpgCurrCosts.Add(kvp.Key, kvp.Value);
        } 
    }

        #endregion Upg Costs

        #region Create Upg

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
                return new TurretReloadSpeedUpgrade();
            case PlayerStatsUpgrade.MeleeDamage:
                return new MeleeDamageUpgrade();
            case PlayerStatsUpgrade.MeleeKnockback:
                return new KnockbackUpgrade();
            case PlayerStatsUpgrade.StaminaMax:
                return new StaminaMaxUpgrade();
            case PlayerStatsUpgrade.StaminaRegen:
                return new StaminaRegenUpgrade();
            case PlayerStatsUpgrade.HealthMax:
                return new HealthMaxUpgrade();
            case PlayerStatsUpgrade.HealthRegen:
                return new HealthRegenUpgrade();
            case PlayerStatsUpgrade.WeaponHandling:
                return new WeaponHandlingUpgrade();
            case PlayerStatsUpgrade.CriticalHitChance:
                return new CriticalHitChanceUpgrade();
            case PlayerStatsUpgrade.CriticalHitDamage:
                return new CriticalHitDamageUpgrade();

            default:
                return null;
        }
    }

    Upgrade CreateUpgrade(StructureStatsUpgrade type)
    {
        switch (type)
        {
            // StructureStats
            case StructureStatsUpgrade.QCubeMaxHealth:
                return new QCubeMaxHealthUpgrade();
            case StructureStatsUpgrade.StructureMaxHealth:
                return new StructureMaxHealthUpgrade();
            case StructureStatsUpgrade.MaxTurrets:
                return new MaxTurretsUpgrade();
            case StructureStatsUpgrade.TurretRotSpeed:
                return new TurretRotSpeedUpgrade();
            case StructureStatsUpgrade.TurretSensors:
                return new TurretSensorsUpgrade();

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
            case ConsumablesUpgrade.BuyConstructionDrone:
                return new BuyConstructionDroneUpgrade();
            case ConsumablesUpgrade.BuyAttackDrone:
                return new BuyAttackDroneUpgrade();

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
            case GunStatsUpgrade.ChargeSpeed:
                return new ChargeDelayUpgrade();
            case GunStatsUpgrade.GunAccuracy:
                return new GunAccuracyUpgrade();
            case GunStatsUpgrade.GunRange:
                return new RangeUpgrade();
            case GunStatsUpgrade.Recoil:
                return new RecoilUpgrade();
            case GunStatsUpgrade.TurretReloadSpeed:
                return new TurretReloadSpeedUpgrade();

            default:
                return null;
        }
    }
        #endregion Create Upg
    
    #endregion Initialization

    #region Apply Upg

    public void OnUpgradeButtonClicked(PlayerStatsUpgrade upgType)
    {
        Upgrade upgrade = CreateUpgrade(upgType);
        ApplyPlayerUpgrade(upgrade, upgType);
    }

    public void OnUpgradeButtonClicked(StructureStatsUpgrade upgType)
    {
        Upgrade upgrade = CreateUpgrade(upgType);
        ApplyStructureUpgrade(upgrade, upgType);
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

    // PlayerStats
    private void ApplyPlayerUpgrade(Upgrade upgrade, PlayerStatsUpgrade upgType)
    {
        // Check max level
        if (playerStatsUpgLevels[upgType] >= playerStatsUpgMaxLevels[upgType])
        {
            Debug.Log("Max level reached for: " + upgType);
            MessagePanel.PulseMessage("Max level reached!", Color.yellow);
            return;
        }

        // Buy and apply
        int cost = Mathf.FloorToInt(playerStatsCostMult * playerStatsUpgCurrCosts[upgType]);
        if (BuildingSystem.PlayerCredits >= cost)
        {
            BuildingSystem.PlayerCredits -= cost;
            playerStatsUpgLevels[upgType]++;

            upgrade.Apply(player.stats, playerStatsUpgLevels[upgType]);
            upgradeEffects.PlayUpgradeButtonEffect();
            
            playerStatsUpgCurrCosts[upgType] = upgrade.CalculateCost(player, 
                playerStatsUpgBaseCosts[upgType], playerStatsUpgLevels[upgType] + 1);

            Debug.Log("PlayerStats upgrade applied: " + upgType);
            MessagePanel.PulseMessage("Exosuit stat upgraded to level " + 
                                     playerStatsUpgLevels[upgType], Color.cyan);
        }
        else
        {
            Debug.Log("Not enough credits for: " + upgType);
            MessagePanel.PulseMessage("Not enough Credits!", Color.red);
        }
    }

    // StructureStats
    private void ApplyStructureUpgrade(Upgrade upgrade, StructureStatsUpgrade upgType)
    {
        // Check max level
        if (structureStatsUpgLevels[upgType] >= structureStatsUpgMaxLevels[upgType])
        {
            Debug.Log("Max level reached for: " + upgType);
            MessagePanel.PulseMessage("Max level reached!", Color.yellow);
            return;
        }

        // Buy and apply
        int cost = Mathf.FloorToInt(structureStatsCostMult * structureStatsUpgCurrCosts[upgType]);
        if (BuildingSystem.PlayerCredits >= cost)
        {
            BuildingSystem.PlayerCredits -= cost;
            structureStatsUpgLevels[upgType]++;

            upgrade.Apply(player.stats.structureStats, structureStatsUpgLevels[upgType]);
            upgradeEffects.PlayUpgradeButtonEffect();

            structureStatsUpgCurrCosts[upgType] = upgrade.CalculateCost(player, 
                structureStatsUpgBaseCosts[upgType], structureStatsUpgLevels[upgType] + 1);

            Debug.Log("Structure upgrade applied: " + upgType);
            MessagePanel.PulseMessage("Structure stat upgraded to level " + 
                                     structureStatsUpgLevels[upgType], Color.cyan);
        }
        else
        {
            Debug.Log("Not enough credits for: " + upgType);
            MessagePanel.PulseMessage("Not enough Credits!", Color.red);
        }
    }

    // Consumables
    private void ApplyConsumableUpgrade(Upgrade upgrade, ConsumablesUpgrade upgType)
    {
        // Buy and apply
        int cost = Mathf.FloorToInt(consumablesCostMult * upgrade.CalculateCost(player, 
                   consumablesUpgBaseCosts[upgType], consumablesUpgLevels[upgType] + 1));

        if (BuildingSystem.PlayerCredits >= cost)
        {
            CheckAndApplyConsumable(upgrade, upgType, cost);
        }
        
        else
        {
            //Debug.Log("Not enough credits for: " + upgType);
            MessagePanel.PulseMessage("Not enough Credits!", Color.red);
        }
    }

    private void CheckAndApplyConsumable(Upgrade upgrade, ConsumablesUpgrade upgType, int cost)
    {
        // Check max level
        if (consumablesUpgLevels[upgType] >= consumablesUpgMaxLevels[upgType])
        {
            Debug.LogError("Consumable int maxed out: " + upgType);
            MessagePanel.PulseMessage("Reached max value for integers. You either bought this over " +
                                      "2 billion times,or there is a bug!  Let me know!", Color.red);
            return;
        }

        if ((ClassManager.Instance.selectedClass == PlayerClass.Fighter && 
            upgType == ConsumablesUpgrade.BuyAttackDrone) ||
            (ClassManager.Instance.selectedClass == PlayerClass.Builder && 
            upgType == ConsumablesUpgrade.BuyConstructionDrone))
            {
                cost /= 2;
            }
        
        if (upgrade.Apply(player.stats))
        {
            BuildingSystem.PlayerCredits -= cost;
            consumablesUpgLevels[upgType]++;
            consumablesUpgCurrCosts[upgType] = cost;

            upgradeEffects.PlayUpgradeButtonEffect();

            //Debug.Log("Consumable applied: " + upgType);
            MessagePanel.PulseMessage("Consumabled applied!", Color.cyan);
        }
        else
        {
            //Debug.Log(upgType + " already full");
            MessagePanel.PulseMessage("It's already full", Color.yellow);
            return;
        }
    }

    // GunStats
    private void ApplyGunUpgrade(Upgrade upgrade, GunStatsUpgrade upgType, int gunIndex)
    {
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

        // Determine which GunType's dictionaries to use
        if (gun.gunType == GunType.Laser)
        {
            gunUpgLevels = laserUpgLevels;
            gunUpgMaxLevels = laserUpgMaxLevels;
            gunUpgCurrCosts = laserUpgCurrCosts;
            gunUpgBaseCosts = laserUpgBaseCosts;
        }
        else if (gun.gunType == GunType.Bullet)
        {
            gunUpgLevels = bulletUpgLevels;
            gunUpgMaxLevels = bulletUpgMaxLevels;
            gunUpgCurrCosts = bulletUpgCurrCosts;
            gunUpgBaseCosts = bulletUpgBaseCosts;
        }
        else if (gun.gunType == GunType.RepairGun)
        {
            gunUpgLevels = repairGunUpgLevels;
            gunUpgMaxLevels = repairGunUpgMaxLevels;
            gunUpgCurrCosts = repairGunUpgCurrCosts;
            gunUpgBaseCosts = repairGunUpgBaseCosts;
        } 
        else
        {
            Debug.LogError("Unsupported gun type: " + gun.gunType);
            return;
        }

        if (gunUpgLevels[upgType] >= gunUpgMaxLevels[upgType])
        {
            Debug.Log("Max level reached for: " + upgType);
            MessagePanel.PulseMessage("Max level reached!", Color.yellow);
            return;
        }

        // Buy and apply
        int cost = Mathf.FloorToInt(gunStatsCostMult * gunUpgCurrCosts[upgType]);
        if (BuildingSystem.PlayerCredits >= cost)
        {
            BuildingSystem.PlayerCredits -= cost;
            gunUpgLevels[upgType]++;

            upgrade.Apply(gun, gunUpgLevels[upgType]);
            upgradeEffects.PlayUpgradeButtonEffect();
            
            gunUpgCurrCosts[upgType] = upgrade.CalculateCost(player, 
                                       gunUpgBaseCosts[upgType], gunUpgLevels[upgType] + 1);

            Debug.Log("Gun upgrade applied: " + upgType);
            MessagePanel.PulseMessage(gun.uiName + " stat upgraded to level " + 
                                     gunUpgLevels[upgType], Color.cyan);
        }
        else
        {
            Debug.Log("Not enough credits for: " + upgType);
            MessagePanel.PulseMessage("Not enough Credits!", Color.red);
        }
    }

    #endregion
}