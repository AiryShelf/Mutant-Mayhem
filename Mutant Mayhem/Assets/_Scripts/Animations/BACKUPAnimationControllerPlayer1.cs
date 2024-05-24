using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationControllerPlayer2 : MonoBehaviour
{
    [SerializeField] float animSpeedFactor = 1f;
    [SerializeField] float feetReturnSpeed = 0.03f;
    [SerializeField] float lowerWeaponTime = 2f;

    [SerializeField] Animator bodyAnim;
    [SerializeField] Animator feetAnim;
    [SerializeField] float speedForNotMoving = 0.2f;
    Player player;
    Rigidbody2D playerRb;
    PlayerShooter playerShooter;
    Coroutine waitToLowerWeaponCoroutine;
    bool isMoveInput;
    bool isBuilding;
    bool isMoving;
     public bool isMeleeing;
     public bool hasMeleeStamina;
    float bodyAnimStartSpeed;
    

    void Start()
    {
        player = FindObjectOfType<Player>();
        playerRb = player.GetComponent<Rigidbody2D>();
        playerShooter = player.GetComponent<PlayerShooter>();
        //feetAnim = transform.Find("Feet").GetComponent<Animator>();
        //feetAnim = GetComponentInChildren<Animator>(); 

        //player.inputAsset.actionMaps[0].actions[6].performed += OnToolbar;
        InputActionMap actionMap = player.inputAsset.FindActionMap("Player");
        InputAction fireAction = actionMap.FindAction("Fire");
        InputAction moveAction = actionMap.FindAction("Move");
        InputAction buildAction = actionMap.FindAction("BuildMenu");
        // Add my method to the action event
        fireAction.performed += OnFireAnim;
        buildAction.performed += ToggleBuildMode;

        // Lambda expression seperates the input parameters 
        // on left from the lambda body on the right
        // It allows to subscribe methods which contain parameters.
        moveAction.performed += ctx => OnMoveStart();
        moveAction.canceled += ctx => OnMoveEnd();
        bodyAnimStartSpeed = bodyAnim.speed;
    }

    void Update()
    {
        AnimatorStateInfo bodyState = bodyAnim.GetCurrentAnimatorStateInfo(0);
        float bodyNormalizedTime = bodyState.normalizedTime;
        feetAnim.Play("Idle_Walk_Run BLEND TREE", 0, bodyNormalizedTime);
    }

    void FixedUpdate()
    {
        UpdatePlayer();
    }

    void UpdatePlayer()
    {
        isBuilding = bodyAnim.GetBool("isBuilding");
        float speed = playerRb.velocity.magnitude;
        feetAnim.speed = speed * animSpeedFactor * Time.deltaTime;
        float normalizedSpeed = speed / player.moveSpeed;
        feetAnim.SetFloat("Speed", normalizedSpeed);
        if (!isMeleeing)
        {
            bodyAnim.SetFloat("Speed", normalizedSpeed);
        }

        // Check if not moving   
        if (speed <= speedForNotMoving && isMoveInput == false)
        {
            isMoving = false;
            bodyAnim.SetBool("isMoving", false);
            if (waitToLowerWeaponCoroutine == null && playerShooter.isShooting == false && 
                                              !isBuilding)
            {
                waitToLowerWeaponCoroutine = StartCoroutine(WaitToLowerWeapon());
            }
            if (isBuilding)
            {
                if (waitToLowerWeaponCoroutine != null)
                {
                    StopCoroutine(waitToLowerWeaponCoroutine);
                    waitToLowerWeaponCoroutine = null;
                }
                bodyAnim.SetBool("isAiming", false);
            }

            // Lerp feet back to forward-facing direction
            feetAnim.transform.rotation = Quaternion.Lerp(feetAnim.transform.rotation, 
                                          bodyAnim.transform.rotation, feetReturnSpeed);
        }
        // if moving
        else
        {
            if (!isBuilding)
                bodyAnim.SetBool("isAiming", true);
            isMoving = true;
            bodyAnim.SetBool("isMoving", true);

            // Rotate feet to movement direction
            float angle = Mathf.Atan2(playerRb.velocity.y, playerRb.velocity.x) * Mathf.Rad2Deg;
            feetAnim.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Set body Animation Speed
        if (isMoving && !isMeleeing)
        {
            bodyAnim.speed = speed * animSpeedFactor * Time.deltaTime;
        }
        else
        {
            bodyAnim.speed = bodyAnimStartSpeed;
        }
    }

    void OnFireAnim(InputAction.CallbackContext context)
    {
        if (!isBuilding)
        {
            bodyAnim.SetBool("isAiming", true);
            if (waitToLowerWeaponCoroutine != null)
            {
                StopCoroutine(waitToLowerWeaponCoroutine);
                waitToLowerWeaponCoroutine = null;
            }
        }
    }

    public void OnMoveStart()
    {
        if (waitToLowerWeaponCoroutine != null)
        {
            StopCoroutine(waitToLowerWeaponCoroutine);
            waitToLowerWeaponCoroutine = null;
        }
        isMoveInput = true;
    }

    void OnMoveEnd()
    {
        isMoveInput = false;
    }

    void ToggleBuildMode(InputAction.CallbackContext context)
    {
        if (!isBuilding)
        {
            bodyAnim.SetBool("isBuilding", true);
            isBuilding  = true;
            
            if (waitToLowerWeaponCoroutine != null)
            {
                StopCoroutine(waitToLowerWeaponCoroutine);
                waitToLowerWeaponCoroutine = null;
            }
            bodyAnim.SetBool("isAiming", false);
            //isMoving = false;
        }
        else
        {
            bodyAnim.SetBool("isBuilding", false);
            isBuilding = false;
        }
    }

    IEnumerator WaitToLowerWeapon()
    {
        yield return new WaitForSeconds(lowerWeaponTime);
        bodyAnim.SetBool("isAiming", false);
        isMoving = false;
        bodyAnim.SetBool("isMoving", false);
        waitToLowerWeaponCoroutine = null;
    }

    public bool MeleeingStarted()
    {
        if (!isBuilding && hasMeleeStamina)
        {
            bodyAnim.SetBool("isMeleeing", true);
            isMeleeing = true;
            bodyAnim.SetBool("isAiming", false);
            if (waitToLowerWeaponCoroutine != null)
            {
                StopCoroutine(waitToLowerWeaponCoroutine);
                waitToLowerWeaponCoroutine = null;
            }
            return true;
        }
        else
            return false;
    }

    public void StopMeleeing()
    {
        if (bodyAnim.GetBool("isMeleeing"))
        {
            bodyAnim.SetBool("isMeleeing", false);
            isMeleeing = false;
            bodyAnim.SetBool("isAiming", true);
            if (waitToLowerWeaponCoroutine != null)
            {
                StopCoroutine(waitToLowerWeaponCoroutine);
                waitToLowerWeaponCoroutine = null;
            }
        }
    }
}
