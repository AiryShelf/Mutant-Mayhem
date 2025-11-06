using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UpgradeFamily
{
    PlayerStats,
    StructureStats,
    Consumables,
    GunStats,
    DroneStats
}
public enum PlayerStatsUpgrade
{
    MoveSpeed,
    StrafeSpeed_Deprecated,
    SprintFactor_Deprecated,
    PlayerReloadSpeed,
    WeaponHandling,
    MeleeDamage,
    MeleeKnockback,
    StaminaMax,
    StaminaRegen,
    HealthMax,
    HealthRegen,
    CriticalHitChance_Deprecated,
    CriticalHit
}

public enum StructureStatsUpgrade
{
    QCubeMaxHealth,
    StructureMaxHealth,
    Armour,
    MaxTurrets_Deprecated,
    TurretRotSpeed,
    TurretSensors,
    SupplyLimit
}

public enum ConsumablesUpgrade
{
    PlayerHeal,
    QCubeRepair,
    GrenadeBuyAmmo,
    SMGBuyAmmo,
    BuyConstructionDrone,
    BuyAttackDrone,
    SellConstructionDrone,
    SellAttackDrone
}

public enum GunStatsUpgrade
{
    GunDamage,
    GunKnockback,
    ShootSpeed,
    ClipSize,
    ChargeSpeed,
    GunAccuracy,
    GunRange,
    Recoil,
    TurretReloadSpeed,
}

public enum DroneStatsUpgrade
{
    DroneSpeed,
    DroneHealth,
    DroneEnergy,
    DroneHangarRange,
    DroneHangarRepairSpeed,
    DroneHangarRechargeSpeed
}

/* NOTES: 
    - After adjusting upgrade values, check UpgradeStatsGetter 
        especially if decimal place count changes
*/
public abstract class Upgrade
{
    public PlayerStatsUpgrade PlayerStatsUpgType { get; private set; }
    public StructureStatsUpgrade StructureUpgType { get; private set; }
    public ConsumablesUpgrade ConsumableUpgType { get; private set; }
    public GunStatsUpgrade GunStatsUpgType { get; private set; }

    protected Upgrade(PlayerStatsUpgrade type)
    {
        PlayerStatsUpgType = type;
    }
    protected Upgrade(StructureStatsUpgrade type)
    {
        StructureUpgType = type;
    }
    protected Upgrade(ConsumablesUpgrade type)
    {
        ConsumableUpgType = type;
    }
    protected Upgrade(GunStatsUpgrade type)
    {
        GunStatsUpgType = type;
    }

    public virtual void Apply(PlayerStats playerStats, int level) { }
    public virtual void Apply(StructureStats structureStats, int level) { }
    public virtual void Apply(TileStats tileStats, int level) { }
    public virtual void Apply(GunSO gunSO, int level) { }
    public virtual bool Apply(PlayerStats playerStats) // Consumables
    {
        return false;
    }
    public virtual void Apply(int level) { } // Drone Stats

    public virtual int CalculateCost(Player player, int baseCost, int level)
    {
        return baseCost * level;
    }
}

#region PlayerStats Upgrades

public class MoveSpeedUpgrade : Upgrade
{
    public MoveSpeedUpgrade() : base(PlayerStatsUpgrade.MoveSpeed) { }

    public static float UpgAmount = 0.15f;
    public static float LookSpeedAmount = 0.05f;
    public static float StrafeUpgAmount = 0.13f;
    public static float SprintUpgAmount = 0.005f;

    public static float GetUpgAmount()
    {
        return UpgAmount * PlanetManager.Instance.statMultipliers[PlanetStatModifier.PlayerMoveSpeed];
    }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.moveSpeed += UpgAmount * PlanetManager.Instance.statMultipliers[PlanetStatModifier.PlayerMoveSpeed];
        playerStats.lookSpeed += LookSpeedAmount * PlanetManager.Instance.statMultipliers[PlanetStatModifier.PlayerMoveSpeed];
        playerStats.strafeSpeed += StrafeUpgAmount * PlanetManager.Instance.statMultipliers[PlanetStatModifier.PlayerMoveSpeed];
        playerStats.sprintFactor += SprintUpgAmount * PlanetManager.Instance.statMultipliers[PlanetStatModifier.PlayerMoveSpeed];

