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
    }  

    public bool SpawnDroneInHangar(DroneType droneType, DroneHangar droneHangar)
    {
        string poolName = "";
        switch (droneType)
        {
            case DroneType.Builder:
                poolName = "Drone_Construction";
                if (droneHangar.GetDroneCount(DroneType.Builder) >= droneHangar.maxConstructionDrones)
                {
                    MessagePanel.PulseMessage("Drone Hangar is full", Color.red);
                    return false;
                }
                break;
            case DroneType.Attacker:
                poolName = "Drone_Attack";
                if (droneHangar.GetDroneCount(DroneType.Attacker) >= droneHangar.maxAttackDrones)
                {
                    MessagePanel.PulseMessage("Drone Hangar is full", Color.red);
                    return false;
                }
                break;
        }

        Drone newDrone = PoolManager.Instance.GetFromPool(poolName).GetComponent<Drone>();
        if (newDrone != null)
        {
            droneHangar.AddDrone(newDrone);
            newDrone.Initialize();
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

        PoolManager.Instance.ReturnToPool(drone.objectPoolName, drone.gameObject);
    }

    public void UpgradeDroneGuns(GunType gunType, GunStatsUpgrade upgType, int level)
    {
        UpgradeDroneGunList(gunType, upgType, level);

        // Apply upgrade to attack drones
        foreach (Drone drone in activeAttackDrones)
        {
            Shooter shooter = drone.GetComponent<Shooter>();
            foreach (GunSO gun in shooter.gunList)
            {
                if (gun.gunType != gunType)
                {
                    return;
                }

                if (gun is TurretGunSO droneGun)
                {
                    UpgradeDroneGun(droneGun, upgType, level);   
                    drone.RefreshStats();             
                }
            }
            // Refresh stats in shooter
            shooter.SwitchGuns(shooter.currentGunIndex);
            
            UpgradeManager.Instance.upgradeEffects.PlayStructureUpgradeEffectAt(drone.transform.position);
            //Debug.Log("Finished upgrading a turret's guns");
        }

        // Apply upgrade to construction drones
        foreach(Drone drone in activeConstructionDrones)
        {
            //drone.buildSpeed += 
        }
    }

    void UpgradeDroneGunList(GunType gunType, GunStatsUpgrade upgType, int level)
    {
        foreach (TurretGunSO droneGun in droneGunList)
        {
            if (droneGun.gunType == gunType)
                UpgradeDroneGun(droneGun, upgType, level);
        }
    }

    void UpgradeDroneGun(TurretGunSO droneGun, GunStatsUpgrade upgType, int level)
    {
        switch (upgType)
        {
            case GunStatsUpgrade.GunDamage:
                droneGun.damage += droneGun.damageUpgFactor * (level + 1);
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
        //Debug.Log("Upgraded Turret Gun");
    }
}
