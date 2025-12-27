using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
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

    [Header("Shooting Stats")]
    public PlayerShooter playerShooter;
    public float reloadFactor = 1f;
    public int grenadeAmmo = 12;

    [Header("Accuracy Stats")]
    [Range(1, 0)]
    public float weaponHandling = 1f;
    public float sprintAccuracyLoss = 4f;
    public float accuracyHoningSpeed = 5f;

    [Header("Crit Hit Stats")]
    public float criticalHitChanceMult = 1f;
    public float criticalHitDamageMult = 1f;
}

[System.Serializable]
public class StructureStats
{
    public BuildingSystem buildingSystem;
    public QCubeController cubeController;
    public QCubeHealth cubeHealthScript;
    public TileManager tileManager;
    public float structureMaxHealthMult = 1;
    public float armour = 0;
    public float pulseDefenceForce = 0;
    public DroneContainer currentDroneContainer = null;
}

public class Player : MonoBehaviour
{
    [SerializeField] SystemsInitializer systemsInitializer;
    public PlayerStats stats;

    [Header("Movement")]
    [SerializeField] float forceFactor = 1.5f;
    [SerializeField] float sprintStaminaUse = 0.1f;
    [SerializeField] float headTurnSpeed = 0.1f;
    [SerializeField] float headTurnMax = 80;
    public float joystickDeadzone = 0.05f;
    [SerializeField] float joystickCurveMagnitude = 2;


    [Header("Sound")]
    [SerializeField] SoundSO walkGrassSound;
    [SerializeField] SoundSO walkWoodSound;
    [SerializeField] SoundSO walkConcreteSound;
    [SerializeField] SoundSO walkMetalSound;

    [Header("Other")]
    public List<GraphicRaycaster> graphicRaycasters;
    public InputActionAsset inputAsset;
    [SerializeField] string grenadePoolName;
    [SerializeField] Transform headImageTrans;
    [SerializeField] Transform playerMainTrans;
    [SerializeField] Transform muzzleTrans;
    [SerializeField] Transform leftHandTrans;
    public AnimationControllerPlayer animControllerPlayer;
    public MeleeControllerPlayer meleeController;  
    [SerializeField] ToolbarSelector toolbarSelector; 
    [SerializeField] float throwAccuracyLoss = 6f;
    [SerializeField] CameraController cameraController;
    [SerializeField] float experimentRotationConstant; 
    
    [Header("Dynamic Vars, Don't set here")]
    public Vector3 aimWorldPos = Vector3.zero;
    public Vector3 lastAimDir = Vector3.zero;
    public float aimDistance = 10;
    public float aimMinDist = 5;

    float slowFactor = 1;
    Coroutine sprintCoroutine;
    float sprintSpeedAmount;
    public Vector2 rawInput;
    Vector2 muzzleDirToMouse;
    float muzzleAngleToMouse;
    Rigidbody2D myRb;
    public Stamina myStamina;
    public bool isSprinting;
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
    ThrowGrenade itemToThrow;
    [HideInInspector] public bool hasFirstThrowTarget;
    [HideInInspector] public Vector2 throwTarget;
    [HideInInspector] public bool useStandardWASD = true;
    [HideInInspector] public bool useFastJoystickAim = false;
    float lastFootstepTime;
    float footstepCooldown = 0.1f;
    int previousGunIndex;

    InputAction sprintAction;
    InputAction fireAction;
    InputAction throwAction;
    InputAction toolbarAction;
    InputActionMap uIActionMap;
    InputAction escapeAction;
    bool isInteracting = false;
    PlayerUIButton[] uiButtons;

    void Awake()
    {
        //KillAllEnemies();

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
        InputActionMap playerMap = inputAsset.FindActionMap("Player");
        sprintAction = playerMap.FindAction("Sprint");
        fireAction = playerMap.FindAction("Fire");
        throwAction = playerMap.FindAction("Throw");
        toolbarAction = playerMap.FindAction("Toolbar");

        uIActionMap = inputAsset.FindActionMap("UI");
        escapeAction = uIActionMap.FindAction("Escape");

        uIActionMap.Enable();
        playerMap.Enable();
    }

    void OnEnable()
    {
        sprintAction.performed += SprintInput_Performed;
        sprintAction.canceled += SprintInput_Cancelled;
        escapeAction.performed += OnEscapePressed;
    }

