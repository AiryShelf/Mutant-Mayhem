using System.Collections;
using System.Collections.Generic;
using LiteDB.Engine;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerShooter : Shooter
{
    public List<bool> gunsUnlocked;
    [SerializeField] SpriteRenderer gunSR;
    public Light2D flashlight1;
    public Light2D flashlight2;
    [SerializeField] ParticleSystem bulletCasingsPS;
    [SerializeField] float casingToGroundTime;
    [SerializeField] AnimationControllerPlayer animControllerPlayer;
    [SerializeField] Animator bodyAnim;
    public int[] gunsAmmo;
    Coroutine shootingCoroutine;
    Dictionary<int, Coroutine> chargeCoroutines = new Dictionary<int, Coroutine>();
    bool waitToShoot;
    public bool isShooting;
    public bool isAiming;
    public bool isBuilding;
    public bool isSwitchingGuns;
    public bool isMeleeing;
    
    bool droppedGun;
    Rigidbody2D myRb;
    
    [HideInInspector]public PlayerStats playerStats;

    [SerializeField] float showDamage;

    protected override void Awake() { }

    void Start()
    {
        myRb = GetComponent<Rigidbody2D>(); 

        CopyGunList();
        RefreshChargeGuns();
        SwitchGuns(0);
    }

    protected override void Update()
    {
        Shoot();
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

    public override void SwitchGuns(int i)
    {
        if (i < 0 || i >= gunList.Count)
        {
            Debug.Log("Tried to switch to a gun that is not unlocked or does not exist");
            return;
        }

        gunSR.sprite = gunList[i].sprite;
        muzzleTrans.localPosition = gunList[i].muzzleLocalPos;
        casingEjectorTrans.localPosition = gunList[i].casingLocalPos;

        // Muzzle Flash
        if (muzzleFlash != null)
            Destroy(muzzleFlash);
        muzzleFlash = Instantiate(gunList[i].muzzleFlashPrefab, muzzleTrans);
        muzzleFlash.SetActive(false);

        // Sights
        if (laserSight != null)
            Destroy(laserSight);
        if (gunList[i].laserSight != null)
            laserSight = Instantiate(gunList[i].laserSight, muzzleTrans);

        // This could be removed and gunIndex used instead for animation transitions
        bodyAnim.SetBool(gunList[currentGunIndex].animatorHasString, false);
        bodyAnim.SetBool(gunList[i].animatorHasString, true);

        currentGunIndex = i;
        currentGunSO = gunList[i];

        AudioManager.instance.PlaySoundAt(currentGunSO.selectedSound, transform.position);
        //Debug.Log("Current gun damage: " + currentGunSO.damage);          
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

    protected override void Fire()
    {
        base.Fire();
        
        StatsCounterPlayer.ShotsFiredByPlayer++;
        Kickback();
        bool oneHand = isMeleeing || !isAiming;
        gunRecoil.TriggerRecoil(currentGunSO.recoilAmount, oneHand);
    }

    #region Sounds

    public void OnReloadSound(int index)
    {
        AudioManager.instance.PlaySoundAt(currentGunSO.reloadSounds[index], transform.position);
    }

    #endregion

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
            Fire();

            yield return new WaitForSeconds(currentGunSO.shootSpeed);
        }
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
