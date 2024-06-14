using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UpgStatGetter
{
    #region Stat Values

    public static string GetStatValue(Player player, PlayerStatsUpgrade playerStatsUpgrade)
    {
        string stat = "null";

        switch (playerStatsUpgrade)
        {
            case PlayerStatsUpgrade.MoveSpeed:
                stat = player.stats.moveSpeed.ToString();
                return stat;

            case PlayerStatsUpgrade.StrafeSpeed:
                stat = player.stats.strafeSpeed.ToString();
                return stat;

            case PlayerStatsUpgrade.SprintFactor:
                stat = player.stats.sprintFactor.ToString();
                return stat;
            
            case PlayerStatsUpgrade.PlayerReloadSpeed:
                stat = (1 - player.stats.reloadFactor).ToString();
                return stat;

            case PlayerStatsUpgrade.PlayerAccuracy:
                stat = (1 - player.stats.accuracy).ToString();
                return stat;
            
            case PlayerStatsUpgrade.MeleeDamage:
                stat = player.stats.meleeDamage.ToString();
                return stat;

            case PlayerStatsUpgrade.MeleeKnockback:
                stat = player.stats.knockback.ToString();
                return stat;

            case PlayerStatsUpgrade.StaminaMax:
                stat = player.stats.staminaMax.ToString();
                return stat;

            case PlayerStatsUpgrade.StaminaRegen:
                stat = player.stats.staminaRegen.ToString();
                return stat;

            case PlayerStatsUpgrade.HealthMax:
                stat = player.stats.playerHealthScript.GetMaxHealth().ToString();
                return stat;

            case PlayerStatsUpgrade.HealthRegen:
                stat = player.stats.playerHealthScript.healthRegenPerSec.ToString();
                return stat;
        }

        return stat;
    }

    public static string GetStatValue(Player player, ConsumablesUpgrade consumablesUpgrade)
    {
        string stat = "";

        // No stat to track for consumables

        return stat;
    }

    public static string GetStatValue(Player player, QCubeStatsUpgrade qCubeStatsUpgrade)
    {
        string stat = "null";

        switch(qCubeStatsUpgrade)
        {
            case QCubeStatsUpgrade.QCubeMaxHealth:
                stat = player.stats.qCubeStats.healthScript.GetMaxHealth().ToString();
                return stat;
        }

        return stat;
    }

    public static string GetStatValue(Player player, GunStatsUpgrade gunStatsUpgrade, int gunIndex)
    {
        string stat = "null";

        switch (gunStatsUpgrade)
        {
            case GunStatsUpgrade.GunDamage:
                stat = player.stats.playerShooter.gunList[gunIndex].damage.ToString();
                return stat;

            case GunStatsUpgrade.GunKnockback:
                stat = player.stats.playerShooter.gunList[gunIndex].knockback.ToString();
                return stat;

            case GunStatsUpgrade.ShootSpeed:
                float speed = 1 - player.stats.playerShooter.gunList[gunIndex].shootSpeed;
                stat = speed.ToString();
                return stat;

            case GunStatsUpgrade.ClipSize:
                stat = player.stats.playerShooter.gunList[gunIndex].clipSize.ToString();
                return stat;

            case GunStatsUpgrade.ChargeDelay:
                float regen = 1 - player.stats.playerShooter.gunList[gunIndex].chargeDelay;
                stat = regen.ToString();
                return stat;

            case GunStatsUpgrade.GunAccuracy:
                stat = player.stats.playerShooter.gunList[gunIndex].accuracy.ToString();
                return stat;

            case GunStatsUpgrade.GunRange:
                float range = player.stats.playerShooter.gunList[gunIndex].bulletLifeTime *
                              player.stats.playerShooter.gunList[gunIndex].bulletSpeed;
                stat = range.ToString();
                return stat;
                
            case GunStatsUpgrade.Recoil:
                stat = player.stats.playerShooter.gunList[gunIndex].recoil.ToString();
                return stat;
        }

        return stat;
    }

    #endregion

    #region Upgrade Amounts

    public static string GetUpgAmount(PlayerStatsUpgrade playerStatsUpgrade, UpgradeSystem upgradeSystem)
    {
        string amount = "null";

        switch (playerStatsUpgrade)
        {
            case PlayerStatsUpgrade.MoveSpeed:
                amount = "+ " + MoveSpeedUpgrade.UpgAmount.ToString();
                return amount;

            case PlayerStatsUpgrade.StrafeSpeed:
                amount = "+ " + StrafeSpeedUpgrade.UpgAmount.ToString();
                return amount;

            case PlayerStatsUpgrade.SprintFactor:
                amount = "+ " + SprintFactorUpgrade.UpgAmount.ToString();
                return amount;

            case PlayerStatsUpgrade.PlayerReloadSpeed:
                amount = "+ " + ReloadSpeedUpgrade.UpgAmount.ToString();
                return amount;

            case PlayerStatsUpgrade.PlayerAccuracy:
                amount = "+ " + PlayerAccuracyUpgrade.UpgAmount.ToString();
                return amount;
            
            case PlayerStatsUpgrade.MeleeDamage:
                amount = "+ " + MeleeDamageUpgrade.GetUpgAmount(upgradeSystem).ToString();
                return amount;

            case PlayerStatsUpgrade.MeleeKnockback:
                amount = "+ " + KnockbackUpgrade.GetUpgAmount(upgradeSystem).ToString();
                return amount;

            case PlayerStatsUpgrade.StaminaMax:
                amount = "+ " + StaminaMaxUpgrade.UpgAmount.ToString();
                return amount;

            case PlayerStatsUpgrade.StaminaRegen:
                amount = "+ " + StaminaRegenUpgrade.UpgAmount.ToString();
                return amount;

            case PlayerStatsUpgrade.HealthMax:
                amount = "+ " + HealthMaxUpgrade.UpgAmount.ToString();
                return amount;

            case PlayerStatsUpgrade.HealthRegen:
                amount = "+ " + HealthRegenUpgrade.UpgAmount.ToString();
                return amount;
        }

        return amount;
    }

    public static string GetUpgAmount(ConsumablesUpgrade consumablesUpgrade)
    {
        string amount = "null";

        switch (consumablesUpgrade)
        {
            case ConsumablesUpgrade.PlayerHeal:
                amount = "+ " + PlayerHealUpgrade.HealAmount.ToString();
                return amount;

            case ConsumablesUpgrade.QCubeRepair:
                amount = "+ " + QCubeRepairUpgrade.RepairAmount.ToString();
                return amount;

            case ConsumablesUpgrade.GrenadeBuyAmmo:
                amount = "+ " + GrenadeBuyAmmoUpgrade.AmmoAmount.ToString();
                return amount;

            case ConsumablesUpgrade.SMGBuyAmmo:
                amount = "+ " + SMGBuyAmmoUpgrade.AmmoAmount.ToString();
                return amount;
        }

        return amount;
    }

    public static string GetUpgAmount(QCubeStatsUpgrade qCubeStatsUpgrade)
    {
        string amount = "null";

        switch (qCubeStatsUpgrade)
        {
            case QCubeStatsUpgrade.QCubeMaxHealth:
                amount = "+ " + QCubeMaxHealthUpgrade.UpgAmount.ToString();
                return amount;
        }

        return amount;
    }

    public static string GetUpgAmount(Player player, GunStatsUpgrade gunStatsUpgrade, int gunIndex, UpgradeSystem upgradeSystem)
    {
        string amount = "null";

        switch (gunStatsUpgrade)
        {
            case GunStatsUpgrade.GunDamage:
                amount = "+ " + GunDamageUpgrade.GetUpgAmount(player, gunIndex, upgradeSystem).ToString();
                return amount;

            case GunStatsUpgrade.GunKnockback:
                amount = "+ " + GunKnockbackUpgrade.GetUpgAmount(player, gunIndex).ToString();
                return amount;

            case GunStatsUpgrade.ShootSpeed:
                amount = "+ " + Mathf.Abs(ShootSpeedUpgrade.GetUpgAmount(player, gunIndex)).ToString();
                return amount;

            case GunStatsUpgrade.ClipSize:
                amount = "+ " + ClipSizeUpgrade.GetUpgAmount(player, gunIndex).ToString();
                return amount;

            case GunStatsUpgrade.ChargeDelay:
                amount = "+ " + Mathf.Abs(ChargeDelayUpgrade.GetUpgAmount(player, gunIndex)).ToString();
                return amount;

            case GunStatsUpgrade.GunAccuracy:
                amount = "+ " + Mathf.Abs(GunAccuracyUpgrade.GetUpgAmount(player, gunIndex)).ToString();
                return amount;

            case GunStatsUpgrade.GunRange:
                amount = "+ " + RangeUpgrade.GetUpgAmount(player, gunIndex).ToString();
                return amount;

            case GunStatsUpgrade.Recoil:
                amount = "+ " + Mathf.Abs(RecoilUpgrade.GetUpgAmount(player, gunIndex)).ToString();
                return amount;            
        }

        return amount;
    }

    #endregion
}
