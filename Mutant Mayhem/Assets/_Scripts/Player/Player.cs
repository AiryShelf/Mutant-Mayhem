using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class PlayerStats
{
    public StructureStats structureStats;
    [HideInInspector] public Player player;

    [Header("Movement stats")]
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
    public float accuracy = 1f;
    public int grenadeAmmo = 12;
}

[System.Serializable]
public class StructureStats
{
    public QCubeHealth cubeHealthScript;
    public TileManager tileManager;
    public float structureMaxHealthMult = 1;
    public float armour = 0;
    public float maxTurrets = 0;
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
    
    float sprintSpeedAmount;
    Vector2 rawInput;
    Vector2 muzzleDirToMouse;
    float muzzleAngleToMouse;
    Rigidbody2D myRb;
    Stamina myStamina;
    public PlayerShooter playerShooter;
    public bool isDead;  
    Throw itemToThrow;
    [HideInInspector] public int movementType;
    float lastFootstepTime;
    float footstepCooldown = 0.1f;

    void Awake()
    {
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
    }

    void Start()
    {
        RefreshMoveForces();
        UpgradeManager.Instance.Initialize();
        TurretManager.Instance.Initialize();
    }

    void FixedUpdate()
    {
        if (!isDead)
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
            AudioManager.instance.PlaySoundAt(walkGrassSound, transform.position);
            lastFootstepTime = Time.time;
        }
    }

    public void RefreshMoveForces()
    {
        stats.moveForce = stats.moveSpeed * myRb.mass * forceFactor;
        stats.strafeForce = stats.strafeSpeed * myRb.mass * forceFactor;
        stats.maxVelocity = (stats.moveForce * stats.sprintFactor) / 
                            (myRb.drag * myRb.mass);
    }

    void OnToolbar()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (playerShooter.currentGunIndex != 0)
                animControllerPlayer.SwitchGunsStart(0);    
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (playerShooter.currentGunIndex != 1)
                animControllerPlayer.SwitchGunsStart(1); 
        }
        
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (playerShooter.currentGunIndex != 2)
                animControllerPlayer.SwitchGunsStart(2); 
        }
        
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (playerShooter.currentGunIndex != 3)
                animControllerPlayer.SwitchGunsStart(3); 
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            if (playerShooter.currentGunIndex != 4)
                animControllerPlayer.SwitchGunsStart(4); 
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            if (playerShooter.currentGunIndex != 5)
                animControllerPlayer.SwitchGunsStart(5); 
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            if (playerShooter.currentGunIndex != 6)
                animControllerPlayer.SwitchGunsStart(6); 
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            if (playerShooter.currentGunIndex != 7)
                animControllerPlayer.SwitchGunsStart(7); 
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            if (playerShooter.currentGunIndex != 8)
                animControllerPlayer.SwitchGunsStart(8); 
        }
        // REPAIR GUN
        else if (Input.GetKeyDown(KeyCode.Alpha0) ||
                 Input.GetKeyDown(KeyCode.C))
        {
            if (playerShooter.currentGunIndex != 9)
                animControllerPlayer.SwitchGunsStart(9); 
        }
    }

    public void OnThrowGrab()
    {
        itemToThrow = Instantiate(grenadePrefab, transform.position, 
                      Quaternion.identity, leftHandTrans).gameObject.GetComponent<Throw>();    
    }

    public void OnThrowFly()
    {
        itemToThrow.transform.parent = null;
        itemToThrow.StartFly();
        stats.grenadeAmmo--;
        StatsCounterPlayer.GrenadesThrownByPlayer++;
    }

    void OnMove(InputValue value)
    {
        rawInput = value.Get<Vector2>();    
    }

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

        Vector3 headDirToMouse = mousePos - headImageTrans.position;

        muzzleDirToMouse.Normalize();
       
        // Get muzzle angle and rotation
        muzzleAngleToMouse = Mathf.Atan2(muzzleDirToMouse.y, muzzleDirToMouse.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, muzzleAngleToMouse);
    
        // Get head angle
        float headAngleToMouse = Mathf.Atan2(headDirToMouse.y, headDirToMouse.x) * Mathf.Rad2Deg;

        // Apply body rotation 
        float rotationSpeed = stats.lookSpeed;
        playerMainTrans.rotation = Quaternion.Lerp(
            playerMainTrans.rotation, targetRotation, rotationSpeed);

        //playerMainTrans.rotation = Quaternion.Lerp(
            //playerMainTrans.rotation, targetRotation, stats.lookSpeed);

        // Calculate the difference between the body angle and the head angle
        float bodyAngle = playerMainTrans.eulerAngles.z;
        float relativeAngle = Mathf.DeltaAngle(bodyAngle, headAngleToMouse);

        // Clamp the relative angle to headTurnMax
        float clampedRelativeAngle = Mathf.Clamp(relativeAngle, -headTurnMax, headTurnMax);

        // Calculate the clamped target rotation for the head
        Quaternion clampedTargetRotation = Quaternion.Euler(0, 0, bodyAngle + clampedRelativeAngle);

        // Apply the clamped rotation to the head
        headImageTrans.rotation = Quaternion.Lerp(
        headImageTrans.rotation, clampedTargetRotation, headTurnSpeed);

        //headImageTrans.rotation = Quaternion.Lerp(
            //headImageTrans.rotation, targetRotation, headTurnSpeed);

        // ** TO ADD DRUNKEN BEHAVIOUR **
        //rotAngle += Random.Range(-moveAccuracy, moveAccuracy); 
        //float radians = rotAngle * Mathf.Deg2Rad;
        //mouseDir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        
        //myRb.rotation = Mathf.LerpAngle(myRb.rotation, 
            //angleToMouse, Time.deltaTime * lookSpeed);
    }

    void Sprint()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (sprintStaminaUse <= myStamina.GetStamina())
            {
                StatsCounterPlayer.TimeSprintingPlayer += Time.fixedDeltaTime;
                sprintSpeedAmount = stats.sprintFactor;
                myStamina.ModifyStamina(-sprintStaminaUse);
            }
            else
            {
                sprintSpeedAmount = 1;
            }
        }
        else
        {
            sprintSpeedAmount = 1;
        }
    }

    void Move()
    {
        Vector2 moveDir;

        if (movementType == 1)
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
}
