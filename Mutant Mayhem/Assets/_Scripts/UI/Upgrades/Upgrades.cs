using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEditor.EditorTools;
using UnityEngine;

public enum UpgradeType
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

    // GunStats
    GunDamage,
    GunKnockback,
    ShootSpeed,
    ClipSize,
    ChargeDelay,
    GunAccuracy,
    GunRange,
    Recoil 
}

public abstract class Upgrade
{
    public UpgradeType Type { get; private set; }

    protected Upgrade(UpgradeType type)
    {
        Type = type;
    }

    public virtual void Apply(PlayerStats playerStats, int level) { }
    //public virtual void Apply(WeaponStats weaponStats, int level) { }
    public virtual void Apply(TileStats tileStats, int level) { }
    public virtual void Apply(GunSO gunSO, int level) { }

    public virtual int CalculateCost(int baseCost, int level)
    {
        return baseCost * level;
    }
}

#region PlayerStats Upgrades

public class MoveSpeedUpgrade : Upgrade
{
    public MoveSpeedUpgrade() : base(UpgradeType.MoveSpeed) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.moveSpeed += 0.2f;
        playerStats.lookSpeed += 0.002f;
    }
}

public class StrafeSpeedUpgrade : Upgrade
{
    public StrafeSpeedUpgrade() : base(UpgradeType.StrafeSpeed) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.strafeSpeed += 0.1f;
    }
}

public class SprintFactorUpgrade : Upgrade
{
    public SprintFactorUpgrade() : base(UpgradeType.SprintFactor) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.sprintFactor += 0.05f;
    }
}

public class ReloadSpeedUpgrade : Upgrade
{
    public ReloadSpeedUpgrade() : base(UpgradeType.PlayerReloadSpeed) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.reloadFactor += 0.1f;
    }
}

public class MeleeDamageUpgrade : Upgrade
{
    public MeleeDamageUpgrade() : base(UpgradeType.MeleeDamage) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.meleeDamage += 1 * level;
    }
}

public class KnockbackUpgrade : Upgrade
{
    public KnockbackUpgrade() : base(UpgradeType.MeleeKnockback) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.knockback += 0.1f * level;
    }
}

public class MeleeAttackRateUpgrade : Upgrade
{
    public MeleeAttackRateUpgrade() : base(UpgradeType.MeleeAttackRate) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.meleeSpeedFactor += 0.02f;
    }
}

public class StaminaMaxUpgrade : Upgrade
{
    public StaminaMaxUpgrade() : base(UpgradeType.StaminaMax) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.staminaMax += 5;
    }
}

public class StaminaRegenUpgrade : Upgrade
{
    public StaminaRegenUpgrade() : base(UpgradeType.StaminaRegen) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.staminaRegen += 0.2f;
    }
}

public class HealthMaxUpgrade : Upgrade
{
    public HealthMaxUpgrade() : base(UpgradeType.HealthMax) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.healthMax += 100;
    }
}

public class HealthRegenUpgrade : Upgrade
{
    public HealthRegenUpgrade() : base(UpgradeType.HealthRegen) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.healthRegen += 0.05f;
    }
}

public class PlayerAccuracyUpgrade : Upgrade
{
    public PlayerAccuracyUpgrade() : base(UpgradeType.PlayerAccuracy) { }

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

#region Gun Stats Upgrades

public class GunDamageUpgrade : Upgrade
{
    public GunDamageUpgrade() : base(UpgradeType.GunDamage) { }

    public override void Apply(GunSO gunSO, int level)
    {
        gunSO.damage += gunSO.damageUpgAmt + level;
    }
}

public class GunKnockbackUpgrade : Upgrade
{
    public GunKnockbackUpgrade() : base(UpgradeType.GunKnockback) { }

    public override void Apply(GunSO gunSO, int level)
    {
        gunSO.knockback += gunSO.knockbackUpgAmt;
    }
}

public class ShootSpeedUpgrade : Upgrade
{
    public ShootSpeedUpgrade() : base(UpgradeType.ShootSpeed) { }

    public override void Apply(GunSO gunSO, int level)
    {
        gunSO.shootSpeed += gunSO.shootSpeedUpgNegAmt;
    }
}

public class ClipSizeUpgrade : Upgrade
{
    public ClipSizeUpgrade() : base(UpgradeType.ClipSize) { }

    public override void Apply(GunSO gunSO, int level)
    {
        gunSO.clipSize += gunSO.clipSizeUpgAmt;
    }
}

public class ChargeDelayUpgrade : Upgrade
{
    public ChargeDelayUpgrade() : base(UpgradeType.ChargeDelay) { }

    public override void Apply(GunSO gunSO, int level)
    {
        gunSO.chargeDelay += gunSO.chargeDelayUpgNegAmt;
    }
}

public class GunAccuracyUpgrade : Upgrade
{
    public GunAccuracyUpgrade() : base(UpgradeType.GunAccuracy) { }

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
    public RangeUpgrade() : base(UpgradeType.GunRange) { }

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
    public RecoilUpgrade() : base(UpgradeType.Recoil) { }

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