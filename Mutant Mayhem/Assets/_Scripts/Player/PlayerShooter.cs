using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] List<GunSO> _gunListSource;
    public List<GunSO> gunList;
    [SerializeField] Transform gunTrans;
    [SerializeField] SpriteRenderer gunSR;
    [SerializeField] Transform muzzleTrans;
    [SerializeField] ParticleSystem bulletCasingsPS;
    [SerializeField] float casingToGroundTime;
    [SerializeField] Transform casingEjectorTrans;
    [SerializeField] Transform clipEjectorTrans;
    [SerializeField] AnimationControllerPlayer animControllerPlayer;
    [SerializeField] Animator bodyAnim;
    public int currentGunIndex = 0;

    [SerializeField] float laserChargeDelay = 0.5f;
    [SerializeField] int laserChargeMax = 50;

    public int[] gunsAmmo;
    public int[] gunsAmmoInClips;

    [HideInInspector] public GunSO currentGunSO;
    GameObject currentMuzzleFlash;
    Coroutine shootingCoroutine;
    Dictionary<int, Coroutine> chargeCoroutines = new Dictionary<int, Coroutine>();
    bool waitToShoot;
    [HideInInspector] public bool isShooting;
    [HideInInspector] public bool isAiming;
    [HideInInspector] public bool isBuilding;
    [HideInInspector] public bool isReloading;
    [HideInInspector] public bool isSwitchingGuns;
    bool droppedGun;
    Rigidbody2D myRb;
    
    [HideInInspector]public PlayerStats playerStats;

    [SerializeField] float showDamage;
    

    void Awake()
    {
        myRb = GetComponent<Rigidbody2D>();  

        // Make a working copy of the gun list
        foreach (GunSO gun in _gunListSource)
        {
            GunSO g = Instantiate(gun);
            gunList.Add(g);
        }
    }

    void Start()
    {
        RefreshChargeGuns();
        SwitchGuns(0);
    }

    void Update()
    {
            Shoot();
            Debug.Log("Current gun damage: " + currentGunSO.damage);
    }

    public void DropGun()
    {
        if (!droppedGun)
        {
            Vector2 worldPos = gunTrans.position;
            Quaternion worldRot = gunTrans.rotation;
            //Debug.Log("worlPos: " + worldPos + " worldRot: " + worldRot);
            gunTrans.parent = null;
            SpriteRenderer sR = gunTrans.GetComponent<SpriteRenderer>();
            sR.sortingLayerName = "Structures";
            sR.sortingOrder = -1;
            gunTrans.position = worldPos;
            gunTrans.rotation = worldRot;
            droppedGun = true;
            myRb.mass = 20;
            //myRb.drag = 10;
            myRb.freezeRotation = true;
        }
    }

    public void SwitchGuns(int i)
    {
        gunSR.sprite = gunList[i].sprite;
        muzzleTrans.localPosition = gunList[i].muzzleLocalPos;
        casingEjectorTrans.localPosition = gunList[i].casingLocalPos;

        if (currentMuzzleFlash != null)
            Destroy(currentMuzzleFlash);
        currentMuzzleFlash = Instantiate(gunList[i].muzzleFlashPrefab, 
                                         muzzleTrans.transform);
        currentMuzzleFlash.SetActive(false);

        // This could be removed and gunIndex used instead for animation transitions
        bodyAnim.SetBool(gunList[currentGunIndex].animatorHasString, false);
        bodyAnim.SetBool(gunList[i].animatorHasString, true);

        currentGunIndex = i;
        currentGunSO = gunList[i];
    }

    public void Reload()
    {
        if (currentGunIndex != 0)
        {
            int numBullets = currentGunSO.clipSize - gunsAmmoInClips[currentGunIndex];

            if (gunsAmmo[currentGunIndex] < numBullets)
            {
                gunsAmmoInClips[currentGunIndex] += gunsAmmo[currentGunIndex];
                gunsAmmo[currentGunIndex] = 0;
            }
            else
            {
                gunsAmmoInClips[currentGunIndex] += numBullets;
                gunsAmmo[currentGunIndex] -= numBullets;
            }
        }
    }

    // Call this whenever adding new charging weapon 
    // or charging ability to non-charge weapon
    void RefreshChargeGuns()
    {
        for (int i = 0; i < gunList.Count; i++)
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

    public void DropClip()
    {
        if (currentGunSO.emptyClipPrefab != null)
        {
            Instantiate(currentGunSO.emptyClipPrefab, clipEjectorTrans.position,
                        gunTrans.rotation, clipEjectorTrans);
        }
    }

    void Kickback()
    {
        myRb.AddForce(-myRb.transform.right * 
                      currentGunSO.recoil, ForceMode2D.Impulse);
    }

    Vector2 ApplyAccuracy(Vector2 dir)
    {
        // Vector to radians to degrees
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Implement accuracy randomness
        angle += Random.Range(-currentGunSO.accuracy * playerStats.accuracy, 
                              currentGunSO.accuracy * playerStats.accuracy);

        // Convert back to radians to vector
        float radians = angle * Mathf.Deg2Rad;
        dir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

        return dir;
    }

    public void StartAiming()
    {
        isAiming = true;
    }

    public void StopAiming()
    {
        isAiming = false;
    }

    void Shoot()
    {
        // If no ammo
        if (gunsAmmoInClips[currentGunIndex] < 1)
        {
            // Reload?
            if (gunsAmmo[currentGunIndex] > 0)
            {
                animControllerPlayer.ReloadTrigger();
            }
            // Stop shooting coroutine and return
            if (shootingCoroutine != null)
            {
                StopCoroutine(shootingCoroutine);
                shootingCoroutine = null;
            }
            return;
        }

        #region State Check for Firing

        if (isShooting && shootingCoroutine == null && !waitToShoot && 
            isAiming && !isBuilding && !isReloading && !isSwitchingGuns)
        {
            shootingCoroutine = StartCoroutine(ShootContinuously());
            StartCoroutine(WaitToShoot());
            waitToShoot = true;
        }
        else if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }

        #endregion
    }

    #region Coroutines

    IEnumerator WaitToShoot()
    {
        yield return new WaitForSeconds(currentGunSO.shootSpeed);
        waitToShoot = false;
    }

    IEnumerator ShootContinuously()
    {
        while (true)
        {
            // Create bullet and casing
            GameObject bullet = Instantiate(currentGunSO.bulletPrefab, 
                                            muzzleTrans.position, muzzleTrans.rotation);
            if (currentGunSO.bulletCasingPrefab != null)
            {
                Instantiate(currentGunSO.bulletCasingPrefab, casingEjectorTrans.position, 
                            gunTrans.rotation, casingEjectorTrans);
            }

            // Use ammo
            gunsAmmoInClips[currentGunIndex]--;
            StatsCounterPlayer.ShotsFiredByPlayer++;
            
            // Apply physics and effects
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            Vector2 dir = ApplyAccuracy(muzzleTrans.right);
            rb.velocity = dir * currentGunSO.bulletSpeed;
            Kickback();
            StartCoroutine(MuzzleFlash());

            StartCoroutine(ManageBulletTrails(bullet, currentGunSO.bulletLifeTime));
            Destroy(bullet, currentGunSO.bulletLifeTime);

            yield return new WaitForSeconds(currentGunSO.shootSpeed);
        }
    }

    IEnumerator ManageBulletTrails(GameObject bullet, float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        if (bullet != null)
        {
            BulletTrails trails = bullet.GetComponent<BulletTrails>();
            trails.transform.parent = null;
            trails.DestroyAfterSeconds();
        }
    }

    IEnumerator MuzzleFlash()
    {
        currentMuzzleFlash.SetActive(true);
        yield return new WaitForSeconds(currentGunSO.muzzleFlashTime);
        currentMuzzleFlash.SetActive(false);
        //currentGun.muzzleFlash.enabled = false;
    }

    IEnumerator ChargeGun(int index)
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