        playerStats.player.RefreshMoveForces();
    }
}

public class PlayerReloadSpeedUpgrade : Upgrade
{
    public PlayerReloadSpeedUpgrade() : base(PlayerStatsUpgrade.PlayerReloadSpeed) { }

    public static float UpgAmount = 0.1f;

    public override void Apply(PlayerStats playerStats, int level)
    {
        Debug.Log("Reload Speed Applied");
        playerStats.reloadFactor += UpgAmount;
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Multiply the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.6f);
        }
        return newCost;
    }
}

public class WeaponHandlingUpgrade : Upgrade
{
    public WeaponHandlingUpgrade() : base(PlayerStatsUpgrade.WeaponHandling) { }

    public static float UpgAmount = 0.25f;

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.accuracyHoningSpeed += UpgAmount;
        float maxLevel = UpgradeManager.Instance.playerStatsUpgMaxLevels[PlayerStatsUpgrade.WeaponHandling];

        // Keeping max handling at 0.5 (0 would be no accuracy loss)
        playerStats.weaponHandling -= UpgAmount / (UpgAmount * maxLevel * 2);  
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Multiply the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.35f);
        }
        return newCost;
    }
}

public class MeleeDamageUpgrade : Upgrade
{
    public MeleeDamageUpgrade() : base(PlayerStatsUpgrade.MeleeDamage) { }

    public static float GetUpgAmount(UpgradeManager upgradeManager)
    {
        float upgAmount = 0.1f * (upgradeManager.playerStatsUpgLevels[PlayerStatsUpgrade.MeleeDamage] + 2) *
                          PlanetManager.Instance.statMultipliers[PlanetStatModifier.LaserDamage];
        return upgAmount;
    }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.meleeDamage += 0.1f * (level + 1) * PlanetManager.Instance.statMultipliers[PlanetStatModifier.LaserDamage];
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Multiply the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.3f);
        }
        return newCost;
    }
}

public class KnockbackUpgrade : Upgrade
{
    public KnockbackUpgrade() : base(PlayerStatsUpgrade.MeleeKnockback) { }

    public static float UpgAmount = 2.5f;

    public static float GetUpgAmount(UpgradeManager upgradeManager)
    {
        return UpgAmount;
    }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.knockback += UpgAmount;
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Multiply the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.5f);
        }
        return newCost;
    }
}

public class StaminaMaxUpgrade : Upgrade
{
    public StaminaMaxUpgrade() : base(PlayerStatsUpgrade.StaminaMax) { }

    public static float UpgAmount = 5;

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.staminaMax += UpgAmount;
    }
}

public class StaminaRegenUpgrade : Upgrade
{
    public StaminaRegenUpgrade() : base(PlayerStatsUpgrade.StaminaRegen) { }

    public static float UpgAmount = 0.2f;

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.staminaRegen += UpgAmount;
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Multiply the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.4f);
        }
        return newCost;
    }
}

public class HealthMaxUpgrade : Upgrade
{
    public HealthMaxUpgrade() : base(PlayerStatsUpgrade.HealthMax) { }

    public static float UpgAmount = 25;

    public static float GetUpgAmount()
    {
        return UpgAmount * PlanetManager.Instance.statMultipliers[PlanetStatModifier.PlayerHealth];
    }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.playerHealthScript.SetMaxHealth(
            playerStats.playerHealthScript.GetMaxHealth() + GetUpgAmount());
    }
}

public class HealthRegenUpgrade : Upgrade
{
    public HealthRegenUpgrade() : base(PlayerStatsUpgrade.HealthRegen) { }

    public static float UpgAmount = 0.1f;

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.playerHealthScript.healthRegenPerSec += UpgAmount;
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Multiply the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.4f);
        }
        return newCost;
    }
}


public class CriticalHitUpgrade : Upgrade
{
    public CriticalHitUpgrade() : base(PlayerStatsUpgrade.CriticalHit) { }

    public static float UpgAmount = 0.02f;

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.criticalHitChanceMult += UpgAmount;
        playerStats.criticalHitDamageMult += UpgAmount;
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Multiply the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.35f);
        }
        return newCost;
    }
}

#endregion

#region Consumables

public class PlayerHealUpgrade : Upgrade
{
    public PlayerHealUpgrade() : base(ConsumablesUpgrade.PlayerHeal) { }

