using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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
                stat = player.stats.moveSpeed.ToString("#0.0");
                return stat;
            case PlayerStatsUpgrade.StrafeSpeed:
                stat = player.stats.strafeSpeed.ToString("#0.0");
                return stat;
            case PlayerStatsUpgrade.SprintFactor:
                stat = player.stats.sprintFactor.ToString("#0.00");
                return stat;
            case PlayerStatsUpgrade.PlayerReloadSpeed:
                stat = player.stats.reloadFactor.ToString("#0.0");
                return stat;
            case PlayerStatsUpgrade.WeaponHandling:
                stat = player.stats.accuracyHoningSpeed.ToString("#0.00");
                return stat;
            case PlayerStatsUpgrade.MeleeDamage:
                stat = player.stats.meleeDamage.ToString("#0.00");
                return stat;
            case PlayerStatsUpgrade.MeleeKnockback:
                stat = player.stats.knockback.ToString("#0.0");
                return stat;
            case PlayerStatsUpgrade.StaminaMax:
                stat = player.stats.staminaMax.ToString("#0");
                return stat;
            case PlayerStatsUpgrade.StaminaRegen:
                stat = player.stats.staminaRegen.ToString("#0.0");
                return stat;
            case PlayerStatsUpgrade.HealthMax:
                stat = player.stats.playerHealthScript.GetMaxHealth().ToString("#0");
                return stat;
            case PlayerStatsUpgrade.HealthRegen:
                stat = player.stats.playerHealthScript.healthRegenPerSec.ToString("#0.00");
                return stat;
            case PlayerStatsUpgrade.CriticalHitChance:
                stat = player.stats.criticalHitChanceMult.ToString("#0.00");
                return stat;
            case PlayerStatsUpgrade.CriticalHitDamage:
                stat = player.stats.criticalHitDamageMult.ToString("#0.00");
                return stat;
        }

        return stat;
    }

    public static string GetStatValue(Player player, ConsumablesUpgrade consumablesUpgrade)
    {
        string stat = "";

        switch(consumablesUpgrade)
        {
            case ConsumablesUpgrade.PlayerHeal:
                stat = player.stats.playerHealthScript.GetHealth().ToString("#0");
                return stat;
            case ConsumablesUpgrade.QCubeRepair:
                stat = player.stats.structureStats.cubeHealthScript.GetHealth().ToString("#0");
                return stat;
            case ConsumablesUpgrade.GrenadeBuyAmmo:
                stat = player.stats.grenadeAmmo.ToString("#0");
                return stat;
            case ConsumablesUpgrade.SMGBuyAmmo:
                stat = (player.stats.playerShooter.gunsAmmo[1] +
                        player.stats.playerShooter.gunsAmmoInClips[1]).ToString("#0");
                return stat;
            case ConsumablesUpgrade.BuyConstructionDrone:
                stat = DroneManager.Instance.activeConstructionDrones.Count.ToString();
                return stat;
            case ConsumablesUpgrade.BuyAttackDrone:
                stat = DroneManager.Instance.activeAttackDrones.Count.ToString();
                return stat;
        }

        return stat;
    }

    public static string GetStatValue(Player player, StructureStatsUpgrade structureStatsUpgrade)
    {
        string stat = "null";

        switch(structureStatsUpgrade)
        {
            case StructureStatsUpgrade.QCubeMaxHealth:
                stat = player.stats.structureStats.cubeHealthScript.GetMaxHealth().ToString("#0");
                return stat;
            case StructureStatsUpgrade.StructureMaxHealth:
                stat = player.stats.structureStats.structureMaxHealthMult.ToString("#0.00");
                return stat;
            case StructureStatsUpgrade.MaxTurrets:
                stat = player.stats.structureStats.maxTurrets.ToString("#0");
                return stat;
            case StructureStatsUpgrade.TurretRotSpeed:
                stat = TurretManager.Instance.turretGunList[0].rotationSpeed.ToString("#0");
                return stat;
            case StructureStatsUpgrade.TurretSensors:
                stat = TurretManager.Instance.turretGunList[0].detectRange.ToString("#0.0");
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
                stat = Mathf.Abs(player.stats.playerShooter.gunList[gunIndex].damage).ToString("#0.00");
                return stat;
            case GunStatsUpgrade.GunKnockback:
                stat = player.stats.playerShooter.gunList[gunIndex].knockback.ToString("#0.0");
                return stat;
            case GunStatsUpgrade.ShootSpeed:
                float speed = player.stats.playerShooter.gunList[gunIndex].shootSpeed;
                stat = speed.ToString("#0.000");
                return stat;
            case GunStatsUpgrade.ClipSize:
                stat = player.stats.playerShooter.gunList[gunIndex].clipSize.ToString("#0");
                return stat;
            case GunStatsUpgrade.ChargeSpeed:
                float regen = player.stats.playerShooter.gunList[gunIndex].chargeDelay;
                stat = regen.ToString("#0.00");
                return stat;
            case GunStatsUpgrade.GunAccuracy:
                stat = (player.stats.playerShooter._gunListSource[gunIndex].accuracy -
                       player.stats.playerShooter.gunList[gunIndex].accuracy).ToString("#0.00");
                return stat;
            case GunStatsUpgrade.GunRange:
                float range = player.stats.playerShooter.gunList[gunIndex].bulletLifeTime *
                              player.stats.playerShooter.gunList[gunIndex].bulletSpeed;
                stat = range.ToString("#0.00");
                return stat;
            case GunStatsUpgrade.Recoil:
                stat = player.stats.playerShooter.gunList[gunIndex].kickback.ToString("#0.0");
                return stat;
            case GunStatsUpgrade.TurretReloadSpeed:
                stat = TurretManager.Instance.turretGunList[1].reloadSpeed.ToString("#0.00");
                return stat;
        }

        return stat;
    }

    #endregion

    #region Upgrade Amounts

    // PlayerStats
    public static string GetUpgAmount(PlayerStatsUpgrade playerStatsUpgrade, UpgradeManager upgradeManager)
    {
        string amount = "null";

        switch (playerStatsUpgrade)
        {
            case PlayerStatsUpgrade.MoveSpeed:
                amount = "+" + MoveSpeedUpgrade.GetUpgAmount().ToString("#0.00");
                return amount;
            case PlayerStatsUpgrade.StrafeSpeed:
                amount = "+" + StrafeSpeedUpgrade.UpgAmount.ToString("#0.00");
                return amount;
            case PlayerStatsUpgrade.SprintFactor:
                amount = "+" + SprintFactorUpgrade.UpgAmount.ToString("#0.00");
                return amount;
            case PlayerStatsUpgrade.PlayerReloadSpeed:
                amount = "+" + PlayerReloadSpeedUpgrade.UpgAmount.ToString("#0.0");
                return amount;
            case PlayerStatsUpgrade.WeaponHandling:
                amount = "+" + WeaponHandlingUpgrade.UpgAmount.ToString("#0.00");
                return amount;
            case PlayerStatsUpgrade.MeleeDamage:
                amount = "+" + MeleeDamageUpgrade.GetUpgAmount(upgradeManager).ToString("#0.00");
                return amount;
            case PlayerStatsUpgrade.MeleeKnockback:
                amount = "+" + KnockbackUpgrade.GetUpgAmount(upgradeManager).ToString("#0.0");
                return amount;
            case PlayerStatsUpgrade.StaminaMax:
                amount = "+" + StaminaMaxUpgrade.UpgAmount.ToString("#0");
                return amount;
            case PlayerStatsUpgrade.StaminaRegen:
                amount = "+" + StaminaRegenUpgrade.UpgAmount.ToString("#0.0");
                return amount;
            case PlayerStatsUpgrade.HealthMax:
                amount = "+" + HealthMaxUpgrade.GetUpgAmount().ToString("#0");
                return amount;
            case PlayerStatsUpgrade.HealthRegen:
                amount = "+" + HealthRegenUpgrade.UpgAmount.ToString("#0.0");
                return amount;
            case PlayerStatsUpgrade.CriticalHitChance:
                amount = "+" + CriticalHitChanceUpgrade.UpgAmount.ToString("#0.00");
                return amount;
            case PlayerStatsUpgrade.CriticalHitDamage:
                amount = "+" + CriticalHitDamageUpgrade.UpgAmount.ToString("#0.00");
                return amount;
        }

        return amount;
    }

    // Consumables
    public static string GetUpgAmount(Player player, ConsumablesUpgrade consumablesUpgrade)
    {
        string amount = "null";

        switch (consumablesUpgrade)
        {
            case ConsumablesUpgrade.PlayerHeal:
                amount = "+" + PlayerHealUpgrade.GetUpgAmount(player.stats.playerHealthScript).ToString();
                return amount;
            case ConsumablesUpgrade.QCubeRepair:
                amount = "+" + QCubeRepairUpgrade.GetUpgAmount(player.stats.structureStats.cubeHealthScript).ToString();
                return amount;
            case ConsumablesUpgrade.GrenadeBuyAmmo:
                amount = "+" + GrenadeBuyAmmoUpgrade.GetUpgAmount().ToString();
                return amount;
            case ConsumablesUpgrade.SMGBuyAmmo:
                amount = "+" + SMGBuyAmmoUpgrade.AmmoAmount.ToString();
                return amount;
            case ConsumablesUpgrade.BuyConstructionDrone:
                amount = "+" + BuyConstructionDroneUpgrade.Amount.ToString();
                return amount;
            case ConsumablesUpgrade.BuyAttackDrone:
                amount = "+" + BuyAttackDroneUpgrade.Amount.ToString();
                return amount;
        }

        return amount;
    }

    // StructureStats
    public static string GetUpgAmount(StructureStatsUpgrade structureStatsUpgrade)
    {
        string amount = "null";

        switch (structureStatsUpgrade)
        {
            case StructureStatsUpgrade.QCubeMaxHealth:
                amount = "+" + QCubeMaxHealthUpgrade.UpgAmount.ToString("#0");
                return amount;
            case StructureStatsUpgrade.StructureMaxHealth:
                amount = "+" + StructureMaxHealthUpgrade.UpgAmount.ToString("#0.00");
                return amount;
            case StructureStatsUpgrade.MaxTurrets:
                amount = "+" + MaxTurretsUpgrade.UpgAmount.ToString("#0");
                return amount;
            case StructureStatsUpgrade.TurretRotSpeed:
                amount = "+" + TurretRotSpeedUpgrade.UpgAmount.ToString("#0");
                return amount;
            case StructureStatsUpgrade.TurretSensors:
                amount = "+" + TurretSensorsUpgrade.GetUpgAmount().ToString("#0.0");
                return amount;
        }

        return amount;
    }

    // GunStats
    public static string GetUpgAmount(Player player, GunStatsUpgrade gunStatsUpgrade, int gunIndex, UpgradeManager upgradeManager)
    {
        string amount = "null";

        switch (gunStatsUpgrade)
        {
            case GunStatsUpgrade.GunDamage:
                amount = "+" + Mathf.Abs(GunDamageUpgrade.GetUpgAmount(player, gunIndex, upgradeManager)).ToString("#0.00");
                return amount;
            case GunStatsUpgrade.GunKnockback:
                amount = "+" + GunKnockbackUpgrade.GetUpgAmount(player, gunIndex).ToString("#0.0");
                return amount;
            case GunStatsUpgrade.ShootSpeed:
                amount = "-" + Mathf.Abs(ShootSpeedUpgrade.GetUpgAmount(player, gunIndex)).ToString("#0.000");
                return amount;
            case GunStatsUpgrade.ClipSize:
                amount = "+" + ClipSizeUpgrade.GetUpgAmount(player, gunIndex).ToString();
                return amount;
            case GunStatsUpgrade.ChargeSpeed:
                amount = "-" + Mathf.Abs(ChargeDelayUpgrade.GetUpgAmount(player, gunIndex)).ToString("#0.00");
                return amount;
            case GunStatsUpgrade.GunAccuracy:
                amount = "+" + Mathf.Abs(GunAccuracyUpgrade.GetUpgAmount(player, gunIndex)).ToString("#0.00");
                return amount;
            case GunStatsUpgrade.GunRange:
                amount = "+" + (RangeUpgrade.GetUpgAmount(player, gunIndex) *
                         player.stats.playerShooter.gunList[gunIndex].bulletSpeed).ToString("#0.0");
                return amount;
            case GunStatsUpgrade.Recoil:
                amount = "-" + Mathf.Abs(RecoilUpgrade.GetUpgAmount(player, gunIndex)).ToString("#0.0");
                return amount;     
            case GunStatsUpgrade.TurretReloadSpeed:
                amount = "-" + Mathf.Abs(TurretReloadSpeedUpgrade.GetUpgAmount(player, gunIndex)).ToString("#0.00");
                return amount;         
        }   

        return amount;
    }

    #endregion

    #region Upgrade Costs

    // Consumables
    public static int GetUpgCost(Player player, ConsumablesUpgrade consumablesUpgrade, UpgradeManager upgradeManager)
    {
        int cost = 0;

        switch (consumablesUpgrade)
        {
            case ConsumablesUpgrade.PlayerHeal:
                cost = PlayerHealUpgrade.GetCost(player, upgradeManager.consumablesUpgBaseCosts[consumablesUpgrade]);
                return cost;
            case ConsumablesUpgrade.QCubeRepair:
                cost = QCubeRepairUpgrade.GetCost(player, upgradeManager.consumablesUpgBaseCosts[consumablesUpgrade]);
                return cost;
            case ConsumablesUpgrade.GrenadeBuyAmmo:
                cost = GrenadeBuyAmmoUpgrade.GetCost(player, upgradeManager.consumablesUpgBaseCosts[consumablesUpgrade]);
                return cost;
            case ConsumablesUpgrade.SMGBuyAmmo:
                cost = SMGBuyAmmoUpgrade.GetCost(player, upgradeManager.consumablesUpgBaseCosts[consumablesUpgrade]);
                return cost;
            case ConsumablesUpgrade.BuyConstructionDrone:
                cost = BuyConstructionDroneUpgrade.GetCost(player, upgradeManager.consumablesUpgBaseCosts[consumablesUpgrade]);
                return cost;
            case ConsumablesUpgrade.BuyAttackDrone:
                cost = BuyAttackDroneUpgrade.GetCost(player, upgradeManager.consumablesUpgBaseCosts[consumablesUpgrade]);
                return cost;
        }

        return Mathf.CeilToInt(cost * upgradeManager.consumablesCostMult);
    }

    #endregion
}
