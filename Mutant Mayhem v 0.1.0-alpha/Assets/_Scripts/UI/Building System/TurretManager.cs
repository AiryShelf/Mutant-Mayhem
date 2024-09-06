using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretManager : MonoBehaviour
{
    public static TurretManager Instance { get; set; }

    public List<TurretGunSO> _turretGunListSource = new List<TurretGunSO>();
    [HideInInspector] public List<TurretGunSO> turretGunList = new List<TurretGunSO>();
    public int numTurrets = 0;
    Dictionary<Vector3Int, GameObject> turrets = new Dictionary<Vector3Int, GameObject>();
    UpgradeManager upgradeManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void Initialize()
    {
        upgradeManager = FindObjectOfType<UpgradeManager>();

        turrets.Clear();
        turretGunList.Clear();
        numTurrets = 0;

        // Make working copy of TurretGunSOs
        foreach(TurretGunSO gun in _turretGunListSource)
        {
            TurretGunSO g = Instantiate(gun);
            turretGunList.Add(g);
        }
    }

    public void AddTurret(Vector3Int rootPos)
    {
        if (turrets.ContainsKey(rootPos))
        {
            Debug.LogError("Turret already exists at build location");
            return;
        }

        StartCoroutine(GetGameObject(rootPos));
        numTurrets++;
    }

    IEnumerator GetGameObject(Vector3Int rootPos)
    {
        yield return new WaitForFixedUpdate();

        GameObject obj = TileManager.StructureTilemap.GetInstantiatedObject(rootPos);
        turrets.Add(rootPos, obj);
        InitializeTurretStats(obj);
    }

    public void InitializeTurretStats(GameObject obj)
    {
        // Need to set stats to upgrade levels on build ***
    }

    public void RemoveTurret(Vector3Int rootPos)
    {
        if (!turrets.ContainsKey(rootPos))
        {
            Debug.LogError("Turret does not exists at 'remove' location");
            return;
        }

        turrets.Remove(rootPos);
    }

    #region Turret Upgrades

    public void UpgradeTurretGuns(GunType gunType, GunStatsUpgrade upgType, int level)
    {
        // Upgrade master list for stats
        UpgradeTurretGunList(gunType, upgType, level);
        
        // Apply upgrade to existing turrets
        foreach (KeyValuePair<Vector3Int, GameObject> kvp in turrets)
        {
            GameObject obj = kvp.Value;
            foreach (GunSO gun in obj.GetComponentInChildren<Shooter>().gunList)
            {
                if (gun.gunType != gunType)
                {
                    return;
                }

                if (gun is TurretGunSO turretGun)
                    UpgradeTurretGun(turretGun, upgType, level);
            }
            upgradeManager.PlayUpgradeEffectAt(obj.transform.position);
            Debug.Log("Upgraded a turret's gun's");
        }
    }

    void UpgradeTurretGunList(GunType gunType, GunStatsUpgrade upgType, int level)
    {
        foreach (TurretGunSO turretGun in turretGunList)
        {
            if (turretGun.gunType == gunType)
                UpgradeTurretGun(turretGun, upgType, level);
        }
    }
    
    void UpgradeTurretGun(TurretGunSO turretGun, GunStatsUpgrade upgType, int level)
    {
        switch (upgType)
        {
            case GunStatsUpgrade.GunDamage:
                turretGun.damage += turretGun.damageUpgFactor * ((level + 1) / (float)2);
                break;
            case GunStatsUpgrade.GunKnockback:
                turretGun.knockback += turretGun.knockbackUpgAmt;
                break;
            case GunStatsUpgrade.ShootSpeed:
                turretGun.shootSpeed += turretGun.shootSpeedUpgNegAmt;
                break;
            case GunStatsUpgrade.ClipSize:
                turretGun.clipSize += turretGun.clipSizeUpgAmt;
                break;
            case GunStatsUpgrade.ChargeSpeed:
                turretGun.chargeDelay += turretGun.chargeSpeedUpgNegAmt;
                break;
            case GunStatsUpgrade.GunAccuracy:
                turretGun.accuracy += turretGun.accuracyUpgNegAmt;
                break;
            case GunStatsUpgrade.GunRange:
                turretGun.bulletLifeTime += turretGun.bulletRangeUpgAmt;
                break;
            case GunStatsUpgrade.Recoil:
                // Depricated
                Debug.LogError("Tried to upgrade turret with depricated upgrade: recoil");
                break;
            case GunStatsUpgrade.TurretReloadSpeed:
                turretGun.reloadSpeed += turretGun.reloadSpeedUpgNegAmt;
                break;
        }
        Debug.Log("Upgraded Turret Gun");
    }

    public void UpgradeTurretStructures(StructureStatsUpgrade upgType)
    {
        // Upgrade Masterlist of turretGuns
        foreach (TurretGunSO turretGun in turretGunList)
        {
            UpgradeTurretStructure(turretGun, upgType);
        }

        // Apply upgrade to existing turrets
        foreach (KeyValuePair<Vector3Int, GameObject> kvp in turrets)
        {
            GameObject obj = kvp.Value;
            Turret turret = obj.GetComponentInChildren<Turret>();

            if (turret == null)
            {
                Debug.LogError("Could not find Turret script during upgrade");
                return;
            }
            
            foreach (TurretGunSO turretGun in turret.shooter.gunList)
                UpgradeTurretStructure(turretGun, upgType);

            turret.UpdateSensors();
            
            upgradeManager.PlayUpgradeEffectAt(obj.transform.position);
            Debug.Log("Upgraded a turret's structure");
        }
    }

    void UpgradeTurretStructure(TurretGunSO turretGun, StructureStatsUpgrade upgType)
    {
        switch (upgType)
        {
            case StructureStatsUpgrade.TurretRotSpeed:
                turretGun.rotationSpeed += turretGun.rotSpeedUpgAmt;
                Debug.Log("Upgraded turret rotation stat");
                break;
            case StructureStatsUpgrade.TurretSensors:
                turretGun.detectRange += turretGun.detectRangeUpgAmt;
                turretGun.expansionDelay += turretGun.expansionDelayUpgNegAmt;
                Debug.Log("Upgraded turret sensors stat");
                break;
        }
    }

    #endregion
}