    public static int HealAmount = 100;

    public static int GetUpgAmount(PlayerHealth health)
    {
        int amount;
        int missingHealth = Mathf.FloorToInt(health.GetMaxHealth() - health.GetHealth());
        
        if (missingHealth <= 0)
            amount = 0;
        else if (missingHealth < HealAmount)
            amount = missingHealth;
        else
            amount = HealAmount;

        return amount;
    }

    public override bool Apply(PlayerStats playerStats)
    {
        if (playerStats.playerHealthScript.GetHealth() <
            playerStats.playerHealthScript.GetMaxHealth())
        {
            playerStats.playerHealthScript.ModifyHealth(HealAmount, 2, Vector2.one, null);
            return true;
        }
        else
        {
            return false;
        }
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        float healthRatio = (float)GetUpgAmount(player.stats.playerHealthScript) / HealAmount;
        return Mathf.FloorToInt(baseCost * healthRatio);
    }

    public static int GetCost(Player player, int baseCost)
    {
        float healthRatio = (float)GetUpgAmount(player.stats.playerHealthScript) / HealAmount;
        return Mathf.FloorToInt(baseCost * healthRatio);
    }
}

public class QCubeRepairUpgrade : Upgrade
{
    public QCubeRepairUpgrade() : base(ConsumablesUpgrade.QCubeRepair) { }

    public static int RepairAmount = 100;

    public static int GetUpgAmount(QCubeHealth health)
    {
        int amount;
        int missingHealth = Mathf.FloorToInt(health.GetMaxHealth() - health.GetHealth());
        if (missingHealth <= 0)
            amount = 0;
        else if (missingHealth < RepairAmount)
            amount = missingHealth;
        else
            amount = RepairAmount;

        return amount;
    }

    public override bool Apply(PlayerStats playerStats)
    {
        if (playerStats.structureStats.cubeHealthScript.GetHealth() <
            playerStats.structureStats.cubeHealthScript.GetMaxHealth())
        {
            playerStats.structureStats.cubeHealthScript.ModifyHealth(100, 2, Vector2.one, null);
            return true;
        }
        else return false;
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        float healthRatio = (float)GetUpgAmount(player.stats.structureStats.cubeHealthScript) / RepairAmount;
        return Mathf.FloorToInt(baseCost * healthRatio);
    }

    public static int GetCost(Player player, int baseCost)
    {
        float healthRatio = (float)GetUpgAmount(player.stats.structureStats.cubeHealthScript) / RepairAmount;
        return Mathf.FloorToInt(baseCost * healthRatio);
    }
}

public class GrenadeBuyAmmoUpgrade : Upgrade
{
    public GrenadeBuyAmmoUpgrade() : base(ConsumablesUpgrade.GrenadeBuyAmmo) { }

    public static int AmmoAmount = 1;

    public static float GetUpgAmount()
    {
        return AmmoAmount * AugManager.Instance.grenadeAmmoMult;
    }

    public override bool Apply(PlayerStats playerStats)
    {
        playerStats.grenadeAmmo += 1 * AugManager.Instance.grenadeAmmoMult;
        return true;
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // No extra cost for consumable
        return Mathf.FloorToInt(baseCost * AugManager.Instance.grenadeCostMult);
    }

    public static int GetCost(Player player, int baseCost)
    {
        
        return Mathf.FloorToInt(baseCost * AugManager.Instance.grenadeCostMult);
    }
}

public class SMGBuyAmmoUpgrade : Upgrade
{
    public SMGBuyAmmoUpgrade() : base(ConsumablesUpgrade.SMGBuyAmmo) { }

    public static float AmmoAmount = 100;

    public override bool Apply(PlayerStats playerStats)
    {
        playerStats.playerShooter.gunsAmmo[1] += 100;
        return true;
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // No extra cost after consumable
        return baseCost;
    }

    public static int GetCost(Player player, int baseCost)
    {
        
        return baseCost;
    }
}

public class BuyConstructionDroneUpgrade : Upgrade
{
    public BuyConstructionDroneUpgrade() : base(ConsumablesUpgrade.BuyConstructionDrone) { }

    public static int Amount = 1;

