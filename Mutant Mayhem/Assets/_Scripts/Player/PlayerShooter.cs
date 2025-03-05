using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerShooter : Shooter
{
    public List<bool> gunsUnlocked;
    [SerializeField] SpriteRenderer gunSR;
    public Light2D flashlight1;
    public Light2D flashlight2;
    [SerializeField] Animator bodyAnim;
    public int[] gunsAmmo;
    Coroutine shootingCoroutine;
    bool waitToShoot;
    float reloadNotificationTimer = 0;
    [SerializeField] float reloadAccuracyLoss = 8f;

    public bool canShoot = true;
    public bool isShooting;
    public bool isAiming;
    public bool isBuilding;
    public bool isRepairing;
    public bool isSwitchingGuns;
    public bool isMeleeing;
    
    bool droppedGun;
    Rigidbody2D myRb;
    ToolbarSelector toolbarSelector;
    [HideInInspector] public PlayerStats playerStats;
    [HideInInspector] public Player player;

    protected override void Awake() 
    {
        criticalHit = GetComponent<CriticalHit>();
        player = GetComponent<Player>();
        myRb = GetComponent<Rigidbody2D>();
        toolbarSelector = FindObjectOfType<ToolbarSelector>();
        playerShooter = this;
    }

    protected override void Start()
    {
        CopyGunList();
        StartChargingGuns();
        SwitchGuns(0);
    }

    protected override void Update()
    {
        UpdateDynamicAccuracy();
        Shoot();
    }

    public void SetElevated(bool elevated)
    {
        isElevated = elevated;

        flashlight1.shadowsEnabled = !elevated;
        flashlight2.shadowsEnabled = !elevated;
        gunSights.isElevated = elevated;
        if (muzzleLight != null)
            muzzleLight.shadowsEnabled = !elevated;
    }

    public float GetRange()
    {
        return currentGunSO.bulletLifeTime * currentGunSO.bulletSpeed;
    }

    public void StartAiming()
    {
        isAiming = true;
    }

    public void StopAiming()
    {
        isAiming = false;
    }

    public void UnlockGun(int i)
    {
        gunsUnlocked[i] = true;
        toolbarSelector.UnlockBoxImage(i);
        Debug.Log("Unlocked gun index: " + i);
    }

    public void ReloadPlayer()
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

            currentAccuracy += currentGunSO.accuracy * playerStats.weaponHandling;
        }
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

    #region Switch Guns

    public override void SwitchGuns(int i)
    {
        if (i < 0 || i >= gunList.Count)
        {
            Debug.LogError("PlayerShooter: Tried to switch to a gun outside of index range");
            return;
        }

        gunSR.sprite = gunList[i].sprite;
        muzzleTrans.localPosition = gunList[i].muzzleLocalPos;
        casingEjectorTrans.localPosition = gunList[i].casingLocalPos;

        // Muzzle Flash
        if (muzzleFlash != null)
            Destroy(muzzleFlash);
        muzzleFlash = Instantiate(gunList[i].muzzleFlashPrefab, muzzleTrans);
        muzzleLight = muzzleFlash.GetComponent<Light2D>();
        muzzleLight.shadowsEnabled = !isElevated;
        muzzleFlash.SetActive(false);

        // Sights
        if (gunSights != null)
            Destroy(gunSights.gameObject);
        if (gunList[i].laserSight != null)
        {
            gunSights = Instantiate(gunList[i].laserSight, muzzleTrans).GetComponent<GunSights>();
            gunSights.Initialize(player);
        }

        // This could be removed and gunIndex used instead for animation transitions
        bodyAnim.SetBool(gunList[currentGunIndex].animatorHasString, false);
        bodyAnim.SetBool(gunList[i].animatorHasString, true);

        currentGunIndex = i;
        currentGunSO = gunList[i];
        currentAccuracy =  currentGunSO.accuracy;

        SFXManager.Instance.PlaySoundAt(currentGunSO.selectedSound, transform.position);
        //Debug.Log("Current gun damage: " + currentGunSO.damage); 

        // Set cursor
        if (!isBuilding)
            CursorManager.Instance.SetAimCursor();

        if (i == 4) // Repair gun
        {
            isRepairing = true;
            InputManager.SetJoystickMouseControl(true);
            if (!playerStats.structureStats.buildingSystem.isInBuildMode)
            {
                playerStats.structureStats.buildingSystem.buildRangeCircle.EnableCircle(true);
                playerStats.structureStats.buildingSystem.SetRepairRangeCircle();
                CursorManager.Instance.inMenu = true;
            }
        }
        else
        {
            isRepairing = false;
            if (playerStats.structureStats.buildingSystem.isInBuildMode)
                playerStats.structureStats.buildingSystem.SetBuildRangeCircle();
            else
            {
                playerStats.structureStats.buildingSystem.buildRangeCircle.EnableCircle(false);
                playerStats.structureStats.buildingSystem.LockCameraToPlayer(false);
                //BuildingSystem.Instance.buildRangeCircle.radius = BuildingSystem.buildRange;
                InputManager.SetJoystickMouseControl(!SettingsManager.Instance.useFastJoystickAim);
                CursorManager.Instance.inMenu = false;
            }
        }

        Debug.Log("PlayerShooter: CurrentGunSO: " + currentGunSO); 
    }

    #endregion

    #region Shooting

    void Shoot()
    {
        reloadNotificationTimer -= Time.deltaTime;

        if (!canShoot)
            return;

        // If no ammo
        if (gunsAmmoInClips[currentGunIndex] < 1)
        {
            // Reload notification
            if (currentGunSO.gunType == GunType.Bullet)
            {
                if (reloadNotificationTimer < 0)
                {
                    reloadNotificationTimer = MessagePanel.TimeToDisplay * 1.1f;
                    if (gunsAmmo[currentGunIndex] > 0)
                    {
                        MessagePanel.PulseMessage("Clip is empty!  Press 'R' to reload!", Color.yellow);
                        //animControllerPlayer.ReloadTrigger();
                    }
                    else
                        MessagePanel.PulseMessage("Out of ammo!  Buy more at the Cube!", Color.red);
                }
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

    protected override void Fire()
    {
        base.Fire();
        
        // if not repair gun (index 9)
        if (currentGunIndex != 9)
            StatsCounterPlayer.ShotsFiredByPlayer++;

        Kickback();
        bool oneHand = isMeleeing || !isAiming;
        gunRecoil.TriggerRecoil(currentGunSO.recoilAmount, oneHand);

        currentAccuracy += currentGunSO.firingAccuracyLoss * playerStats.weaponHandling;
    }

    void Kickback()
    {
        myRb.AddForce(-myRb.transform.right * 
                      currentGunSO.kickback, ForceMode2D.Impulse);
    }

    void UpdateDynamicAccuracy()
    {
        if (playerShooter)
            currentAccuracy -= Time.deltaTime * playerShooter.player.stats.accuracyHoningSpeed;
        else
            currentAccuracy -= Time.deltaTime * accuracyHoningSpeed;

        currentAccuracy = Mathf.Clamp(currentAccuracy, 0, currentGunSO.accuracy * 
                                      playerShooter.playerStats.weaponHandling);
        gunSights.SetAccuracy(currentAccuracy);
    }

    #endregion

    #region Sounds

    public void OnReloadSound(int index)
    {
        SFXManager.Instance.PlaySoundAt(currentGunSO.reloadSounds[index], transform.position);
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