    void OnDisable()
    {
        sprintAction.performed -= SprintInput_Performed;
        sprintAction.canceled -= SprintInput_Cancelled;
        escapeAction.performed -= OnEscapePressed;

        TimeControl.Instance.UnsubscribePlayerTimeControl(this);
    }

    void Start()
    {
        systemsInitializer.InitializeLevelStart(this);
        ProfileManager.Instance.currentProfile.playthroughs++;
        ProfileManager.Instance.SaveCurrentProfile();

        CursorManager.Instance.SetCustomCursorVisible(true);
        CursorManager.Instance.MoveCustomCursorWorldToUi(Vector2.zero);
        StartCoroutine(Sprint(false));
        RefreshMoveForces();

        // Set up UI attack buttons
        uiButtons = FindObjectsOfType<PlayerUIButton>(true);
        foreach (PlayerUIButton button in uiButtons)
        {
            switch (button.buttonType)
            {
                case PlayerUIButtonType.Shoot:
                    button.onPressed.AddListener(OnShootPressed);
                    button.onReleased.AddListener(OnShootReleased);
                    break;
                case PlayerUIButtonType.Melee:
                    button.onPressed.AddListener(OnMeleePressed);
                    button.onReleased.AddListener(OnMeleeReleased);
                    break;
            }
        }
    }

