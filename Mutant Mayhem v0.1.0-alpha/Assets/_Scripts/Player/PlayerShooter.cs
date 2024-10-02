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
    bool waitToShoot;
    public bool isShooting;
    public bool isAiming;
    public bool isBuilding;
    public bool isSwitchingGuns;
    public bool isMeleeing;
    
    bool droppedGun;
    Rigidbody2D myRb;
    ToolbarSelector toolbarSelector;
    
    [HideInInspector]public PlayerStats playerStats;

    [SerializeField] float showDamage;

    protected override void Awake() { }

    protected override void Start()
    {
        myRb = GetComponent<Rigidbody2D>();
        toolbarSelector = FindObjectOfType<ToolbarSelector>(); 

        CopyGunList();
        StartChargingGuns();
        SwitchGuns(0);
    }

    protected override void Update()
    {
        Shoot();
    }

    public void UnlockGun(int i)
    {
        gunsUnlocked[i] = true;
        toolbarSelector.UnlockBoxImage(i);
        Debug.Log("Unlocked gun index: " + i);
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
            Debug.Log("Tried to switch to a gun outside of index range");
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
            Destroy(laserSight.gameObject);
        if (gunList[i].laserSight != null)
        {
            laserSight = Instantiate(gunList[i].laserSight, muzzleTrans).GetComponent<GunSights>();
            laserSight.RefreshSights();
        }

        // This could be removed and gunIndex used instead for animation transitions
        bodyAnim.SetBool(gunList[currentGunIndex].animatorHasString, false);
        bodyAnim.SetBool(gunList[i].animatorHasString, true);

        currentGunIndex = i;
        currentGunSO = gunList[i];

        AudioManager.Instance.PlaySoundAt(currentGunSO.selectedSound, transform.position);
        //Debug.Log("Current gun damage: " + currentGunSO.damage); 

        // Set cursor
        if (!isBuilding)
            CursorManager.Instance.SetAimCursor();  
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
        AudioManager.Instance.PlaySoundAt(currentGunSO.reloadSounds[index], transform.position);
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

    #endregion
}