    public override bool Apply(PlayerStats playerStats)
    {
        
        return DroneManager.Instance.SpawnDroneInHangar(DroneType.Builder, playerStats.structureStats.currentDroneContainer);
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        int cost = Mathf.Clamp(baseCost * (DroneManager.Instance.activeConstructionDrones.Count + 1), baseCost, int.MaxValue);
        if (ClassManager.Instance.selectedClass == PlayerClass.Builder)
            cost /= 2;
        return cost;
    }

    public static int GetCost(Player player, int baseCost)
    {
        int cost = Mathf.Clamp(baseCost * (DroneManager.Instance.activeConstructionDrones.Count + 1), baseCost, int.MaxValue);
        if (ClassManager.Instance.selectedClass == PlayerClass.Builder)
            cost /= 2;
        return cost;
    }
}

public class BuyAttackDroneUpgrade : Upgrade
{
    public BuyAttackDroneUpgrade() : base(ConsumablesUpgrade.BuyAttackDrone) { }

    public static int Amount = 1;

    public override bool Apply(PlayerStats playerStats)
    {
        return DroneManager.Instance.SpawnDroneInHangar(DroneType.Attacker, playerStats.structureStats.currentDroneContainer);
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        int activeDrones = DroneManager.Instance.activeAttackDrones.Count;

        // Increment cost
        int newDroneCount = activeDrones + 1;
        int newCost = Mathf.FloorToInt(baseCost * newDroneCount);
        if (ClassManager.Instance.selectedClass == PlayerClass.Fighter)
            newCost = Mathf.FloorToInt(newCost / 1.5f);
        //int newCost = Mathf.FloorToInt(baseCost * Mathf.Pow(2, newDroneCount));
        return newCost;
    }

    public static int GetCost(Player player, int baseCost)
    {
        int activeDrones = DroneManager.Instance.activeAttackDrones.Count;

        // Increment cost
        int newDroneCount = activeDrones + 1;
        int newCost = Mathf.FloorToInt(baseCost * newDroneCount);
        if (ClassManager.Instance.selectedClass == PlayerClass.Fighter)
            newCost = Mathf.FloorToInt(newCost / 1.5f);
        //int newCost = Mathf.FloorToInt(baseCost * Mathf.Pow(2, newDroneCount));
        return newCost;
    }
}

public class SellConstructionDroneUpgrade : Upgrade
{
    public SellConstructionDroneUpgrade() : base(ConsumablesUpgrade.SellConstructionDrone) { }

    public static int Amount = 1;

    public override bool Apply(PlayerStats playerStats)
    {
        return DroneManager.Instance.SellDrone(DroneType.Builder, playerStats.structureStats.currentDroneContainer);
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        int cost = Mathf.Clamp(baseCost * (DroneManager.Instance.activeConstructionDrones.Count), baseCost, int.MaxValue);
        if (ClassManager.Instance.selectedClass == PlayerClass.Builder)
            cost /= 2;
        return -cost; // Negative cost for selling
    }
}

#endregion

#region StructureStats Upgrades

public class QCubeMaxHealthUpgrade : Upgrade
{
    public QCubeMaxHealthUpgrade() : base(StructureStatsUpgrade.MaxTurrets_Deprecated) { }

    public static float UpgAmount = 200;

    public override void Apply(StructureStats structureStats, int level)
    {
        structureStats.cubeHealthScript.SetMaxHealth(
            structureStats.cubeHealthScript.GetMaxHealth() + UpgAmount);
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Double the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost *= 2;
        }
        return newCost;
    }
}

public class StructureMaxHealthUpgrade : Upgrade
{
    public StructureMaxHealthUpgrade() : base(StructureStatsUpgrade.StructureMaxHealth) { }

    public static float UpgAmount = 0.02f;

    public override void Apply(StructureStats structureStats, int level)
    {
        Debug.Log("StructureMaxHealth applied");
        structureStats.structureMaxHealthMult += UpgAmount;
        structureStats.tileManager.ModifyMaxHealthAll(structureStats.structureMaxHealthMult);
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Multiply the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.4f);;
        }
        return newCost;
    }
}

public class TurretRotSpeedUpgrade : Upgrade
{
    public TurretRotSpeedUpgrade() : base(StructureStatsUpgrade.TurretRotSpeed) { }

    public static float UpgAmount = 4;