    void OnDestroy()
    {
        foreach (PlayerUIButton button in uiButtons)
        {
            switch (button.buttonType)
            {
                case PlayerUIButtonType.Shoot:
                    button.onPressed.RemoveListener(OnShootPressed);
                    button.onReleased.RemoveListener(OnShootReleased);
                    break;
                case PlayerUIButtonType.Melee:
                    button.onPressed.RemoveListener(OnMeleePressed);
                    button.onReleased.RemoveListener(OnMeleeReleased);
                    break;
            }
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

    public void PlayFootStepSound()
    {
        // Need check for ground type
        if (Time.time - lastFootstepTime >= footstepCooldown)
        {
            AudioManager.Instance.PlaySoundAt(walkGrassSound, transform.position);
            lastFootstepTime = Time.time;
        }
    }

    #region Inputs

    // These are used by the Touchscreen UI buttons
    public void OnShootPressed()
    {
        animControllerPlayer.FireInput_Performed(new InputAction.CallbackContext());
    }

    public void OnShootReleased()
    {
        animControllerPlayer.FireInput_Cancelled(new InputAction.CallbackContext());
    }

    public void OnMeleePressed()
    {
        animControllerPlayer.MeleeInput_Performed(new InputAction.CallbackContext());
    }

    public void OnMeleeReleased()
    {
        animControllerPlayer.MeleeInput_Cancelled(new InputAction.CallbackContext());
    }

    public void OnInteract()
    {
        // Close build menu
        if (BuildingSystem.Instance.isInBuildMode)
            animControllerPlayer.ToggleBuildMode();

        // Check for panels and open the closest one
        if (!isInteracting)
        {
            if (InteractController.Instance.OpenHighlightedPanel(this))
            {
                EnterInteractMode();
            }
            else
            {
                MessageBanner.PulseMessage("No interactable object nearby!  Get closer!", Color.yellow);
            }
        }
        else
        {
            CloseInteractPanel();
            ExitInteractMode();
        }
    }

    public void OnEscapePressed(InputAction.CallbackContext context)
    {
        //Debug.Log("OnEscapePressed called");
        if (isInteracting)
        {
            CloseInteractPanel();
            ExitInteractMode();
        }

        if (PanelManager.NumPanelsOpen > 0)
            return;

        if (UpgradePanelManager.Instance.isOpen)
        {
            StartCoroutine(WaitToCheckForPause());
        }
    }

    void CloseInteractPanel()
    {
        InteractController.Instance.CloseOpenedPanel();
    }

    void EnterInteractMode()
    {
        //yield return new WaitForFixedUpdate();
        stats.playerShooter.isRepairing = false;

        if (InputManager.LastUsedDevice == Touchscreen.current)
            CursorManager.Instance.SetCustomCursorVisible(false);

        InputManager.SetJoystickMouseControl(true);
        CursorManager.Instance.inMenu = true;
        cameraController.ZoomAndFocus(transform, 1, 0f, 0.5f, true, false);
        cameraController.SetTouchscreenOffset(false);

        animControllerPlayer.FireInput_Cancelled(new InputAction.CallbackContext());
        fireAction.Disable();
        throwAction.Disable();
        if (InputManager.LastUsedDevice == Gamepad.current)
            toolbarAction.Disable();

        isInteracting = true;
    }

    public void ExitInteractMode()
    {
        if (stats.playerShooter.currentGunIndex == 4) // Repair Gun
            stats.playerShooter.isRepairing = true;
        else
            InputManager.SetJoystickMouseControl(!SettingsManager.Instance.useInstantJoystickAim);

        CursorManager.Instance.inMenu = false;
        if (!stats.playerShooter.isBuilding && !stats.playerShooter.isRepairing)
        {
            cameraController.ZoomAndFocus(transform, 1, 1, 0.5f, false, false);
            cameraController.SetTouchscreenOffset(true);
        }
        else
            BuildingSystem.Instance.LockCameraToPlayer(true);

        CursorManager.Instance.SetCustomCursorVisible(true);

        //Debug.Log("CloseUpgradeWindow ran");
        fireAction.Enable();
        throwAction.Enable();
        toolbarAction.Enable();
        isInteracting = false;
    }

    IEnumerator WaitToCheckForPause()
    {
        yield return new WaitForSecondsRealtime(0.05f);

        ExitInteractMode();
    }

    public void SprintInput_Performed(InputAction.CallbackContext context)
    {
        if (sprintCoroutine != null)
            StopCoroutine(sprintCoroutine);
        sprintCoroutine = StartCoroutine(Sprint(true));
        //Debug.Log("Sprint was triggered");
    }

    public void SprintInput_Cancelled(InputAction.CallbackContext context)
    {
        if (sprintCoroutine != null)
            StopCoroutine(sprintCoroutine);
        sprintCoroutine = StartCoroutine(Sprint(false));
        //Debug.Log("Sprint was cancelled");
    }

    void OnToolbar()
    {
        // Laser Weapon
        if (Keyboard.current.digit1Key.isPressed)
            SwitchToGun(0);
        // Bullet Weapon
        else if (Keyboard.current.digit2Key.isPressed)
            SwitchToGun(1);
        // ??
        else if (Keyboard.current.digit3Key.isPressed)
            SwitchToGun(2);
        // ??
        else if (Keyboard.current.digit4Key.isPressed)
            SwitchToGun(3);
        // REPAIR GUN
        else if (Keyboard.current.digit5Key.isPressed)
        {
            animControllerPlayer.SwitchGunsStart(4);
        }
        else if ((Keyboard.current != null && Keyboard.current.cKey.isPressed) || 
                 (Gamepad.current != null && Gamepad.current.dpad.up.isPressed))
        {
            if (playerShooter.currentGunIndex != 4)
            {
                animControllerPlayer.SwitchGunsStart(4);
            }
            else 
            {
                // Switch back to previous weapon
                animControllerPlayer.SwitchGunsStart(previousGunIndex);
            }
            return;
        }
        else if (Gamepad.current != null && Gamepad.current.dpad.left.isPressed)
        {
            int index = GetNextUnlockedGun(playerShooter.currentGunIndex, -1);
            if (index != -1)
            {
                animControllerPlayer.SwitchGunsStart(index);
            }
        }
        else if (Gamepad.current != null && Gamepad.current.dpad.right.isPressed)
        {
            int index = GetNextUnlockedGun(playerShooter.currentGunIndex, 1);
            if (index != -1)
            {
                animControllerPlayer.SwitchGunsStart(index);
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

    public void SwitchToGun(int gunIndex)
    {
        if (playerShooter.currentGunIndex != gunIndex && 
            animControllerPlayer.SwitchGunsStart(gunIndex))
        {
            previousGunIndex = gunIndex;
        }
    }

    public void OnThrowGrab()
    {
        GameObject obj = PoolManager.Instance.GetFromPool(grenadePoolName);
        itemToThrow = obj.GetComponent<ThrowGrenade>();
        if (itemToThrow == null)
        {
            Debug.LogError("Player: Pooled object is missing ThrowGrenade component!");
            return;
        }
        itemToThrow.transform.parent = leftHandTrans;
        itemToThrow.transform.localPosition = Vector3.zero;
        itemToThrow.transform.localRotation = Quaternion.identity;
    }

    public void OnThrowFly()
    {
        if (itemToThrow == null)
        {
            Debug.LogError("No grenade to throw!");
            return;
        }

        // Handles player input holding down the throw action
        if (hasFirstThrowTarget)
        {
            itemToThrow.target = throwTarget;
            hasFirstThrowTarget = false;
        }
        else
        {
            itemToThrow.target = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
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
        //Debug.Log($"OnMove called, rawInput: {rawInput}");
    }

    #endregion

    #region Look At Mouse

    void LookAtMouse()
    {
        Vector2 joystickInput = Vector2.zero;
        if (InputManager.LastUsedDevice == Touchscreen.current)
            joystickInput = TouchManager.Instance.aimJoystick.JoystickOutput;
        else if (InputManager.LastUsedDevice == Gamepad.current)
            joystickInput = Gamepad.current.rightStick.ReadValue();

        float joystickInputMagnitude = joystickInput.magnitude;
        float curvedMagnitude = Mathf.Pow(joystickInputMagnitude, joystickCurveMagnitude);
        float scaledDistance = Mathf.Lerp(aimMinDist, aimDistance, curvedMagnitude);

        if (playerShooter.isBuilding || playerShooter.isRepairing || InputManager.LastUsedDevice == Keyboard.current)
        {
            //Debug.Log("Player: Aiming to cursorPos");
            aimWorldPos = CursorManager.Instance.GetCustomCursorWorldPos();
            //lastAimDir = Vector3.zero;
        }
        else if ((TouchManager.Instance.GetVirtualJoysticksActive() || InputManager.LastUsedDevice == Gamepad.current) && 
                 SettingsManager.Instance.useInstantJoystickAim && joystickInputMagnitude > joystickDeadzone)
        {
            //Debug.Log("Player: Joysticks are active, using fastJoystickAim");
            // Instant Joystick Aim
            lastAimDir = (Vector3)(joystickInput.normalized * scaledDistance);
            aimWorldPos = transform.position + lastAimDir;
        }
        else if (InputManager.GetJoystickAsMouseState() && 
                 CursorManager.Instance.usingCustomCursor && 
                 !SettingsManager.Instance.useInstantJoystickAim)
        {
            //Debug.Log("Player: Joysticks are actice, not using fastJoystickAim, aiming to lastAimDir");
            // Use custom cursor position directly for aiming
            aimWorldPos = transform.position + lastAimDir;
            //lastAimDir = Vector3.zero; // Reset last joystick aim direction
        }
        else
        {
            //Debug.Log("Player: Joysticks are NOT actice, aiming to cursorPos");
            aimWorldPos = CursorManager.Instance.GetCustomCursorWorldPos();
            //lastAimDir = Vector3.zero;

            if (lastAimDir != Vector3.zero)
            {
                // For Instant Joystick Aim lock
                aimWorldPos = transform.position + lastAimDir;
            }
        }

        // Aim or virtual mouse for joystick
        //if (!InputManager.GetJoystickAsMouseState())
            CursorManager.Instance.MoveCustomCursorWorldToUi(aimWorldPos);

        if ((transform.position - aimWorldPos).magnitude > 
            (transform.position - muzzleTrans.position).magnitude + 0.5f)
        {
            muzzleDirToMouse = aimWorldPos - muzzleTrans.transform.position;
        }
        else
        {
            muzzleDirToMouse = aimWorldPos - transform.position;
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

        RotateHead(aimWorldPos);
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
        if (InputManager.LastUsedDevice == Touchscreen.current)
            rawInput = TouchManager.Instance.moveJoystick.JoystickOutput;

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
        moveDir *= speed * slowFactor * sprintSpeedAmount;

        myRb.AddForce(moveDir);
    }

    public void ApplySlowFactor(float factor)
    {
        slowFactor = Mathf.Clamp(slowFactor - factor, 0.3f, 1);
    }

    public void RemoveSlowFactor(float factor)
    {
        slowFactor = Mathf.Clamp(slowFactor + factor, 0.3f, 1);
    }

    #endregion
}
