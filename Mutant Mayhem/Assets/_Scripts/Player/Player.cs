using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

[System.Serializable]
public class PlayerStats
{
    public StructureStats structureStats;
    [HideInInspector] public Player player;

    [Header("Movement Stats")]
    public float moveSpeed = 8;
    public float strafeSpeed = 5;
    public float sprintFactor = 1.5f;
    public float lookSpeed = 0.05f;
    [HideInInspector] public float moveForce;
    [HideInInspector] public float strafeForce;
    [HideInInspector] public float maxVelocity;

    [Header("Health Stats")]
    public PlayerHealth playerHealthScript;

    [Header("Stamina Stats")]
    public float staminaMax = 100;
    public float staminaRegen = 4f;

    [Header("Melee Stats")]
    public float meleeDamage = 60;
    public float knockback = 10;
    public float meleeSpeedFactor = 1f;

    [Header("Shooting Stats")]
    public PlayerShooter playerShooter;
    public float reloadFactor = 1f;
    public int grenadeAmmo = 12;

    [Header("Accuracy Stats")]
    [Range(1, 0)]
    public float weaponHandling = 1f;
    public float accuracy = 1f;
    public float sprintAccuracyLoss = 4f;
    public float accuracyHoningSpeed = 5f;

    [Header("Crit Hit Stats")]
    public float criticalHitChanceMult = 1f;
    public float criticalHitDamageMult = 1f;
}

[System.Serializable]
public class StructureStats
{
    public QCubeHealth cubeHealthScript;
    public TileManager tileManager;
    public float structureMaxHealthMult = 1;
    public float armour = 0;
    public int maxTurrets = 0;
    public float pulseDefenceForce = 0;
}

public class Player : MonoBehaviour
{
    public PlayerStats stats;

    [Header("Movement")]
    [SerializeField] float forceFactor = 1.5f;
    [SerializeField] float sprintStaminaUse = 0.1f;
    [SerializeField] float headTurnSpeed = 0.1f;
    [SerializeField] float headTurnMax = 80;

    [Header("Sound")]
    [SerializeField] SoundSO walkGrassSound;
    [SerializeField] SoundSO walkWoodSound;
    [SerializeField] SoundSO walkConcreteSound;
    [SerializeField] SoundSO walkMetalSound;

    [Header("Other")]
    public InputActionAsset inputAsset;
    [SerializeField] GameObject grenadePrefab;
    [SerializeField] Transform headImageTrans;
    [SerializeField] Transform playerMainTrans;
    [SerializeField] Transform muzzleTrans;
    public CapsuleCollider2D gunCollider;
    [SerializeField] Transform leftHandTrans;
    public AnimationControllerPlayer animControllerPlayer;
    public MeleeControllerPlayer meleeController;  
    [SerializeField] ToolbarSelector toolbarSelector; 
    [SerializeField] float throwAccuracyLoss = 6f;

    [SerializeField] float experimentRotationConstant; 
    
    float sprintSpeedAmount;
    Vector2 rawInput;
    Vector2 muzzleDirToMouse;
    float muzzleAngleToMouse;
    Rigidbody2D myRb;
    Stamina myStamina;
    public PlayerShooter playerShooter;
    static bool _isDead; // Backing field
    public static event Action<bool> OnPlayerDestroyed;
    public bool IsDead
    {
        get { return _isDead; }
        set
        {
            if (_isDead != value)
            {
                _isDead = value;
                OnPlayerDestroyed?.Invoke(_isDead);
            }
        }
    }
    Throw itemToThrow;
    [HideInInspector] public bool hasFirstThrowTarget;
    [HideInInspector] public Vector2 throwTarget;
    [HideInInspector] public bool useStandardWASD = true;
    float lastFootstepTime;
    float footstepCooldown = 0.1f;
    int previousGunIndex;

