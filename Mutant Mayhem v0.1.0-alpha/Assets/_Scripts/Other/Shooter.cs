using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public List<GunSO> _gunListSource;
    [HideInInspector] public List<GunSO> gunList;
    [SerializeField] protected Transform gunTrans;
    [SerializeField] protected Transform muzzleTrans;
    [SerializeField] protected Transform casingEjectorTrans;
    [SerializeField] protected Transform clipEjectorTrans;
    [SerializeField] protected LayerMask elevatedHitLayers;
    [SerializeField] protected GunRecoil gunRecoil;
    public List<int> gunsAmmoInClips = new List<int>();
    float shootSpeed = 1.0f;
    public float reloadTime = 2.0f; 
    public bool isReloading;
    public bool isElevated;


    [HideInInspector] public bool hasTarget;
    [HideInInspector] public int currentGunIndex = 0;
    [HideInInspector] public GunSO currentGunSO;
    protected GameObject muzzleFlash;
    protected GameObject laserSight;
    Coroutine reloadRoutine;
    int clipSize;
    float fireTimer;
    float reloadTimer;

    protected virtual void Awake()
    {
        CopyGunList();

        // Initialize first gun
        SwitchGuns(0);
        gunsAmmoInClips[0] = 0;
        isReloading = true;
    }

    protected virtual void Update()
    {
        if (isReloading)
        {
            if (reloadRoutine == null)
                reloadRoutine = StartCoroutine(ReloadRoutine());
        }

        if (!hasTarget)
            return;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0)
        {
            Fire();
            fireTimer = shootSpeed;

            // Start reloading after firing a certain number of shots
            if (ShouldReload())
            {
                isReloading = true;
            }
        }
    }

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

    public virtual void SwitchGuns(int i)
    {
        if (i < 0 || i >= gunList.Count)
        {
            Debug.Log("Tried to switch to a gun that is not unlocked or does not exist");
            return;
        }

        // Muzzle Flash
        if (muzzleFlash != null)
            Destroy(muzzleFlash);
        //Debug.Log("MuzzleFlash instantiated");
        muzzleFlash = Instantiate(gunList[i].muzzleFlashPrefab, muzzleTrans);
        muzzleFlash.SetActive(false);

        // Sights
        if (laserSight != null)
            Destroy(laserSight);
        if (gunList[i].laserSight != null)
            laserSight = Instantiate(gunList[i].laserSight, muzzleTrans);

        // Turrets
        if (gunList[i] is TurretGunSO turretGunSO)
        {
            reloadTime = turretGunSO.reloadSpeed;
        }
        currentGunIndex = i;
        currentGunSO = gunList[i];
        shootSpeed = gunList[i].shootSpeed;
        clipSize = gunList[i].clipSize;

        fireTimer = shootSpeed;
        reloadTimer = reloadTime;
    }

    protected virtual void Fire()
    {
        // Use ammo
        gunsAmmoInClips[currentGunIndex]--;

        // Create bullet and casing
        GameObject bulletObj = Instantiate(currentGunSO.bulletPrefab, 
                                           muzzleTrans.position, muzzleTrans.rotation);
        if (currentGunSO.bulletCasingPrefab != null)
        {
            GameObject casingObj = Instantiate(currentGunSO.bulletCasingPrefab, 
                                               casingEjectorTrans.position, 
                                               gunTrans.rotation, casingEjectorTrans);

            // If elevated, all shells go over walls                
            if (isElevated)
            {
                casingObj.layer = LayerMask.NameToLayer("Default");
            }
        }

        Bullet bullet = bulletObj.GetComponent<Bullet>();

        // Check if elevated for shooting over walls
        if (isElevated)
        {
            bullet.hitLayers = elevatedHitLayers;
        }
        
        // Apply stats and effects
        bullet.damage = currentGunSO.damage;
        //Debug.Log("Bullet damage: " + bullet.damage);
        bullet.origin = this.transform;
        bullet.knockback = currentGunSO.knockback;
        bullet.destroyTime = currentGunSO.bulletLifeTime;
        
        Vector2 dir = ApplyAccuracy(muzzleTrans.right);
        bullet.velocity = dir * currentGunSO.bulletSpeed;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);

        StartCoroutine(MuzzleFlash());
    }

    protected Vector2 ApplyAccuracy(Vector2 dir)
    {
        // Vector to radians to degrees
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Implement accuracy randomness
        angle += Random.Range(-currentGunSO.accuracy, currentGunSO.accuracy);

        // Convert back to radians to vector
        float radians = angle * Mathf.Deg2Rad;
        dir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

        return dir;
    }

    bool ShouldReload()
    {
        if (gunsAmmoInClips[currentGunIndex] <= 0)
            return true;
        else
            return false;
    }

    public void DropClip()
    {
        if (currentGunSO.emptyClipPrefab != null)
        {
            GameObject obj = Instantiate(currentGunSO.emptyClipPrefab, clipEjectorTrans.position,
                                         gunTrans.rotation, clipEjectorTrans);
            if (isElevated)
            {
                obj.layer = LayerMask.NameToLayer("Default");
            }
        }
    }

    protected IEnumerator ReloadRoutine()
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
                reloadTimer = reloadTime;
                reloadRoutine = null;
            }
        }
    }

    protected IEnumerator MuzzleFlash()
    {
        //Debug.Log("Muzzle Flash");
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(currentGunSO.muzzleFlashTime);
        muzzleFlash.SetActive(false);
        //currentGun.muzzleFlash.enabled = false;
    }
}
