using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneManager : MonoBehaviour
{
    public List<Drone> _droneListSource;
    public List<bool> unlockedDrones;
    public List<Drone> allActiveDrones;
    public List<Drone> activeConstructionDrones;
    public List<Drone> activeAttackDrones;

    public List<TurretGunSO> _droneGunListSource = new List<TurretGunSO>();
    [Header("Dynamic Vars:")]
    public List<TurretGunSO> droneGunList = new List<TurretGunSO>();

    public static DroneManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        droneGunList.Clear();
        foreach(TurretGunSO gun in _droneGunListSource)
        {
            TurretGunSO g = Instantiate(gun);
            droneGunList.Add(g);
        }
    }  

    public bool SpawnDroneInHangar(DroneType droneType, DroneHangar droneHangar)
    {
        string poolName = "";
        TurretGunSO droneGun = null;
        switch (droneType)
        {
            case DroneType.Builder:
                if (droneHangar.GetDroneCount(DroneType.Builder) >= droneHangar.maxConstructionDrones)
                {
                    MessagePanel.PulseMessage("Drone Hangar is full", Color.red);
                    return false;
                }
                poolName = "Drone_Construction";
                droneGun = droneGunList[0];
                break;
            case DroneType.Attacker:
                if (droneHangar.GetDroneCount(DroneType.Attacker) >= droneHangar.maxAttackDrones)
                {
                    MessagePanel.PulseMessage("Drone Hangar is full", Color.red);
                    return false;
                }
                poolName = "Drone_Attack";
                droneGun = droneGunList[1];
                break;
        }

        
        Drone newDrone = PoolManager.Instance.GetFromPool(poolName).GetComponent<Drone>();
        if (newDrone != null)
        {
            newDrone.powerConsumer.AddConsumer();
            newDrone.Initialize(droneGun);
            droneHangar.AddDrone(newDrone);
            allActiveDrones.Add(newDrone);

            if (newDrone is AttackDrone)
                activeAttackDrones.Add(newDrone);
            else
                activeConstructionDrones.Add(newDrone);
            return true;
        }

        return false;
    }

    public void RemoveDrone(Drone drone)
    {
        allActiveDrones.Remove(drone);

        switch (drone.droneType)
        {
            case DroneType.Builder:
                activeConstructionDrones.Remove(drone);
                break;
            case DroneType.Attacker:
                activeAttackDrones.Remove(drone);
                break;
        }

        drone.powerConsumer.RemoveConsumer();
        PoolManager.Instance.ReturnToPool(drone.objectPoolName, drone.gameObject);
    }

    #region Upgrades

    public void UpgradeDroneGuns(GunType gunType, GunStatsUpgrade upgType, int level)
    {
        //Debug.Log("UpgradeDroneGuns called");
        UpgradeDroneGunList(gunType, upgType, level);
    }

    void UpgradeDroneGunList(GunType gunType, GunStatsUpgrade upgType, int level)
    {
        //Debug.Log("UpgradeDroneGunList called");
        foreach (TurretGunSO droneGunBase in droneGunList)
        {
            if (droneGunBase.gunType == gunType)
                UpgradeDroneGun(droneGunBase, upgType, level);
        }
    }

    void UpgradeDroneGun(TurretGunSO droneGun, GunStatsUpgrade upgType, int level)
    {
        float damageAmount = 0;
        switch (droneGun.gunType)
        {
            case GunType.Laser:
                damageAmount = droneGun.damageUpgFactor * (level + 1) * PlanetManager.Instance.statMultipliers[PlanetStatModifier.LaserDamage];
                break;
            case GunType.Bullet:
                damageAmount = droneGun.damageUpgFactor * (level + 1) * PlanetManager.Instance.statMultipliers[PlanetStatModifier.BulletDamage];
                break;
            case GunType.RepairGun:
                damageAmount = droneGun.damageUpgFactor * PlanetManager.Instance.statMultipliers[PlanetStatModifier.RepairGunDamage];
                break;
        }

        //Debug.Log($"UpgradeDroneGun: {droneGun}, UpgType: {upgType}, level: {level}");
        switch (upgType)
        {
            case GunStatsUpgrade.GunDamage:
                droneGun.damage += damageAmount;
                break;
            case GunStatsUpgrade.GunKnockback:
                droneGun.knockback += droneGun.knockbackUpgAmt;
                break;
            case GunStatsUpgrade.ShootSpeed:
                droneGun.shootSpeed += droneGun.shootSpeedUpgNegAmt;
                break;
            case GunStatsUpgrade.ClipSize:
                droneGun.clipSize += droneGun.clipSizeUpgAmt;
                break;
            case GunStatsUpgrade.ChargeSpeed:
                droneGun.chargeDelay += droneGun.chargeSpeedUpgNegAmt;
                break;
            case GunStatsUpgrade.GunAccuracy:
                droneGun.accuracy += droneGun.accuracyUpgNegAmt;
                break;
            case GunStatsUpgrade.GunRange:
                droneGun.bulletLifeTime += droneGun.bulletRangeUpgAmt;
                break;
            case GunStatsUpgrade.Recoil:
                // Depricated
                Debug.LogError("Tried to upgrade turret with depricated upgrade: recoil");
                break;
            case GunStatsUpgrade.TurretReloadSpeed:
                droneGun.reloadSpeed += droneGun.reloadSpeedUpgNegAmt;
                break;
        }
    }

    #endregion
}