    void Awake()
    {
        //KillAllEnemies();
        //Time.timeScale = 1;

        stats.player = GetComponent<Player>();
        playerShooter = GetComponent<PlayerShooter>();
        myRb = GetComponent<Rigidbody2D>();
        myStamina = GetComponent<Stamina>();

        // Initialize stats
        stats.playerHealthScript = GetComponent<PlayerHealth>();
        meleeController.stats = stats;
        myStamina.stats = stats;
        playerShooter.playerStats = stats;
        stats.structureStats.cubeHealthScript = FindObjectOfType<QCubeHealth>();
        IsDead = false;
    }

    void Start()
    {
        //ParticleManager.Instance.ClearAllChildrenParticleSystems();
        TimeControl.Instance.SubscribePlayerTimeControl(this);
        SFXManager.Instance.Initialize();
        //TutorialManager.ResetTutorialPanel();
        StatsCounterPlayer.ResetStatsCounts();
        
        SettingsManager.Instance.GetComponent<CursorManager>().Initialize();
        SettingsManager.Instance.RefreshSettingsFromProfile(ProfileManager.Instance.currentProfile);
        SettingsManager.Instance.ApplyGameplaySettings();

        ClassManager.Instance.ApplyClassEffects(this);
        UpgradeManager.Instance.Initialize();
        AugManager.Instance.ApplySelectedAugmentations();
        TurretManager.Instance.Initialize(this);
        FindObjectOfType<WaveControllerRandom>().Initialize();
        
        RefreshMoveForces();

        //StartCoroutine(DelayInitializePool());
        //PoolManager.Instance.ResetAllPools();
    }

    IEnumerator DelayInitializePool()
    {
        yield return new WaitForFixedUpdate();
        PoolManager.Instance.ResetAllPools();
    }

    void OnDisable()
    {
        TimeControl.Instance.UnsubscribePlayerTimeControl(this);
        //ParticleManager.Instance.ClearAllChildrenParticleSystems();
    }

    void KillAllEnemies()
    {
        bool enemiesExist = true;
        while (enemiesExist)
        {
            EnemyBase enemy = FindObjectOfType<EnemyBase>();
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
            else
                enemiesExist = false;
        }
    }

    void FixedUpdate()
    {
        if (!IsDead)
        {
            LookAtMouse();
            Sprint();
            Move();
        }
        else
        {
            playerShooter.isShooting = false; 
        }
    }

    public void PlayFootStepSound()
    {
        // Need check for ground type
        if (Time.time - lastFootstepTime >= footstepCooldown)
        {
            SFXManager.Instance.PlaySoundAt(walkGrassSound, transform.position);
            lastFootstepTime = Time.time;
        }
    }

    #region Inputs

