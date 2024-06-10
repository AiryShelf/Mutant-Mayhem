using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEditor.EditorTools;
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

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.moveSpeed += 0.2f;
        playerStats.lookSpeed += 0.002f;
    }
}

public class StrafeSpeedUpgrade : Upgrade
{
    public StrafeSpeedUpgrade() : base(PlayerStatsUpgrade.StrafeSpeed) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.strafeSpeed += 0.1f;
    }
}

public class SprintFactorUpgrade : Upgrade
{
    public SprintFactorUpgrade() : base(PlayerStatsUpgrade.SprintFactor) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.sprintFactor += 0.05f;
    }
}

public class ReloadSpeedUpgrade : Upgrade
{
    public ReloadSpeedUpgrade() : base(PlayerStatsUpgrade.PlayerReloadSpeed) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.reloadFactor += 0.1f;
    }
}

public class MeleeDamageUpgrade : Upgrade
{
    public MeleeDamageUpgrade() : base(PlayerStatsUpgrade.MeleeDamage) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.meleeDamage += 1 * level;
    }
}

public class KnockbackUpgrade : Upgrade
{
    public KnockbackUpgrade() : base(PlayerStatsUpgrade.MeleeKnockback) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.knockback += 0.1f * level;
    }
}

public class MeleeAttackRateUpgrade : Upgrade
{
    public MeleeAttackRateUpgrade() : base(PlayerStatsUpgrade.MeleeAttackRate) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.meleeSpeedFactor += 0.02f;
    }
}

public class StaminaMaxUpgrade : Upgrade
{
    public StaminaMaxUpgrade() : base(PlayerStatsUpgrade.StaminaMax) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.staminaMax += 5;
    }
}

public class StaminaRegenUpgrade : Upgrade
{
    public StaminaRegenUpgrade() : base(PlayerStatsUpgrade.StaminaRegen) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.staminaRegen += 0.2f;
    }
}

public class HealthMaxUpgrade : Upgrade
{
    public HealthMaxUpgrade() : base(PlayerStatsUpgrade.HealthMax) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.playerHealthScript.SetMaxHealth(
            playerStats.playerHealthScript.GetMaxHealth() + 100);
    }
}

public class HealthRegenUpgrade : Upgrade
{
    public HealthRegenUpgrade() : base(PlayerStatsUpgrade.HealthRegen) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.playerHealthScript.healthRegenPerSec += 0.05f;
    }
}

public class PlayerAccuracyUpgrade : Upgrade
{
    public PlayerAccuracyUpgrade() : base(PlayerStatsUpgrade.PlayerAccuracy) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.accuracy -= 0.1f;
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

#endregion

// Consumables grouped with PlayerStats in dicts and lists
#region Consumables

public class PlayerHealUpgrade : Upgrade
{
    public PlayerHealUpgrade() : base(ConsumablesUpgrade.PlayerHeal) { }

    public override bool Apply(PlayerStats playerStats)
    {
        if (playerStats.playerHealthScript.GetHealth() <
            playerStats.playerHealthScript.GetMaxHealth())
        {
            playerStats.playerHealthScript.ModifyHealth(100, null);
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

    public override bool Apply(PlayerStats playerStats)
    {
        playerStats.grenadeAmmo++;
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

    public override void Apply(QCubeStats qCubeStats, int level)
    {
        qCubeStats.healthScript.SetMaxHealth(
            qCubeStats.healthScript.GetMaxHealth() + 200);
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

    public override void Apply(GunSO gunSO, int level)
    {
        gunSO.damage += gunSO.damageUpgAmt + level;
    }
}

public class GunKnockbackUpgrade : Upgrade
{
    public GunKnockbackUpgrade() : base(GunStatsUpgrade.GunKnockback) { }

    public override void Apply(GunSO gunSO, int level)
    {
        gunSO.knockback += gunSO.knockbackUpgAmt;
    }
}

public class ShootSpeedUpgrade : Upgrade
{
    public ShootSpeedUpgrade() : base(GunStatsUpgrade.ShootSpeed) { }

    public override void Apply(GunSO gunSO, int level)
    {
        gunSO.shootSpeed += gunSO.shootSpeedUpgNegAmt;
    }
}

public class ClipSizeUpgrade : Upgrade
{
    public ClipSizeUpgrade() : base(GunStatsUpgrade.ClipSize) { }

    public override void Apply(GunSO gunSO, int level)
    {
        gunSO.clipSize += gunSO.clipSizeUpgAmt;
    }
}

public class ChargeDelayUpgrade : Upgrade
{
    public ChargeDelayUpgrade() : base(GunStatsUpgrade.ChargeDelay) { }

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