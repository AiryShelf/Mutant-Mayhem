using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretManager : MonoBehaviour
{
    public static TurretManager Instance { get; private set; }
    
    [SerializeField] List<TurretGunSO> _turretGunListSource = new List<TurretGunSO>();
    
    [Header("Dynamic vars, don't set here")]
    public int currentNumTurrets = 0;
    public List<TurretGunSO> turretGunList = new List<TurretGunSO>();
    Dictionary<Vector3Int, GameObject> turrets = new Dictionary<Vector3Int, GameObject>();
    Player player;

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

    public void Initialize(Player player)
    {
        this.player = player;
        turrets.Clear();
        turretGunList.Clear();
        currentNumTurrets = 0;

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
        currentNumTurrets++;
    }

    IEnumerator GetGameObject(Vector3Int rootPos)
    {
        yield return new WaitForFixedUpdate();

        GameObject obj = TileManager.StructureTilemap.GetInstantiatedObject(rootPos);
        turrets.Add(rootPos, obj);

        // Link turret with player for ammo use
        Turret turret = obj.GetComponentInChildren<Turret>();
        if (turret != null)
            turret.InitializeTurret(player);
        else
            Debug.LogError("TurretManager: Couldn't find Turret component on turret");
    }

    public void RemoveTurret(Vector3Int rootPos)
    {
        if (!turrets.ContainsKey(rootPos))
        {
            Debug.LogError("Turret does not exists at 'remove' location");
            return;
        }

        turrets.Remove(rootPos);
        currentNumTurrets--;
    }

    #region Turret Upgrades

    public void UpgradeTurretGuns(GunType gunType, GunStatsUpgrade upgType, int level)
    {
        // Upgrade working list for stats
        UpgradeTurretGunList(gunType, upgType, level);
        
        // Apply upgrade to existing turrets
        foreach (KeyValuePair<Vector3Int, GameObject> kvp in turrets)
        {
            GameObject obj = kvp.Value;
            Shooter shooter = obj.GetComponentInChildren<Shooter>();
            
            // Refresh stats in shooter
            shooter.SwitchGuns(shooter.currentGunIndex);
            
            UpgradeManager.Instance.upgradeEffects.PlayStructureUpgradeEffectAt(obj.transform.position);
            //Debug.Log("Finished upgrading a turret's guns");
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
        float damageAmount = 0;
        switch (turretGun.gunType)
        {
            case GunType.Laser:
                damageAmount = turretGun.damageUpgFactor * (level + 1) * PlanetManager.Instance.statMultipliers[PlanetStatModifier.LaserDamage];
                break;
            case GunType.Bullet:
                damageAmount = turretGun.damageUpgFactor * (level + 1) * PlanetManager.Instance.statMultipliers[PlanetStatModifier.BulletDamage];
                break;
            case GunType.RepairGun:
                damageAmount = turretGun.damageUpgFactor * PlanetManager.Instance.statMultipliers[PlanetStatModifier.RepairGunDamage];
                break;
        }

        switch (upgType)
        {
            case GunStatsUpgrade.GunDamage:
                turretGun.damage += damageAmount;
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
        //Debug.Log("Upgraded Turret Gun");
    }

    public void UpgradeTurretStructures(StructureStatsUpgrade upgType)
    {
        // Upgrade Masterlist of turretGuns
        foreach (TurretGunSO turretGun in turretGunList)
        {
            UpgradeTurretStructure(turretGun);
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
                UpgradeTurretStructure(turretGun);

            turret.UpdateStructure();
            
            UpgradeManager.Instance.upgradeEffects.PlayStructureUpgradeEffectAt(obj.transform.position);
        }
    }

    void UpgradeTurretStructure(TurretGunSO turretGun)
    {
        turretGun.rotationSpeed += turretGun.rotSpeedUpgAmt;
        //Debug.Log("Upgraded turret rotation stat");
        turretGun.detectRange += turretGun.detectRangeUpgAmt * 
                                 PlanetManager.Instance.statMultipliers[PlanetStatModifier.SensorsRange];
        turretGun.expansionDelay += turretGun.expansionDelayUpgNegAmt * 
                                    PlanetManager.Instance.statMultipliers[PlanetStatModifier.SensorsRange];
        //Debug.Log("Upgraded turret sensors stat");
    }

    #endregion
}