    void OnToolbar()
    {
        // Laser Pistol
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (playerShooter.currentGunIndex != 0 && 
                animControllerPlayer.SwitchGunsStart(0))
            {
                toolbarSelector.SwitchBoxes(0); 
                previousGunIndex = 0;
            }
        }
        // SMG
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {

            if (playerShooter.currentGunIndex != 1 && 
                animControllerPlayer.SwitchGunsStart(1))
            { 
                toolbarSelector.SwitchBoxes(1);
                previousGunIndex = 1;
            }
        }
        // Battle Rifle
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {

            if (playerShooter.currentGunIndex != 2 && 
                animControllerPlayer.SwitchGunsStart(2))
            { 
                toolbarSelector.SwitchBoxes(2);
                previousGunIndex = 2;
            }
        }
        // Laser Rifle
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {

            if (playerShooter.currentGunIndex != 3 && 
                animControllerPlayer.SwitchGunsStart(3))
            { 
                toolbarSelector.SwitchBoxes(3);
                previousGunIndex = 3;
            }
        }
        // FlameThrower?
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {

            if (playerShooter.currentGunIndex != 4 && 
                animControllerPlayer.SwitchGunsStart(4))
            { 
                toolbarSelector.SwitchBoxes(4);
                previousGunIndex = 4;
            }
        }
        // Rocket Launcher?
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {

            if (playerShooter.currentGunIndex != 5 && 
                animControllerPlayer.SwitchGunsStart(5))
            { 
                toolbarSelector.SwitchBoxes(5);
                previousGunIndex = 5;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {

            if (playerShooter.currentGunIndex != 6 && 
                animControllerPlayer.SwitchGunsStart(6))
            { 
                toolbarSelector.SwitchBoxes(6);
                previousGunIndex = 6;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {

            if (playerShooter.currentGunIndex != 7 && 
                animControllerPlayer.SwitchGunsStart(7))
            { 
                toolbarSelector.SwitchBoxes(7);
                previousGunIndex = 7;
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {

            if (playerShooter.currentGunIndex != 8 && 
                animControllerPlayer.SwitchGunsStart(8))
            { 
                toolbarSelector.SwitchBoxes(8);
                previousGunIndex = 8;
            }
        }
        // REPAIR GUN
        else if (Input.GetKeyDown(KeyCode.Alpha0) ||
                 Input.GetKeyDown(KeyCode.C))
        {
            if (playerShooter.currentGunIndex != 9)
            {
                animControllerPlayer.SwitchGunsStart(9);
                toolbarSelector.SwitchBoxes(9);
            }
            else 
            {
                // Switch back to previous weapon
                animControllerPlayer.SwitchGunsStart(previousGunIndex);
                toolbarSelector.SwitchBoxes(previousGunIndex);
            }
            return;
        }
    }

    public void OnThrowGrab()
    {
        itemToThrow = Instantiate(grenadePrefab, transform.position, 
                      Quaternion.identity, leftHandTrans).gameObject.GetComponent<Throw>();
    }

    public void OnThrowFly()
    {
        // Handles player input holding down the throw action
        if (hasFirstThrowTarget)
        {
            itemToThrow.target = throwTarget;
            hasFirstThrowTarget = false;
        }
        else
        {
            itemToThrow.target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        itemToThrow.transform.parent = null;
        itemToThrow.StartFly();
        stats.grenadeAmmo--;
        StatsCounterPlayer.GrenadesThrownByPlayer++;

        playerShooter.currentAccuracy += throwAccuracyLoss;
    }

    void OnMove(InputValue value)
    {
        rawInput = value.Get<Vector2>();    
    }

    #endregion

    #region Movement

    public void RefreshMoveForces()
    {
        stats.moveForce = stats.moveSpeed * myRb.mass * forceFactor;
        stats.strafeForce = stats.strafeSpeed * myRb.mass * forceFactor;
        stats.maxVelocity = (stats.moveForce * stats.sprintFactor) / 
                            (myRb.drag * myRb.mass);
    }

    void LookAtMouse()
    {
        // Find Mouse direction and angle
        Vector3 mousePos = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        //muzzleDirToMouse = mousePos - transform.position;

        
        if ((transform.position - mousePos).magnitude > 
            (transform.position - muzzleTrans.position).magnitude + 0.5f)
        {
            muzzleDirToMouse = mousePos - muzzleTrans.transform.position;
        }
        else
        {
            muzzleDirToMouse = mousePos - transform.position;
        }
        

        muzzleDirToMouse.Normalize();
    
        // Get muzzle angle and rotation
        muzzleAngleToMouse = Mathf.Atan2(muzzleDirToMouse.y, muzzleDirToMouse.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, muzzleAngleToMouse);

        // Calculate the angle difference between the current and target rotation
        float angleDifference = Quaternion.Angle(playerMainTrans.rotation, targetRotation);

        // Adjust rotation speed: fast at start, slow down as it approaches the target
        float dynamicSpeed = Mathf.Lerp(stats.lookSpeed * 3f, stats.lookSpeed, angleDifference / 180f);

        // Apply the rotation using Quaternion.Slerp for smooth interpolation
        playerMainTrans.rotation = Quaternion.Slerp(
            playerMainTrans.rotation, targetRotation, Time.deltaTime * dynamicSpeed);

        RotateHead(mousePos);
    }

/* THIS WAS TOO SLOW, ESPECIALLY FOR SMALL MOVEMENTS
    void LookAtMouse()
    {
        // Find Mouse direction and angle
        Vector3 mousePos = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        if ((transform.position - mousePos).magnitude > 
            (transform.position - muzzleTrans.position).magnitude)
        {
            muzzleDirToMouse = mousePos - muzzleTrans.transform.position;
        }
        else
        {
            muzzleDirToMouse = mousePos - transform.position;
        }

        muzzleDirToMouse.Normalize();
       
        // Get muzzle angle and rotation
        muzzleAngleToMouse = Mathf.Atan2(muzzleDirToMouse.y, muzzleDirToMouse.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, muzzleAngleToMouse);

        // Apply body rotation 
        double rotationSpeed = Math.Pow(stats.lookSpeed, experimentRotationConstant);
        playerMainTrans.rotation = Quaternion.Lerp(
            playerMainTrans.rotation, targetRotation, (float)rotationSpeed);

        RotateHead(mousePos);
    }
*/

    void RotateHead(Vector3 mousePos)
    {
        // Get head angle
        Vector3 headDirToMouse = mousePos - headImageTrans.position;
        float headAngleToMouse = Mathf.Atan2(headDirToMouse.y, headDirToMouse.x) * Mathf.Rad2Deg;

        // Calculate the difference between the body angle and the head angle
        float bodyAngle = playerMainTrans.eulerAngles.z;
        float relativeAngle = Mathf.DeltaAngle(bodyAngle, headAngleToMouse);

        // Clamp the relative angle to headTurnMax
        float clampedRelativeAngle = Mathf.Clamp(relativeAngle, -headTurnMax, headTurnMax);

        // Calculate the clamped target rotation for the head
        Quaternion clampedTargetRotation = Quaternion.Euler(0, 0, bodyAngle + clampedRelativeAngle);

        // Apply the clamped rotation to the head
        double rotationSpeed = Math.Pow(headTurnSpeed, experimentRotationConstant / 1.5f);
        headImageTrans.rotation = Quaternion.Lerp(
        headImageTrans.rotation, clampedTargetRotation, (float)rotationSpeed);
    }

    void Sprint()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (sprintStaminaUse <= myStamina.GetStamina() && rawInput.sqrMagnitude > 0)
            {
                float time = Time.fixedDeltaTime;
                StatsCounterPlayer.TimeSprintingPlayer += time;
                sprintSpeedAmount = stats.sprintFactor;
                myStamina.ModifyStamina(-sprintStaminaUse);

                playerShooter.currentAccuracy += time * stats.sprintAccuracyLoss * stats.weaponHandling;
            }
            else
                sprintSpeedAmount = 1;
        }
        else
            sprintSpeedAmount = 1;
    }

    void Move()
    {
        Vector2 moveDir;

        if (useStandardWASD == true)
        {
            // Standard WASD movement
            moveDir = new Vector2(rawInput.x, rawInput.y).normalized;
        }
        else
        {
            // Move to mouse
            moveDir = new Vector2(rawInput.y, -rawInput.x).normalized;
            moveDir = Quaternion.Euler(0, 0, muzzleAngleToMouse) * moveDir;
        }

        // Calculate the angle difference between the movement direction and facing direction
        float moveAngle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        float angleDifference = Mathf.DeltaAngle(moveAngle, muzzleAngleToMouse);

        // Determine the speed based on the angle difference
        float maxSpeedAngle = 15f; // Full speed within ±15 degrees
        float minSpeedAngle = 90f;
        float speedFactor = 1.0f;
        
        if (Mathf.Abs(angleDifference) > maxSpeedAngle)
        {
            // Normalize the angle difference within the range
            speedFactor = Mathf.InverseLerp(minSpeedAngle, maxSpeedAngle, Mathf.Abs(angleDifference));
        }

        float speed = Mathf.Lerp(stats.strafeForce, stats.moveForce, speedFactor);
        moveDir *= speed * sprintSpeedAmount;


        //OG code
        //float speed = Mathf.Lerp(strafeForce, moveForce, Mathf.Cos(angleDifference * Mathf.Deg2Rad));
        //moveDir *= speed * sprintSpeedAmount;

        myRb.AddForce(moveDir);
    }

    #endregion
}
