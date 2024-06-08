using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class PlayerStats
{
    public QCubeStats qCubeStats;

    [Header("Movement stats")]
    public float moveSpeed = 8;
    public float strafeSpeed = 5;
    public float sprintFactor = 1.5f;
    public float lookSpeed = 0.05f;

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

public class Player : MonoBehaviour
{
    public PlayerStats stats;

    [Header("Movement")]
    [SerializeField] float forceFactor = 1.5f;
    [SerializeField] float sprintStaminaUse = 0.1f;
    [SerializeField] float headTurnSpeed = 0.1f;

    [Header("Other")]
    public InputActionAsset inputAsset;
    [SerializeField] GameObject grenadePrefab;
    [SerializeField] Transform headImageTrans;
    [SerializeField] Transform playerMainTrans;
    [SerializeField] Transform muzzleTrans;
    public CapsuleCollider2D gunCollider;
    [SerializeField] Transform leftHandTrans;
    [SerializeField] AnimationControllerPlayer animControllerPlayer;
    [SerializeField] MeleeControllerPlayer meleeController;
    [SerializeField] PlayerShooter myShooter;
    
    
    float sprintSpeedAmount;
    float moveForce;
    float strafeForce;
    Vector2 rawInput;
    Vector2 dirToMouse;
    float angleToMouse;
    Rigidbody2D myRb;
    Stamina myStamina;
    PlayerShooter playerShooter;
    public bool isDead;  
    Throw itemToThrow;

    void Awake()
    {
        playerShooter = GetComponent<PlayerShooter>();
        myRb = GetComponent<Rigidbody2D>();
        myStamina = GetComponent<Stamina>();

        // Initialize stats
        stats.playerHealthScript = GetComponent<PlayerHealth>();
        meleeController.stats = stats;
        myStamina.stats = stats;
        myShooter.playerStats = stats;        
    }

    void Start()
    { 
        // Use these for force-based movements
        moveForce = stats.moveSpeed * myRb.mass * forceFactor;
        strafeForce = stats.strafeSpeed * myRb.mass * forceFactor;

        StartCoroutine(RefreshForceRepeat());
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

    void RefreshForce()
    {
        moveForce = stats.moveSpeed * myRb.mass * forceFactor;
        strafeForce = stats.strafeSpeed * myRb.mass * forceFactor;
    }

    IEnumerator RefreshForceRepeat()
    {
        while (true)
        {
            yield return new WaitForSeconds(2);

            // Update force values
            RefreshForce();
        }
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
    }

    // Triggered by anim
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
            dirToMouse = mousePos - muzzleTrans.transform.position;
        }
        else
        {
            dirToMouse = mousePos - transform.position;
        }

        dirToMouse.Normalize();
       
        // Convert mouseDir to rotation, add moveAccuracy, and convert back 
        angleToMouse = Mathf.Atan2(dirToMouse.y, dirToMouse.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angleToMouse);

        //Apply rotations       
        playerMainTrans.rotation = Quaternion.Lerp(
            playerMainTrans.rotation, targetRotation, stats.lookSpeed);
        headImageTrans.rotation = Quaternion.Lerp(
            headImageTrans.rotation, targetRotation, headTurnSpeed);

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
        // Move x moveSpeed forward but strafeSpeed backwards and sideways
        if (rawInput.y > 0)
        {
            moveDir = new Vector2 (rawInput.y * moveForce, 
                                   -rawInput.x * strafeForce) * sprintSpeedAmount;
        }
        else
        {
            moveDir = new Vector2 (rawInput.y * strafeForce, 
                                   -rawInput.x * strafeForce) * sprintSpeedAmount;
        }

        moveDir = Quaternion.Euler(0, 0, angleToMouse) * moveDir;
        myRb.AddForce(moveDir);
    }
}
