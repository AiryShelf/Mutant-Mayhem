using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UpgradeFamily
{
    PlayerStats,
    StructureStats,
    Consumables,
    GunStats,
}
public enum PlayerStatsUpgrade
{
    MoveSpeed,
    StrafeSpeed,
    SprintFactor,
    PlayerReloadSpeed,
    WeaponHandling,
    MeleeDamage,
    MeleeKnockback,
    StaminaMax,
    StaminaRegen,
    HealthMax,
    HealthRegen,
    CriticalHitChance,
    CriticalHitDamage
}

public enum StructureStatsUpgrade
{
    QCubeMaxHealth,
    StructureMaxHealth,
    Armour,
    MaxTurrets,
    TurretRotSpeed,
    TurretSensors,
}

public enum ConsumablesUpgrade
{
    PlayerHeal,
    QCubeRepair,
    GrenadeBuyAmmo,
    SMGBuyAmmo,
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
    // Consumables below
    public virtual bool Apply(PlayerStats playerStats) 
    { 
        return false; 
    }

    public virtual int CalculateCost(Player player, int baseCost, int level) 
    { 
        return baseCost * level; 
    }
}

#region PlayerStats Upgrades

public class MoveSpeedUpgrade : Upgrade
{
    public MoveSpeedUpgrade() : base(PlayerStatsUpgrade.MoveSpeed) { }

    public static float UpgAmount = 0.2f;

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.moveSpeed += UpgAmount;
        playerStats.lookSpeed += 0.1f;
        playerStats.player.RefreshMoveForces();
    }
}

public class StrafeSpeedUpgrade : Upgrade
{
    public StrafeSpeedUpgrade() : base(PlayerStatsUpgrade.StrafeSpeed) { }

    public static float UpgAmount = 0.17f;

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.strafeSpeed += UpgAmount;
        playerStats.player.RefreshMoveForces();
    }
}

public class SprintFactorUpgrade : Upgrade
{
    public SprintFactorUpgrade() : base(PlayerStatsUpgrade.SprintFactor) { }

    public static float UpgAmount = 0.015f;

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.sprintFactor += UpgAmount;
        playerStats.player.RefreshMoveForces();
    }
}

public class PlayerReloadSpeedUpgrade : Upgrade
{
    public PlayerReloadSpeedUpgrade() : base(PlayerStatsUpgrade.PlayerReloadSpeed) { }

    public static float UpgAmount = 0.1f;

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.reloadFactor += UpgAmount;
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
        // Double the cost each level
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
        float upgAmount = 0.2f * (upgradeManager.playerStatsUpgLevels[PlayerStatsUpgrade.MeleeDamage] + 2);
        return upgAmount;
    }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.meleeDamage += 0.25f * (level + 1);
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Double the cost each level
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

    public static float GetUpgAmount(UpgradeManager upgradeManager)
    {
        float upgAmount = 0.5f;
        return upgAmount;
    }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.knockback += 0.5f;
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Double the cost each level
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

    public static float UpgAmount = 2;

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
        // Double the cost each level
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

    public static float UpgAmount = 20;

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.playerHealthScript.SetMaxHealth(
            playerStats.playerHealthScript.GetMaxHealth() + UpgAmount);
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
        // Double the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.4f);
        }
        return newCost;
    }
}

public class CriticalHitChanceUpgrade : Upgrade
{
    public CriticalHitChanceUpgrade() : base(PlayerStatsUpgrade.CriticalHitChance) { }

    public static float UpgAmount = 0.02f;

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.criticalHitChanceMult += UpgAmount;
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Double the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.35f);
        }
        return newCost;
    }
}

public class CriticalHitDamageUpgrade : Upgrade
{
    public CriticalHitDamageUpgrade() : base(PlayerStatsUpgrade.CriticalHitDamage) { }

    public static float UpgAmount = 0.02f;

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.criticalHitDamageMult += UpgAmount;
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Double the cost each level
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

    public static int AmmoAmount = 1 * AugManager.Instance.grenadeAmmoMult;

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

#endregion

#region StructureStats Upgrades

public class QCubeMaxHealthUpgrade : Upgrade
{
    public QCubeMaxHealthUpgrade() : base(StructureStatsUpgrade.MaxTurrets) { }

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
        // Double the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.4f);;
        }
        return newCost;
    }
}

public class MaxTurretsUpgrade : Upgrade
{
    public MaxTurretsUpgrade() : base(StructureStatsUpgrade.StructureMaxHealth) { }

    public static float UpgAmount = 1;

    public override void Apply(StructureStats structureStats, int level)
    {
        Debug.Log("MaxTurretsUpg applied");
        structureStats.maxTurrets += (int)UpgAmount;
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
        // Double the cost each level
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

    public override void Apply(StructureStats structureStats, int level)
    {
        Debug.Log("TurretDetectRangeUpg applied");
        TurretManager.Instance.UpgradeTurretStructures(base.StructureUpgType);
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Double the cost each level
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
                            (upgradeManager.laserUpgLevels[GunStatsUpgrade.GunDamage] + 2);
                return upgAmount;
            case 1:
                upgAmount = player.playerShooter.gunList[gunIndex].damageUpgFactor *
                            (upgradeManager.bulletUpgLevels[GunStatsUpgrade.GunDamage] + 2);
                return upgAmount;
            case 9:
                // upgAmount should not scale up for repair gun
                upgAmount = player.playerShooter.gunList[gunIndex].damageUpgFactor;
                return upgAmount;
            default:
                return upgAmount; 
        }
    }

    public override void Apply(GunSO gunSO, int level)
    {
        if (gunSO.uiName == "Repair Gun")
            gunSO.damage += gunSO.damageUpgFactor;
        else
        {
            gunSO.damage += gunSO.damageUpgFactor * (level + 1);
            TurretManager.Instance.UpgradeTurretGuns(gunSO.gunType, base.GunStatsUpgType, level);
        }
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Double the cost each level
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
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Double the cost each level
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
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Double the cost each level
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
        // Double the cost each level
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
    }

    public override int CalculateCost(Player player, int baseCost, int level)
    {
        // Double the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i++)
        {
            newCost = Mathf.CeilToInt(newCost * 1.5f);
        }
        return newCost;
    }
}

#endregion