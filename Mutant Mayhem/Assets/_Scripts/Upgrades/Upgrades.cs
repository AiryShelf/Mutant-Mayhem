using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;

public enum UpgradeType
{
    MoveSpeed,
    //LookSpeed,  lookSpeed should be combined with MoveSpeed
    StrafeSpeed,
    SprintFactor,
    ReloadSpeed,
    MeleeDamage,
    Knockback,
    MeleeAttackRate,
    StaminaMax,
    StaminaRegen,
    HealthMax,
    HealthRegen,
    Accuracy,
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

    public virtual int CalculateCost(int baseCost, int level)
    {
        return baseCost * level;
    }
}

public class MoveSpeedUpgrade : Upgrade
{
    public MoveSpeedUpgrade() : base(UpgradeType.MoveSpeed) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.moveSpeed += 0.1f;
    }
}

public class StrafeSpeedUpgrade : Upgrade
{
    public StrafeSpeedUpgrade() : base(UpgradeType.StrafeSpeed) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.strafeSpeed += 0.05f;
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
    public ReloadSpeedUpgrade() : base(UpgradeType.ReloadSpeed) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        playerStats.reloadFactor -= 0.05f;
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
    public KnockbackUpgrade() : base(UpgradeType.Knockback) { }

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
        // Do something
    }
}

public class StaminaMaxUpgrade : Upgrade
{
    public StaminaMaxUpgrade() : base(UpgradeType.StaminaMax) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        // Do something
    }
}

public class StaminaRegenUpgrade : Upgrade
{
    public StaminaRegenUpgrade() : base(UpgradeType.StaminaRegen) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        // Do something
    }
}

public class HealthMaxUpgrade : Upgrade
{
    public HealthMaxUpgrade() : base(UpgradeType.HealthMax) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        // Do something
    }
}

public class HealthRegenUpgrade : Upgrade
{
    public HealthRegenUpgrade() : base(UpgradeType.HealthRegen) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        // Do something
    }
}

public class AccuracyUpgrade : Upgrade
{
    public AccuracyUpgrade() : base(UpgradeType.Accuracy) { }

    public override void Apply(PlayerStats playerStats, int level)
    {
        // Do something
    }
}
