using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    [Header("Drone Stats")]
    public int numStartBuilderDrones = 2;
    public int numStartAttackDrones = 1;
}

[System.Serializable]
public class StructureStats
{
    public QCubeController cubeController;
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
    [SerializeField] float joystickDeadzone = 0.05f;
    [SerializeField] float joystickCurveMagnitude = 2;

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
    [SerializeField] Transform leftHandTrans;
    public AnimationControllerPlayer animControllerPlayer;
    public MeleeControllerPlayer meleeController;  
    [SerializeField] ToolbarSelector toolbarSelector; 
    [SerializeField] float throwAccuracyLoss = 6f;

    [SerializeField] float experimentRotationConstant; 
    
    Vector3 aimPos = Vector3.zero;
    Vector3 lastAimDir = Vector3.zero;
    float aimDistance = 10;
    float aimMinDist = 5;
    Rect blankRect = new Rect();

    Coroutine sprintCoroutine;
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
    [HideInInspector] public bool useFastJoystickAim = false;
    float lastFootstepTime;
    float footstepCooldown = 0.1f;
    int previousGunIndex;
    [SerializeField] List<GraphicRaycaster> graphicRaycasters;

    InputAction sprintAction;

    void Awake()
    {
        //KillAllEnemies();
        TimeControl.Instance.ResetTimeScale();

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
        stats.structureStats.cubeController = FindObjectOfType<QCubeController>();
        IsDead = false;

        // Inputs
        InputActionMap actionMap = inputAsset.FindActionMap("Player");
        sprintAction = actionMap.FindAction("Sprint");
        aimDistance = CursorManager.Instance.aimDistance;
        aimMinDist = CursorManager.Instance.aimMinDistance;
    }

    void OnEnable()
    {
        sprintAction.performed += SprintInput_Performed;
        sprintAction.canceled += SprintInput_Cancelled;
        TimeControl.Instance.SubscribePlayerTimeControl(this);
    }

    void OnDisable()
    {
        sprintAction.performed -= SprintInput_Performed;
        sprintAction.canceled -= SprintInput_Cancelled;
        TimeControl.Instance.UnsubscribePlayerTimeControl(this);
    }

    void Start()
    {
        SFXManager.Instance.Initialize();
        StatsCounterPlayer.ResetStatsCounts();
        
        CursorManager.Instance.Initialize();
        CursorManager.Instance.SetGraphicRaycasters(graphicRaycasters);
        InputController.SetJoystickMouseControl(!SettingsManager.Instance.useFastJoystickAim);
        SettingsManager.Instance.RefreshSettingsFromProfile(ProfileManager.Instance.currentProfile);
        SettingsManager.Instance.ApplyGameplaySettings();

        TurretManager.Instance.Initialize(this);
        UpgradeManager.Instance.Initialize();
        ClassManager.Instance.ApplyClassEffects(this);
        AugManager.Instance.ApplySelectedAugmentations();
        PlanetManager.Instance.ApplyPlanetProperties();
        
        FindObjectOfType<WaveControllerRandom>().Initialize();
        
        StartCoroutine(Sprint(false));
        RefreshMoveForces();
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

    void SprintInput_Performed(InputAction.CallbackContext context)
    {
        if (sprintCoroutine != null)
            StopCoroutine(sprintCoroutine);
        sprintCoroutine = StartCoroutine(Sprint(true));
        Debug.Log("Sprint was triggered");
    }

    void SprintInput_Cancelled(InputAction.CallbackContext context)
    {
        if (sprintCoroutine != null)
            StopCoroutine(sprintCoroutine);
        sprintCoroutine = StartCoroutine(Sprint(false));
        Debug.Log("Sprint was cancelled");
    }

    void OnToolbar()
    {
        // Laser Weapon
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SwitchToGun(0);
        // Bullet Weapon
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            SwitchToGun(1);
        // ??
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            SwitchToGun(2);
        // ??
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            SwitchToGun(3);
        // REPAIR GUN
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            animControllerPlayer.SwitchGunsStart(4);
            toolbarSelector.SwitchBoxes(4);
        }
        else if (Input.GetKeyDown(KeyCode.C) || Gamepad.current.dpad.up.isPressed)
        {
            if (playerShooter.currentGunIndex != 4)
            {
                animControllerPlayer.SwitchGunsStart(4);
                toolbarSelector.SwitchBoxes(4);
            }
            else 
            {
                // Switch back to previous weapon
                animControllerPlayer.SwitchGunsStart(previousGunIndex);
                toolbarSelector.SwitchBoxes(previousGunIndex);
            }
            return;
        }
        else if (Gamepad.current.dpad.left.isPressed)
        {
            int index = GetNextUnlockedGun(playerShooter.currentGunIndex, -1);
            if (index != -1)
            {
                animControllerPlayer.SwitchGunsStart(index);
                toolbarSelector.SwitchBoxes(index);
            }
        }
        else if (Gamepad.current.dpad.right.isPressed)
        {
            int index = GetNextUnlockedGun(playerShooter.currentGunIndex, 1);
            if (index != -1)
            {
                animControllerPlayer.SwitchGunsStart(index);
                toolbarSelector.SwitchBoxes(index);
            }
        }
    }