    public override void Apply(StructureStats structureStats, int level)
    {
        Debug.Log("TurretRotSpeedUpg applied");
        TurretManager.Instance.UpgradeTurretStructures(base.StructureUpgType);
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Multiply the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.5f);;
        }
        return newCost;
    }
}

public class TurretSensorsUpgrade : Upgrade
{
    public TurretSensorsUpgrade() : base(StructureStatsUpgrade.TurretSensors) { }

    public static float UpgAmount = 0.5f;

    public static float GetUpgAmount()
    {
        return UpgAmount * PlanetManager.Instance.statMultipliers[PlanetStatModifier.TurretSensors];
    }

    public override void Apply(StructureStats structureStats, int level)
    {
        Debug.Log("TurretDetectRangeUpg applied");
        TurretManager.Instance.UpgradeTurretStructures(base.StructureUpgType);
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Multiply the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.5f);
        }
        return newCost;
    }
}

public class SupplyLimitUpgrade : Upgrade
{
    public SupplyLimitUpgrade() : base(StructureStatsUpgrade.SupplyLimit) { }

    public static int UpgAmount = 4;

    public static float GetUpgAmount()
    {
        return UpgAmount;
    }

    public override void Apply(StructureStats structureStats, int level)
    {
        Debug.Log("SupplyLimitUpg applied");
        SupplyManager.SupplyLimit += UpgAmount;
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // 1.5x the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.5f);
        }
        return newCost;
    }
}

#endregion

#region GunStats Upgrades

public class GunDamageUpgrade : Upgrade
{
    public GunDamageUpgrade() : base(GunStatsUpgrade.GunDamage) { }

    public static float GetUpgAmount(Player player, int gunIndex, UpgradeManager upgradeManager)
    {
        float upgAmount = 0;
        switch (gunIndex)
        {
            case 0:
                upgAmount = player.playerShooter.gunList[gunIndex].damageUpgFactor *
                            (upgradeManager.laserUpgLevels[GunStatsUpgrade.GunDamage] + 2) *
                            PlanetManager.Instance.statMultipliers[PlanetStatModifier.LaserDamage];
                return upgAmount;
            case 1:
                upgAmount = player.playerShooter.gunList[gunIndex].damageUpgFactor *
                            (upgradeManager.bulletUpgLevels[GunStatsUpgrade.GunDamage] + 2) *
                            PlanetManager.Instance.statMultipliers[PlanetStatModifier.BulletDamage];
                return upgAmount;
            case 4:
                // upgAmount does not scale up for repair gun
                upgAmount = player.playerShooter.gunList[gunIndex].damageUpgFactor *
                            PlanetManager.Instance.statMultipliers[PlanetStatModifier.RepairGunDamage];
                return upgAmount;
            default:
                return upgAmount;
        }
    }

    public override void Apply(GunSO gunSO, int level)
    {
        float amount;
        switch (gunSO.gunType)
        {
            case GunType.Laser:
                amount = gunSO.damageUpgFactor * (level + 1) * PlanetManager.Instance.statMultipliers[PlanetStatModifier.LaserDamage];
                gunSO.damage += amount;
                break;
            case GunType.Bullet:
                amount = gunSO.damageUpgFactor * (level + 1) * PlanetManager.Instance.statMultipliers[PlanetStatModifier.BulletDamage];
                gunSO.damage += amount;
                break;
            case GunType.RepairGun:
                amount = gunSO.damageUpgFactor * PlanetManager.Instance.statMultipliers[PlanetStatModifier.RepairGunDamage];
                gunSO.damage += amount;
                break;
        }
        TurretManager.Instance.UpgradeTurretGuns(gunSO.gunType, base.GunStatsUpgType, level);
        DroneManager.Instance.UpgradeDroneGuns(gunSO.gunType, base.GunStatsUpgType, level);
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Multiply the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.3f);
        }
        return newCost;
    }
}

public class GunKnockbackUpgrade : Upgrade
{
    public GunKnockbackUpgrade() : base(GunStatsUpgrade.GunKnockback) { }

    public static float GetUpgAmount(Player player, int gunIndex)
    {
        float upgAmount = player.playerShooter.gunList[gunIndex].knockbackUpgAmt;
        return upgAmount;
    }

