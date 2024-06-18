using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UpgradeFamily
{
    PlayerStats,
    QCubeStats,
    Consumables,
    GunStats,
}
public enum PlayerStatsUpgrade
{
    // PlayerStats
    MoveSpeed,
    StrafeSpeed,
    SprintFactor,
    PlayerReloadSpeed,
    PlayerAccuracy,
    MeleeDamage,
    MeleeKnockback,
    MeleeAttackRate,
    StaminaMax,
    StaminaRegen,
    HealthMax,
    HealthRegen,
}

public enum QCubeStatsUpgrade
{
    // QCubeStats
    QCubeMaxHealth,
}

public enum ConsumablesUpgrade
{
    // Consumables
    PlayerHeal,
    QCubeRepair,
    GrenadeBuyAmmo,
    SMGBuyAmmo,
}

public enum GunStatsUpgrade
{
    // GunStats
    GunDamage,
    GunKnockback,
    ShootSpeed,
    ClipSize,
    ChargeDelay,
    GunAccuracy,
    GunRange,
    Recoil,
}

public abstract class Upgrade
{
    public PlayerStatsUpgrade PlayerStatsUpgType { get; private set; }
    public QCubeStatsUpgrade QCubeStatsUpgType { get; private set; }
    public ConsumablesUpgrade ConsumUpgType { get; private set; }
    public GunStatsUpgrade GunStatsUpgType { get; private set; }

    protected Upgrade(PlayerStatsUpgrade type)
    {
        PlayerStatsUpgType = type;
    }
    protected Upgrade(QCubeStatsUpgrade type)
    {
        QCubeStatsUpgType = type;
    }
    protected Upgrade(ConsumablesUpgrade type)
    {
        ConsumUpgType = type;
    }
    protected Upgrade(GunStatsUpgrade type)
    {
        GunStatsUpgType = type;
    }

    public virtual void Apply(PlayerStats playerStats, int level) { }
    public virtual void Apply(QCubeStats qCubeStats, int level) { }
    public virtual void Apply(TileStats tileStats, int level) { }
    public virtual void Apply(GunSO gunSO, int level) { }
    // Consumables vv
    public virtual bool Apply(PlayerStats playerStats) 
    { 
        return false; 
    }

    public virtual int CalculateCost(int baseCost, int level) 
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
        playerStats.lookSpeed += 0.001f;
    }
}

public class StrafeSpeedUpgrade : Upgrade
{
    public StrafeSpeedUpgrade() : base(PlayerStatsUpgrade.StrafeSpeed) { }

    public static float UpgAmount = 0.1f;

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.strafeSpeed += UpgAmount;
    }
}

public class SprintFactorUpgrade : Upgrade
{
    public SprintFactorUpgrade() : base(PlayerStatsUpgrade.SprintFactor) { }

    public static float UpgAmount = 0.05f;

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.sprintFactor += UpgAmount;
    }
}

public class ReloadSpeedUpgrade : Upgrade
{
    public ReloadSpeedUpgrade() : base(PlayerStatsUpgrade.PlayerReloadSpeed) { }

    public static float UpgAmount = 0.1f;

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.reloadFactor += UpgAmount;
    }
}

public class PlayerAccuracyUpgrade : Upgrade
{
    public PlayerAccuracyUpgrade() : base(PlayerStatsUpgrade.PlayerAccuracy) { }

    public static float UpgAmount = 0.1f;

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.accuracy -= UpgAmount;
        playerStats.accuracy = Mathf.Clamp(playerStats.accuracy, 0, 1);
    }

    public override int CalculateCost(int baseCost, int level)
    {
        // Double the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i ++)
        {
            newCost *= 2;
        }
        return newCost;
    }
}

public class MeleeDamageUpgrade : Upgrade
{
    public MeleeDamageUpgrade() : base(PlayerStatsUpgrade.MeleeDamage) { }

    public static float GetUpgAmount(UpgradeSystem upgradeSystem)
    {
        float upgAmount = 0.5f * (upgradeSystem.playerStatsUpgLevels[PlayerStatsUpgrade.MeleeDamage] + 1);
        return upgAmount;
    }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.meleeDamage += 0.5f * level;
    }
}

public class KnockbackUpgrade : Upgrade
{
    public KnockbackUpgrade() : base(PlayerStatsUpgrade.MeleeKnockback) { }

    public static float GetUpgAmount(UpgradeSystem upgradeSystem)
    {
        float upgAmount = 0.5f;
        return upgAmount;
    }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.knockback += 0.5f;
    }
}

public class MeleeAttackRateUpgrade : Upgrade
{
    // NOT USING THIS UPGRADE, DON'T PLAN TO SO FAR EITHER
    public MeleeAttackRateUpgrade() : base(PlayerStatsUpgrade.MeleeAttackRate) { }

    public static float UpgAmount = 0.02f;

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.meleeSpeedFactor += UpgAmount;
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
}

public class HealthMaxUpgrade : Upgrade
{
    public HealthMaxUpgrade() : base(PlayerStatsUpgrade.HealthMax) { }

    public static float UpgAmount = 100;

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.playerHealthScript.SetMaxHealth(
            playerStats.playerHealthScript.GetMaxHealth() + UpgAmount);
    }
}

public class HealthRegenUpgrade : Upgrade
{
    public HealthRegenUpgrade() : base(PlayerStatsUpgrade.HealthRegen) { }

    public static float UpgAmount = 0.05f;

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.playerHealthScript.healthRegenPerSec += UpgAmount;
    }
}

#endregion

// Consumables grouped with PlayerStats in dicts and lists
#region Consumables