    int GetNextUnlockedGun(int startIndex, int direction)
{
    int totalWeapons = playerShooter.gunsUnlocked.Count;
    int index = startIndex;

    for (int i = 0; i < totalWeapons; i++)
    {
        index = (index + direction + totalWeapons) % totalWeapons; // Loop around
        if (playerShooter.gunsUnlocked[index]) 
            return index;
    }

    return -1; // No unlocked weapon found (shouldn't happen unless all are locked)
}

    void SwitchToGun(int gunIndex)
    {
        if (playerShooter.currentGunIndex != gunIndex && 
            animControllerPlayer.SwitchGunsStart(gunIndex))
        {
            toolbarSelector.SwitchBoxes(gunIndex); 
            previousGunIndex = gunIndex;
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

    #region Look At Mouse

    void LookAtMouse()
    {
        // Find Mouse direction and angle
        float joystickX = Input.GetAxis("RightStickHorizontal");
        float joystickY = Input.GetAxis("RightStickVertical");
        Vector2 joystickInput = new Vector2(joystickX, joystickY);

        float joystickInputMagnitude = joystickInput.magnitude;
        float curvedMagnitude = Mathf.Pow(joystickInputMagnitude, joystickCurveMagnitude);
        float scaledDistance = Mathf.Lerp(aimMinDist, aimDistance, curvedMagnitude);

        if (InputController.GetJoystickAsMouseState() && CursorManager.Instance.usingCustomCursor)
        {
            // Use custom cursor position directly for aiming
            aimPos = CursorManager.Instance.GetCustomCursorWorldPos();
            lastAimDir = Vector3.zero; // Reset last joystick aim direction
        }
        else
        {
            // Mouse position
            if (InputController.LastUsedDevice == Keyboard.current)
            {
                aimPos = CursorManager.Instance.GetCustomCursorWorldPos();
                //aimPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                lastAimDir = Vector3.zero;
            }
            // Touchscreen
            else if (InputController.LastUsedDevice == Touchscreen.current)
            {
                // TODO: Get input from virtual joysticks, apply to aimPos
            }
            // Joystick input
            else if (joystickInputMagnitude > joystickDeadzone)
            {
                lastAimDir = (Vector3)(joystickInput.normalized * scaledDistance);
                aimPos = transform.position + lastAimDir;
            }
            else if (lastAimDir != Vector3.zero)
            {
                aimPos = transform.position + lastAimDir;
                //lastAimDir = aimPos;
            }
        }

        // Aim or virtual mouse for joystick
        if (!InputController.GetJoystickAsMouseState())
            CursorManager.Instance.MoveCustomCursorWorldToUi(aimPos);

        if ((transform.position - aimPos).magnitude > 
            (transform.position - muzzleTrans.position).magnitude + 0.5f)
        {
            muzzleDirToMouse = aimPos - muzzleTrans.transform.position;
        }
        else
        {
            muzzleDirToMouse = aimPos - transform.position;
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

        RotateHead(aimPos);
    }

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

    #endregion

    #region Movement

    public void RefreshMoveForces()
    {
        stats.moveForce = stats.moveSpeed * myRb.mass * forceFactor;
        stats.strafeForce = stats.strafeSpeed * myRb.mass * forceFactor;
        stats.maxVelocity = (stats.moveForce * stats.sprintFactor) / 
                            (myRb.drag * myRb.mass);
    }

    IEnumerator Sprint(bool isSprinting)
    {
        if (isSprinting)
        {
            while (true)
            {
                if (sprintStaminaUse <= myStamina.GetStamina()  && rawInput.sqrMagnitude > 0 )
                {
                    float time = Time.fixedDeltaTime;
                    StatsCounterPlayer.TimeSprintingPlayer += time;
                    sprintSpeedAmount = stats.sprintFactor;
                    myStamina.ModifyStamina(-sprintStaminaUse);

                    playerShooter.currentAccuracy += time * stats.sprintAccuracyLoss * stats.weaponHandling;
                    //Debug.Log("Sprint code ran successfully");
                }
                else
                    sprintSpeedAmount = 1;

                yield return new WaitForFixedUpdate();
            }
            
        }
        else
            sprintSpeedAmount = 1;

        yield return new WaitForEndOfFrame();
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
        float minSpeedAngle = 120f;
        float speedFactor = 1.0f;
        
        if (Mathf.Abs(angleDifference) > maxSpeedAngle)
        {
            // Normalize the angle difference within the range
            speedFactor = Mathf.InverseLerp(minSpeedAngle, maxSpeedAngle, Mathf.Abs(angleDifference));
        }

        float speed = Mathf.Lerp(stats.strafeForce, stats.moveForce, speedFactor);
        moveDir *= speed * sprintSpeedAmount;

        myRb.AddForce(moveDir);
    }

    #endregion
}