    public override void Apply(GunSO gunSO, int level)
    {
        gunSO.knockback += gunSO.knockbackUpgAmt;
        TurretManager.Instance.UpgradeTurretGuns(gunSO.gunType, base.GunStatsUpgType, level);
        DroneManager.Instance.UpgradeDroneGuns(gunSO.gunType, base.GunStatsUpgType, level);
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Multiply the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.8f);
        }
        return newCost;
    }
}

public class ShootSpeedUpgrade : Upgrade
{
    public ShootSpeedUpgrade() : base(GunStatsUpgrade.ShootSpeed) { }

    public static float GetUpgAmount(Player player, int gunIndex)
    {
        float upgAmount = player.playerShooter.gunList[gunIndex].shootSpeedUpgNegAmt;
        return upgAmount;
    }

    public override void Apply(GunSO gunSO, int level)
    {
        gunSO.shootSpeed += gunSO.shootSpeedUpgNegAmt;
        TurretManager.Instance.UpgradeTurretGuns(gunSO.gunType, base.GunStatsUpgType, level);
        DroneManager.Instance.UpgradeDroneGuns(gunSO.gunType, base.GunStatsUpgType, level);
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Double the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 2f);
        }
        return newCost;
    }
}

public class ClipSizeUpgrade : Upgrade
{
    public ClipSizeUpgrade() : base(GunStatsUpgrade.ClipSize) { }

    public static float GetUpgAmount(Player player, int gunIndex)
    {
        float upgAmount = player.playerShooter.gunList[gunIndex].clipSizeUpgAmt;
        return upgAmount;
    }

    public override void Apply(GunSO gunSO, int level)
    {
        gunSO.clipSize += gunSO.clipSizeUpgAmt;
        TurretManager.Instance.UpgradeTurretGuns(gunSO.gunType, base.GunStatsUpgType, level);
        DroneManager.Instance.UpgradeDroneGuns(gunSO.gunType, base.GunStatsUpgType, level);
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Multiply the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.8f);
        }
        return newCost;
    }
}

public class ChargeDelayUpgrade : Upgrade
{
    public ChargeDelayUpgrade() : base(GunStatsUpgrade.ChargeSpeed) { }

    public static float GetUpgAmount(Player player, int gunIndex)
    {
        float upgAmount = player.playerShooter.gunList[gunIndex].chargeSpeedUpgNegAmt;
        return upgAmount;
    }

    public override void Apply(GunSO gunSO, int level)
    {
        gunSO.chargeDelay += gunSO.chargeSpeedUpgNegAmt;
        TurretManager.Instance.UpgradeTurretGuns(gunSO.gunType, base.GunStatsUpgType, level);
        DroneManager.Instance.UpgradeDroneGuns(gunSO.gunType, base.GunStatsUpgType, level);
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Double the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 2f);
        }
        return newCost;
    }
}

public class GunAccuracyUpgrade : Upgrade
{
    public GunAccuracyUpgrade() : base(GunStatsUpgrade.GunAccuracy) { }

    public static float GetUpgAmount(Player player, int gunIndex)
    {
        float upgAmount = player.playerShooter.gunList[gunIndex].accuracyUpgNegAmt;
        return upgAmount;
    }

    public override void Apply(GunSO gunSO, int level)
    {
        gunSO.accuracy += gunSO.accuracyUpgNegAmt;
        TurretManager.Instance.UpgradeTurretGuns(gunSO.gunType, base.GunStatsUpgType, level);
        DroneManager.Instance.UpgradeDroneGuns(gunSO.gunType, base.GunStatsUpgType, level);
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Double the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 2f);
        }
        return newCost;
    }
}

public class RangeUpgrade : Upgrade
{
    public RangeUpgrade() : base(GunStatsUpgrade.GunRange) { }

    public static float GetUpgAmount(Player player, int gunIndex)
    {
        float upgAmount = player.playerShooter.gunList[gunIndex].bulletRangeUpgAmt;
        return upgAmount;
    }

    public override void Apply(GunSO gunSO, int level)
    {
        gunSO.bulletLifeTime += gunSO.bulletRangeUpgAmt;
        TurretManager.Instance.UpgradeTurretGuns(gunSO.gunType, base.GunStatsUpgType, level);
        DroneManager.Instance.UpgradeDroneGuns(gunSO.gunType, base.GunStatsUpgType, level);
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Double the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 2f);
        }
        return newCost;
    }
}

