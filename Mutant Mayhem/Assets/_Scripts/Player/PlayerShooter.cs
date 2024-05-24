using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] Transform gunTrans;
    [SerializeField] SpriteRenderer gunSR;
    [SerializeField] List<GunSO> gunList;
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

    public int[] gunAmmoTotals;
    public int[] gunAmmoInClips;
    [HideInInspector] public GunSO currentGunSO;
    GameObject currentMuzzleFlash;
    Coroutine shootingCoroutine;
    bool waitToShoot;
    [HideInInspector] public bool isShooting;
    [HideInInspector] public bool isAiming;
    public bool isBuilding;
    [HideInInspector] public bool isReloading;
    [HideInInspector] public bool isSwitchingGuns;
    bool droppedGun;
    Rigidbody2D myRb;

    

    void Awake()
    {
        myRb = GetComponent<Rigidbody2D>();
        SwitchGuns(0);
    }

    void Start()
    {
        StartCoroutine(LaserCharge());
    }

    void Update()
    {
            Shoot();
    }

    public void DropGun()
    {
        if (!droppedGun)
        {
            Vector2 worldPos = gunTrans.position;
            Quaternion worldRot = gunTrans.rotation;
            Debug.Log("worlPos: " + worldPos + " worldRot: " + worldRot);
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

    void Shoot()
    {
        if (gunAmmoInClips[currentGunIndex] < 1)
        {
            if (gunAmmoTotals[currentGunIndex] > 0)
            {
                animControllerPlayer.IsReloadInput();
            }
            if (shootingCoroutine != null)
            {
                StopCoroutine(shootingCoroutine);
                shootingCoroutine = null;
            }
            return;
        }


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
    }

    public void Reload()
    {
        if (currentGunIndex != 0)
        {
            int numBullets = currentGunSO.clipSize - gunAmmoInClips[currentGunIndex];

            if (gunAmmoTotals[currentGunIndex] < numBullets)
            {
                gunAmmoInClips[currentGunIndex] += gunAmmoTotals[currentGunIndex];
                gunAmmoTotals[currentGunIndex] = 0;
            }
            else
            {
                gunAmmoInClips[currentGunIndex] += numBullets;
                gunAmmoTotals[currentGunIndex] -= numBullets;
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
            gunAmmoInClips[currentGunIndex]--;
            
            // Apply physics
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            Vector2 dir = ApplyAccuracy(gunTrans.right);
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

    Vector2 ApplyAccuracy(Vector2 dir)
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

    IEnumerator MuzzleFlash()
    {
        currentMuzzleFlash.SetActive(true);
        yield return new WaitForSeconds(currentGunSO.muzzleFlashTime);
        currentMuzzleFlash.SetActive(false);
        //currentGun.muzzleFlash.enabled = false;
    }

    void Kickback()
    {
        myRb.AddForce(-myRb.transform.right * 
                      currentGunSO.kickback, ForceMode2D.Impulse);
    }

    public void StartAiming()
    {
        isAiming = true;
    }

    public void StopAiming()
    {
        isAiming = false;
    }

    IEnumerator LaserCharge()
    {
        while (true)
        {
            if (gunAmmoInClips[0] < laserChargeMax)
                gunAmmoInClips[0]++;
            yield return new WaitForSeconds(laserChargeDelay); 
        }  
    }


}
