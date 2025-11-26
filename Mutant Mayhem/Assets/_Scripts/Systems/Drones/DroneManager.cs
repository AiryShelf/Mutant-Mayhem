using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneManager : MonoBehaviour
{
    public static DroneManager Instance;

    public static Action<int> OnDroneCountChanged;

    [Header("Base Stats and Upgrade Multipliers:")]
    public float droneSpeedMult = 1;
    public float droneSpeedUpgMult = 0.05f;
    public float droneRotationSpeedMult = 1;
    public float droneRotationSpeedUpgMult = 0.025f;
    public float droneHealthMult = 1;
    public float droneHealthUpgMult = 0.1f;
    public float droneEnergyMult = 1;
    public float droneEnergyUpgMult = 0.05f;
    public float droneHangarRange = 10;
    public float droneHangarRangeUpgAmount = 0.5f;
    public int droneHangarRepairSpeed = 5; // Repair per second spread between docked drones
    public int droneHangarRepairSpeedUpgAmount = 1;
    public int droneHangarRechargeSpeed = 5; // Energy recharge per second on one drone at a time
    public int droneHangarRechargeSpeedUpgAmount = 1;

    [Header("Drone Lists:")]
    public List<Drone> allActiveDrones;
    public List<Drone> activeConstructionDrones;
    public List<Drone> activeAttackDrones;

    public List<TurretGunSO> _droneGunListSource = new List<TurretGunSO>();
    [Header("Dynamic Vars:")]
    public List<TurretGunSO> droneGunList = new List<TurretGunSO>();
    public List<DroneContainer> droneContainers = new List<DroneContainer>();

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

    void Start()
    {
        // Apply planet stat modifiers
        droneHangarRange *= PlanetManager.Instance.statMultipliers[PlanetStatModifier.SupportSensors];
        droneHangarRangeUpgAmount *= PlanetManager.Instance.statMultipliers[PlanetStatModifier.SupportSensors];
    }

    public bool SpawnDroneInHangar(DroneType droneType, DroneContainer droneContainer)
    {
        string poolName = "";
        TurretGunSO droneGun = null;
        switch (droneType)
        {
            case DroneType.Builder:
                if (droneContainer.GetTotalDroneCount() >= droneContainer.maxDrones)
                {
                    MessageBanner.PulseMessage("Drone Hangar is full", Color.red);
                    return false;
                }
                poolName = "Drone_Construction";
                droneGun = droneGunList[0];
                break;
            case DroneType.Attacker:
                if (droneContainer.GetTotalDroneCount() >= droneContainer.maxDrones)
                {
                    MessageBanner.PulseMessage("Drone Hangar is full", Color.red);
                    return false;
                }
                poolName = "Drone_Attack";
                droneGun = droneGunList[1];
                break;
        }

        
        Drone newDrone = PoolManager.Instance.GetFromPool(poolName).GetComponent<Drone>();
        if (newDrone != null)
        {
            //newDrone.powerConsumer.AddConsumer();
            newDrone.Initialize(droneGun);
            droneContainer.AddDrone(newDrone);
            allActiveDrones.Add(newDrone);

            if (newDrone is AttackDrone)
                activeAttackDrones.Add(newDrone);
            else
                activeConstructionDrones.Add(newDrone);

            OnDroneCountChanged?.Invoke(allActiveDrones.Count);

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

        OnDroneCountChanged?.Invoke(allActiveDrones.Count);

        //drone.powerConsumer.RemoveConsumer();
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

    public void UpgradeDroneSpeed(int level)
    {
        droneSpeedMult += droneSpeedUpgMult;
        foreach(Drone d in allActiveDrones)
        {
            d.RefreshStats();
        }
    }

    public void UpgradeDroneHealth(int level)
    {
        droneHealthMult += droneHealthUpgMult;
        foreach(Drone d in allActiveDrones)
        {
            d.RefreshStats();
        }
    }

    public void UpgradeDroneEnergy(int level)
    {
        droneEnergyMult += droneEnergyUpgMult;
        foreach(Drone d in allActiveDrones)
        {
            d.RefreshStats();
        }
    }

    public void UpgradeDroneHangarRange(int level)
    {
        droneHangarRange += droneHangarRangeUpgAmount;
    }

    public void UpgradeDroneHangarRepairSpeed(int level)
    {
        droneHangarRepairSpeed += droneHangarRepairSpeedUpgAmount;
    }

    public void UpgradeDroneHangarRechargeSpeed(int level)
    {
        droneHangarRechargeSpeed += droneHangarRechargeSpeedUpgAmount;
    }

    public bool SellDrone(DroneType droneType, DroneContainer droneContainer)
    {
        Drone droneToSell = droneContainer.GetDroneToSell(droneType);
        if (droneToSell != null)
        {
            Debug.Log("Selling drone of type: " + droneType);
            droneToSell.Die();
            return true;
        }

        MessageBanner.PulseMessage("No drones of that type are docked!", Color.red);
        return false;
    }

    #endregion
}