public class RecoilUpgrade : Upgrade
{
    // ******** DEPRICATED ********
    public RecoilUpgrade() : base(GunStatsUpgrade.Recoil) { }

    public static float GetUpgAmount(Player player, int gunIndex)
    {
        float upgAmount = player.playerShooter.gunList[gunIndex].kickbackUpgNegAmt;
        return upgAmount;
    }

    public override void Apply(GunSO gunSO, int level)
    {
        gunSO.kickback += gunSO.kickbackUpgNegAmt;
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Multiply the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.4f);
        }
        return newCost;
    }
}

public class TurretReloadSpeedUpgrade : Upgrade
{
    public TurretReloadSpeedUpgrade() : base(GunStatsUpgrade.TurretReloadSpeed) { }

    public static float GetUpgAmount(Player player, int gunIndex)
    {
        TurretGunSO turretGunSO = TurretManager.Instance.turretGunList[gunIndex];
        float upgAmount = turretGunSO.reloadSpeedUpgNegAmt;

        return upgAmount;
    }

    public override void Apply(GunSO gunSO, int level)
    {
        TurretManager.Instance.UpgradeTurretGuns(gunSO.gunType, base.GunStatsUpgType, level);
        DroneManager.Instance.UpgradeDroneGuns(gunSO.gunType, base.GunStatsUpgType, level);
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Multiply the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.5f);
        }
        return newCost;
    }
}

#endregion

#region DroneStats Upgrades

public class DroneSpeedUpgrade : Upgrade
{
    public DroneSpeedUpgrade() : base(GunStatsUpgrade.GunRange) { }

    public static float GetUpgAmount()
    {
        return DroneManager.Instance.droneSpeedUpgMult;
    }

    public override void Apply(int level)
    {
        DroneManager.Instance.UpgradeDroneSpeed(level);
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Multiply the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.5f);
        }
        return newCost;
    }
}

public class DroneHealthUpgrade : Upgrade
{
    public DroneHealthUpgrade() : base(GunStatsUpgrade.GunRange) { }

    public static float GetUpgAmount()
    {
        return DroneManager.Instance.droneHealthUpgMult;
    }

    public override void Apply(int level)
    {
        DroneManager.Instance.UpgradeDroneHealth(level);
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Multiply the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.5f);
        }
        return newCost;
    }
}

public class DroneEnergyUpgrade : Upgrade
{
    public DroneEnergyUpgrade() : base(GunStatsUpgrade.GunRange) { }

    public static float GetUpgAmount()
    {
        return DroneManager.Instance.droneEnergyUpgMult;
    }

    public override void Apply(int level)
    {
        DroneManager.Instance.UpgradeDroneEnergy(level);
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Multiply the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.5f);
        }
        return newCost;
    }
}

public class DroneHangarRangeUpgrade : Upgrade
{
    public DroneHangarRangeUpgrade() : base(GunStatsUpgrade.GunRange) { }

    public static float GetUpgAmount()
    {
        return DroneManager.Instance.droneHangarRangeUpgAmount;
    }

    public override void Apply(int level)
    {
        DroneManager.Instance.UpgradeDroneHangarRange(level);
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Multiply the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.5f);
        }
        return newCost;
    }
}

public class DroneHangarRepairSpeedUpgrade : Upgrade
{
    public DroneHangarRepairSpeedUpgrade() : base(GunStatsUpgrade.GunRange) { }

    public static float GetUpgAmount()
    {
        return DroneManager.Instance.droneHangarRepairSpeedUpgAmount;
    }

    public override void Apply(int level)
    {
        DroneManager.Instance.UpgradeDroneHangarRepairSpeed(level);
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Multiply the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.5f);
        }
        return newCost;
    }
}

public class DroneHangarRechargeSpeedUpgrade : Upgrade
{
    public DroneHangarRechargeSpeedUpgrade() : base(GunStatsUpgrade.GunRange) { }

    public static float GetUpgAmount()
    {
        return DroneManager.Instance.droneHangarRechargeSpeedUpgAmount;
    }

    public override void Apply(int level)
    {
        DroneManager.Instance.UpgradeDroneHangarRechargeSpeed(level);
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Multiply the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.5f);
        }
        return newCost;
    }
}

#endregion