public class PlayerHealUpgrade : Upgrade
{
    public PlayerHealUpgrade() : base(ConsumablesUpgrade.PlayerHeal) { }

    public static int HealAmount = 100;

    public override bool Apply(PlayerStats playerStats)
    {
        if (playerStats.playerHealthScript.GetHealth() <
            playerStats.playerHealthScript.GetMaxHealth())
        {
            playerStats.playerHealthScript.ModifyHealth(HealAmount, null);
            return true;
        }
        else
        {
            return false;
        }
    }

    public override int CalculateCost(int baseCost, int level)
    {
        // No extra cost after consumable
        return baseCost;
    }
}

public class QCubeRepairUpgrade : Upgrade
{
    public QCubeRepairUpgrade() : base(ConsumablesUpgrade.QCubeRepair) { }

    public static int RepairAmount = 100;

    public override bool Apply(PlayerStats playerStats)
    {
        if (playerStats.qCubeStats.healthScript.GetHealth() <
            playerStats.qCubeStats.healthScript.GetMaxHealth())
        {
            playerStats.qCubeStats.healthScript.ModifyHealth(100, null);
            return true;
        }
        else return false;
    }

    public override int CalculateCost(int baseCost, int level)
    {
        // No extra cost after consumable
        return baseCost;
    }
}

public class GrenadeBuyAmmoUpgrade : Upgrade
{
    public GrenadeBuyAmmoUpgrade() : base(ConsumablesUpgrade.GrenadeBuyAmmo) { }

    public static int AmmoAmount = 1;

    public override bool Apply(PlayerStats playerStats)
    {
        playerStats.grenadeAmmo += AmmoAmount;
        return true;
    }

    public override int CalculateCost(int baseCost, int level)
    {
        // No extra cost after consumable
        return baseCost;
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

    public override int CalculateCost(int baseCost, int level)
    {
        // No extra cost after consumable
        return baseCost;
    }
}

#endregion

#region QCube Stats Upgrades

public class QCubeMaxHealthUpgrade : Upgrade
{
    public QCubeMaxHealthUpgrade() : base(QCubeStatsUpgrade.QCubeMaxHealth) { }

    public static float UpgAmount = 200;

    public override void Apply(QCubeStats qCubeStats, int level)
    {
        qCubeStats.healthScript.SetMaxHealth(
            qCubeStats.healthScript.GetMaxHealth() + UpgAmount);
    }

    public override int CalculateCost(int baseCost, int level)
    {
        // Double the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i ++)
        {
            newCost *= 2;
        }
        return newCost;
    }
}

#endregion

#region Gun Stats Upgrades

public class GunDamageUpgrade : Upgrade
{
    public GunDamageUpgrade() : base(GunStatsUpgrade.GunDamage) { }

    public static float GetUpgAmount(Player player, int gunIndex, UpgradeSystem upgradeSystem)
    {
        float upgAmount = 0;
        switch (gunIndex)
        {
            case 0:
                upgAmount = 0.5f * (player.playerShooter.gunList[gunIndex].damageUpgAmt + 
                            upgradeSystem.laserPistolUpgLevels[GunStatsUpgrade.GunDamage] + 1);
                return upgAmount;
            case 1:
                upgAmount = 0.5f * (player.playerShooter.gunList[gunIndex].damageUpgAmt + 
                            upgradeSystem.SMGUpgLevels[GunStatsUpgrade.GunDamage] + 1);
                return upgAmount;
            default:
                return upgAmount;
        }
    }

    public override void Apply(GunSO gunSO, int level)
    {
        gunSO.damage += 0.5f * (gunSO.damageUpgAmt + level);
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
    }
}

public class ChargeDelayUpgrade : Upgrade
{
    public ChargeDelayUpgrade() : base(GunStatsUpgrade.ChargeDelay) { }

    public static float GetUpgAmount(Player player, int gunIndex)
    {
        float upgAmount = player.playerShooter.gunList[gunIndex].chargeDelayUpgNegAmt;
        return upgAmount;
    }

    public override void Apply(GunSO gunSO, int level)
    {
        gunSO.chargeDelay += gunSO.chargeDelayUpgNegAmt;
    }
}

public class GunAccuracyUpgrade : Upgrade
{
    public GunAccuracyUpgrade() : base(GunStatsUpgrade.GunAccuracy) { }

    public override void Apply(GunSO gunSO, int level)
    {
        gunSO.accuracy += gunSO.accuracyUpgNegAmt;
    }

    public static float GetUpgAmount(Player player, int gunIndex)
    {
        float upgAmount = player.playerShooter.gunList[gunIndex].accuracyUpgNegAmt;
        return upgAmount;
    }

    public override int CalculateCost(int baseCost, int level)
    {
        // Double the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i ++)
        {
            newCost *= 2;
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
    }

    public override int CalculateCost(int baseCost, int level)
    {
        // Double the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i ++)
        {
            newCost *= 2;
        }
        return newCost;
    }
}

public class RecoilUpgrade : Upgrade
{
    public RecoilUpgrade() : base(GunStatsUpgrade.Recoil) { }

    public static float GetUpgAmount(Player player, int gunIndex)
    {
        float upgAmount = player.playerShooter.gunList[gunIndex].recoilUpgNegAmt;
        return upgAmount;
    }

    public override void Apply(GunSO gunSO, int level)
    {
        gunSO.recoil += gunSO.recoilUpgNegAmt;
    }

    public override int CalculateCost(int baseCost, int level)
    {
        // Double the cost each level
        int newCost = baseCost;
        for (int i = 1; i < level; i ++)
        {
            newCost *= 2;
        }
        return newCost;
    }
}

#endregion