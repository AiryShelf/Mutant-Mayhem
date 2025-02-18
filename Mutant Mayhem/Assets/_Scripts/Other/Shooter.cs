using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Shooter : MonoBehaviour
{
    public List<GunSO> _gunListSource;
    
    [SerializeField] protected Transform gunTrans;
    public Transform muzzleTrans;
    [SerializeField] protected Transform casingEjectorTrans;
    [SerializeField] protected Transform clipEjectorTrans;
    [SerializeField] protected LayerMask elevatedHitLayers;
    [SerializeField] protected GunRecoil gunRecoil;
    public List<int> gunsAmmoInClips = new List<int>();

    [Header("For Non-Player Dynamic Accuracy:")]
    public float accuracyHoningSpeed = 4;
    

    [Header("Dynamic vars, don't set here")]
    public List<GunSO> gunList;
    public int currentGunIndex = 0;
    public GunSO currentGunSO;
    float shootSpeed = 1.0f;
    public float TurretReloadTime = 2.0f; 
    public bool isReloading;
    public bool isElevated;
    public bool hasTarget;
    protected GameObject muzzleFlash;
    public GunSights gunSights;
    protected Coroutine reloadRoutine;
    protected int clipSize;
    float fireTimer;
    protected float reloadTimer;
    Dictionary<int, Coroutine> chargeCoroutines = new Dictionary<int, Coroutine>();
    float laserDamageMult = 1f;
    float bulletDamageMult = 1f;
    protected CriticalHit criticalHit;
    public float currentAccuracy;
    protected PlayerShooter playerShooter;
    protected TurretShooter turretShooter;

    
    protected virtual void Awake()
    {
        /*
        CopyGunList();
        ApplyPlanetProperties();

        // Initialize first gun
        SwitchGuns(0);
        gunsAmmoInClips[0] = 0;
        isReloading = true;

        criticalHit = GetComponent<CriticalHit>();
        */
    }
    

    protected virtual void Start()
    {
        CopyGunList();
        ApplyPlanetProperties();

        // Initialize first gun
        SwitchGuns(0);
        gunsAmmoInClips[0] = 0;
        isReloading = true;

        criticalHit = GetComponent<CriticalHit>();
        StartChargingGuns();
    }

    protected virtual void Update()
    {
        if (isReloading)
        {
            if (reloadRoutine == null)
                Reload();
            return;
        }

        if (!hasTarget)
            return;

        // Start reloading after firing a certain number of shots
        if (ShouldReload())
        {
            isReloading = true;
            return;
        }

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0 && gunsAmmoInClips[currentGunIndex] > 0)
        {
            Fire();
            fireTimer = shootSpeed;
        }
    }

    #region Initialize

    protected void CopyGunList()
    {
        // Make a working copy of the gun list
        foreach (GunSO gun in _gunListSource)
        {
            if (gun != null)
            {
                GunSO g = Instantiate(gun);
                gunList.Add(g);
            }
        }
    }

    protected void ApplyPlanetProperties()
    {
        Dictionary<PlanetStatModifier, float> statMultipliers = PlanetManager.Instance.statMultipliers;
        foreach (GunSO gun in gunList)
        {
            TurretGunSO turretGun = gun as TurretGunSO;
            switch (gun.gunType)
            {
                case GunType.Laser:
                    gun.damage *= statMultipliers[PlanetStatModifier.LaserDamage];
                    gun.bulletLifeTime *= statMultipliers[PlanetStatModifier.LaserRange];
                    
                    if (turretGun != null)
                    {
                        turretGun.detectRange *= statMultipliers[PlanetStatModifier.TurretSensors];
                        turretGun.expansionDelay *= statMultipliers[PlanetStatModifier.TurretSensors];
                    }
                    break;
                case GunType.Bullet:
                    gun.damage *= statMultipliers[PlanetStatModifier.BulletDamage];
                    gun.bulletLifeTime *= statMultipliers[PlanetStatModifier.BulletRange];

                    if (turretGun != null)
                    {
                        turretGun.detectRange *= statMultipliers[PlanetStatModifier.TurretSensors];
                        turretGun.expansionDelay *= statMultipliers[PlanetStatModifier.TurretSensors];
                    }
                    break;
                case GunType.RepairGun:
                    gun.damage *= statMultipliers[PlanetStatModifier.RepairGunDamage];

                    if (turretGun != null)
                    {
                        turretGun.detectRange *= statMultipliers[PlanetStatModifier.TurretSensors];
                        turretGun.expansionDelay *= statMultipliers[PlanetStatModifier.TurretSensors];
                    }
                    break;
            }
        }
    }

    #endregion

    #region Fire

    protected virtual void Fire()
    {
        // Use ammo
        gunsAmmoInClips[currentGunIndex]--;

        // Create bullet and casing
        GameObject bulletObj = PoolManager.Instance.GetFromPool(currentGunSO.bulletPoolName);
        bulletObj.transform.position = muzzleTrans.position;
        bulletObj.transform.rotation = muzzleTrans.rotation;

        if (!string.IsNullOrEmpty(currentGunSO.FXManagerCasingMethodName))
        {
            ParticleManager.Instance.PlayCasingEffectByName(currentGunSO.FXManagerCasingMethodName, 
                                                            casingEjectorTrans,
                                                            gunTrans.rotation, isElevated);
            //Debug.Log("Shooter attempted to play casing effect");
        }

        Bullet bullet = bulletObj.GetComponent<Bullet>();

        // Check if elevated for shooting over walls
        if (isElevated)
        {
            bullet.hitLayers = elevatedHitLayers;
        }
        
        // Apply stats and effects
        float damage = currentGunSO.damage;
        switch (bullet.gunType)
        {
            case GunType.Laser:
                damage *= laserDamageMult;
                break;
            case GunType.Bullet:
                damage *= bulletDamageMult;
                break;
        }
        bullet.damage = damage;
        bullet.damageVariance = currentGunSO.damageVariance;
        bullet.origin = this.transform;
        bullet.knockback = currentGunSO.knockback;
        bullet.destroyTime = currentGunSO.bulletLifeTime;
        bullet.objectPoolName = currentGunSO.bulletPoolName;
        bullet.criticalHit = criticalHit;

        Vector2 dir;
        if (playerShooter)
        {
            dir = ApplyAccuracy(muzzleTrans.right, currentAccuracy);
            bullet.critChanceMult = playerShooter.playerStats.criticalHitChanceMult;
            bullet.critDamageMult = playerShooter.playerStats.criticalHitDamageMult;
        }
        else if (turretShooter)
            dir = ApplyAccuracy(muzzleTrans.right, currentGunSO.accuracy * 1.5f);
        else
            dir = ApplyAccuracy(muzzleTrans.right, currentGunSO.accuracy);
        bullet.velocity = dir * currentGunSO.bulletSpeed;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);

        bullet.Fly();

        StartCoroutine(MuzzleFlash());
    }

    #endregion

    #region Switch Guns

    public virtual void SwitchGuns(int i)
    {
        if (i < 0 || i >= gunList.Count)
        {
            Debug.LogError("Tried to switch to a gun that is not unlocked or does not exist");
            return;
        }

        // Muzzle Flash
        if (muzzleFlash != null)
            Destroy(muzzleFlash);
        //Debug.Log("MuzzleFlash instantiated");
        muzzleFlash = Instantiate(gunList[i].muzzleFlashPrefab, muzzleTrans);
        muzzleFlash.SetActive(false);

        // Sights
        if (gunSights != null)
            Destroy(gunSights);
        if (gunList[i].laserSight != null)
        {
            gunSights = Instantiate(gunList[i].laserSight, muzzleTrans).GetComponent<GunSights>();
            gunSights.Initialize(null);
        }
        // Turrets
        if (gunList[i] is TurretGunSO turretGunSO)
        {
            TurretReloadTime = turretGunSO.reloadSpeed;
        }
        currentGunIndex = i;
        currentGunSO = gunList[i];
        shootSpeed = gunList[i].shootSpeed;
        clipSize = gunList[i].clipSize;

        fireTimer = shootSpeed;
        reloadTimer = TurretReloadTime;
    }

    protected IEnumerator MuzzleFlash()
    {
        //Debug.Log("Muzzle Flash");
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(currentGunSO.muzzleFlashTime);
        muzzleFlash.SetActive(false);
        //currentGun.muzzleFlash.enabled = false;
    }

    protected Vector2 ApplyAccuracy(Vector2 dir, float accuracy)
    {
        // Vector to radians to degrees
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Implement accuracy randomness
        angle += Random.Range(-accuracy, accuracy);

        // Convert back to radians to vector
        float radians = angle * Mathf.Deg2Rad;
        dir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

        return dir;
    }

    #endregion

    #region Charge / Reload

    bool ShouldReload()
    {
        if (currentGunSO.gunType == GunType.Laser)
            return false;

        if (gunsAmmoInClips[currentGunIndex] <= 0)
            return true;
        else
            return false;
    }

    protected virtual void Reload()
    {
        reloadRoutine = StartCoroutine(ReloadRoutine());
    }

    protected virtual IEnumerator ReloadRoutine()
    {
        DropClip();

        while (isReloading)
        {
            yield return new WaitForSeconds(0.1f);
            reloadTimer -= 0.1f;

            if (reloadTimer <= 0)
            {
                isReloading = false;
                gunsAmmoInClips[currentGunIndex] = clipSize;
                reloadTimer = TurretReloadTime;
                reloadRoutine = null;
            }
        }
    }

    public void DropClip()
    {
        if (currentGunSO.emptyClipPrefab != null)
        {
            GameObject obj = Instantiate(currentGunSO.emptyClipPrefab, clipEjectorTrans.position,
                                         gunTrans.rotation, clipEjectorTrans);
            BulletCasingFly casingFly = obj.GetComponent<BulletCasingFly>();
            if (casingFly != null)
            {
                casingFly.casingTrans = clipEjectorTrans;
            }
            else
            {
                Debug.LogWarning("CasingFly not found on clip");
            }

            if (isElevated)
            {
                obj.layer = LayerMask.NameToLayer("Default");
            }
        }
    }

    // Call this whenever adding new charging weapon
    public void StartChargingGuns()
    {
        for (int i = 0; i < gunList.Count; i++)
        {
            if (gunList[i] != null)
            {
                // Only start ChargeGun for chargeables
                if (gunList[i].chargeDelay != 0)
                {
                    if (chargeCoroutines.ContainsKey(i) && chargeCoroutines[i] != null)
                    {
                        StopCoroutine(chargeCoroutines[i]);
                    }
                    chargeCoroutines[i] = StartCoroutine(ChargeGun(i));
                }
            }
        }
    }

    protected IEnumerator ChargeGun(int index)
    {
        while (true)
        {
            if (gunsAmmoInClips[index] < gunList[index].clipSize)
                gunsAmmoInClips[index]++;

            yield return new WaitForSeconds(gunList[index].chargeDelay); 
        }  
    }

    #endregion
}
