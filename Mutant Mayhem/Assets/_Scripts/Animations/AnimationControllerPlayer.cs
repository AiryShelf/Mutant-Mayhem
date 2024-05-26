using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationControllerPlayer : MonoBehaviour
{
    [SerializeField] float animSpeedFactor = 1f;
    [SerializeField] float feetReturnSpeed = 0.03f;
    [SerializeField] float lowerWeaponTime = 2f;
    [SerializeField] BuildingSystem buildingSystemController;


    public Animator bodyAnim;
    public Animator feetAnim;
    [SerializeField] float speedForNotMoving = 0.2f;
    Player player;
    Rigidbody2D playerRb;
    PlayerShooter playerShooter;
    Coroutine waitToLowerWeaponCoroutine;

    bool isFireInput;
    bool isThrowInput;
    bool hasMeleeStamina;
    bool meleeAnimPlaying;
    bool throwAnimPlaying;
    bool switchGunsAnimPlaying;
    bool reloadAnimPlaying;
    bool hasDied;
    float bodyAnimStartSpeed;

    InputActionMap actionMap;
    InputAction fireAction;
    InputAction moveAction;
    InputAction buildAction;
    InputAction meleeAction;
    InputAction throwAction;
    InputAction reloadAction;
    

    void Start()
    {
        player = FindObjectOfType<Player>();
        playerRb = player.GetComponent<Rigidbody2D>();
        playerShooter = player.GetComponent<PlayerShooter>(); 

        actionMap = player.inputAsset.FindActionMap("Player");
        fireAction = actionMap.FindAction("Fire");
        moveAction = actionMap.FindAction("Move");
        buildAction = actionMap.FindAction("BuildMenu");
        meleeAction = actionMap.FindAction("Melee");
        throwAction = actionMap.FindAction("Throw");
        reloadAction = actionMap.FindAction("Reload");       

        // Lambda expression seperates the input parameters 
        // on left from the lambda body on the right.
        // It allows to subscribe methods which contain parameters.
        buildAction.performed += ctx => ToggleBuildMode();
        moveAction.performed += ctx => IsMoveInput(true);
        moveAction.canceled += ctx => IsMoveInput(false);
        meleeAction.performed += ctx => IsMeleeInput(true);
        meleeAction.canceled += ctx => IsMeleeInput(false);
        fireAction.performed += ctx => IsFireInput(true);
        fireAction.canceled += ctx => IsFireInput(false);
        throwAction.performed += ctx => IsThrowInput(true);
        throwAction.canceled += ctx => IsThrowInput(false);
        reloadAction.performed += ctx => IsReloadInput();

        bodyAnimStartSpeed = bodyAnim.speed;
    }

    void OnDisable()
    {
        buildAction.performed -= ctx => ToggleBuildMode();
        moveAction.performed -= ctx => IsMoveInput(true);
        moveAction.canceled -= ctx => IsMoveInput(false);
        meleeAction.performed -= ctx => IsMeleeInput(true);
        meleeAction.canceled -= ctx => IsMeleeInput(false);
        fireAction.performed -= ctx => IsFireInput(true);
        fireAction.canceled -= ctx => IsFireInput(false);
        throwAction.performed -= ctx => IsThrowInput(true);
        throwAction.canceled -= ctx => IsThrowInput(false);
        reloadAction.performed -= ctx => IsReloadInput();
    }

    void Update()
    {
        // Inserted below
        //AnimatorStateInfo bodyState = bodyAnim.GetCurrentAnimatorStateInfo(0);
        //float bodyNormalizedTime = bodyState.normalizedTime;
        //feetAnim.Play("Idle_Walk_Run BLEND TREE", 0, bodyNormalizedTime);
    }

    void FixedUpdate()
    {
        if (player.isDead)
        {
            if (!hasDied)
            {
                bodyAnim.SetTrigger("isDead");
                feetAnim.SetBool("isDead", true);
                feetAnim.transform.rotation = bodyAnim.transform.rotation;
                bodyAnim.speed = bodyAnimStartSpeed;
                feetAnim.speed = bodyAnimStartSpeed;
                hasDied = true;
            }
            
        }
        else
            UpdatePlayerStates();
    }

    void UpdatePlayerStates()
    {
        if (player.grenadeAmmo < 1 && isThrowInput)
        {
            bodyAnim.SetBool("isThrowing", false);
            if (waitToLowerWeaponCoroutine != null)
            {
                StopCoroutine(waitToLowerWeaponCoroutine);
                waitToLowerWeaponCoroutine = null;
            }
        }

        hasMeleeStamina = bodyAnim.GetBool("hasMeleeStamina");
        bool isBuilding = bodyAnim.GetBool("isBuilding");
        bool isMeleeing;
        
        if (!hasMeleeStamina) 
        {
            bodyAnim.SetBool("isMeleeing", false);
            isMeleeing = false;
        }
        else
        {
            isMeleeing = bodyAnim.GetBool("isMeleeing");
        }

        if (isBuilding)
        {
            bodyAnim.SetBool("isAiming", false);
        }
        
        float speed = playerRb.velocity.magnitude;
        feetAnim.speed = speed * animSpeedFactor * Time.deltaTime;
        float normalizedSpeed = speed / player.moveSpeed;

        feetAnim.SetFloat("BlendSpeed", normalizedSpeed);
        
        bodyAnim.SetFloat("BlendSpeed", normalizedSpeed);

        // Check if shooting
        if (isFireInput && !isBuilding)
        {
            playerShooter.isShooting = true;
        }
        else
        {
            playerShooter.isShooting = false;
        }

        bool isMotion = false;
        // Check if not moving   
        if (speed <= speedForNotMoving)
        {
            isMotion = false;

            if (waitToLowerWeaponCoroutine == null && playerShooter.isShooting == false && 
                                              !isBuilding && !isMeleeing)
            {
                waitToLowerWeaponCoroutine = StartCoroutine(WaitToLowerWeapon());
            }
            else if (playerShooter.isShooting == true && waitToLowerWeaponCoroutine != null)
            {
                StopCoroutine(waitToLowerWeaponCoroutine);
                waitToLowerWeaponCoroutine = null;
            }

            // Lerp feet back to forward-facing direction
            feetAnim.transform.rotation = Quaternion.Lerp(feetAnim.transform.rotation, 
                                          bodyAnim.transform.rotation, feetReturnSpeed);
        }
        // if moving
        else
        {
            isMotion = true;

            if (waitToLowerWeaponCoroutine != null)
            {
                StopCoroutine(waitToLowerWeaponCoroutine);
                waitToLowerWeaponCoroutine = null;
            }

            if (!isBuilding && !isMeleeing)
                bodyAnim.SetBool("isAiming", true);

            // Rotate feet to movement direction
            float angle = Mathf.Atan2(playerRb.velocity.y, playerRb.velocity.x) * Mathf.Rad2Deg;
            feetAnim.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Set body Animation Speed
        if (isMotion && !meleeAnimPlaying && !isMeleeing && !throwAnimPlaying && 
            !switchGunsAnimPlaying && !reloadAnimPlaying)
        {
            bodyAnim.speed = speed * animSpeedFactor * Time.deltaTime;
            AnimatorStateInfo bodyState = bodyAnim.GetCurrentAnimatorStateInfo(0);
            float bodyNormalizedTime = bodyState.normalizedTime;
            feetAnim.Play("Idle_Walk_Run BLEND TREE", 0, bodyNormalizedTime);
        }
        else
        {
            bodyAnim.speed = bodyAnimStartSpeed;
        }

        // Set extra gun collider
        if ((!isMotion && !bodyAnim.GetBool("isAiming")) || isBuilding)
        {
            player.gunCollider.enabled = false;
        }
        else
        {
            player.gunCollider.enabled = true;
        }
    }

    void ToggleBuildMode()
    {
        if (!playerShooter.isBuilding)
        {
            buildingSystemController.ToggleBuildMenu(true);
            playerShooter.isBuilding = true;
            bodyAnim.SetBool("isBuilding", true);
            bodyAnim.SetBool("isAiming", false);
            bodyAnim.SetBool("isMeleeing", false);
            bodyAnim.SetBool("isThrowing", false);
            
            if (waitToLowerWeaponCoroutine != null)
            {
                StopCoroutine(waitToLowerWeaponCoroutine);
                waitToLowerWeaponCoroutine = null;
            }  
        }
        else
        {
            buildingSystemController.ToggleBuildMenu(false);
            playerShooter.isBuilding = false;
            bodyAnim.SetBool("isBuilding", false);
            bodyAnim.SetBool("isAiming", true);
        }
    }

    IEnumerator WaitToLowerWeapon()
    {
        yield return new WaitForSeconds(lowerWeaponTime);
        bodyAnim.SetBool("isAiming", false);
        waitToLowerWeaponCoroutine = null;
    }

    // THERES A BUNCH OF STUFF COLLAPSED IN THESE REGIONS
    #region Animations

    public void MeleeAnimationPlaying(bool playing)
    {
        if (playing)
        {
            meleeAnimPlaying = true;
        }

        else
        {
            meleeAnimPlaying = false;
        }
    }

    public void ThrowAnimationPlaying(bool playing)
    {
        if (playing)
        {
            throwAnimPlaying = true;
        }
        else
        {
            throwAnimPlaying = false;
        }
    }

    public void SwitchGunsAnimationPlaying(bool playing)
    {
        if (playing)
        {
            //Debug.Log("SwithcGuns called in animControllerPlayer");
            playerShooter.isSwitchingGuns = true;
            switchGunsAnimPlaying = true;
        }
        else
        {
            playerShooter.isSwitchingGuns = false;
            switchGunsAnimPlaying = false;
            bodyAnim.SetBool("isSwitchingGuns", false);
            bodyAnim.SetBool("isAiming", true);
        }
    }

    public void ReloadAnimationPlaying(bool playing)
    {
        if (playing)
        {
            //Debug.Log("RelodAnim called");
            reloadAnimPlaying = true;
            bodyAnim.SetBool("isReloading", false);
            playerShooter.isReloading = true;
        }
        else
        {
            reloadAnimPlaying = false;
            bodyAnim.SetBool("isAiming", true);
            playerShooter.isReloading = false;
            if (waitToLowerWeaponCoroutine != null)
            {
                StopCoroutine(waitToLowerWeaponCoroutine);
                waitToLowerWeaponCoroutine = null;
            }
        }
    }

    public void DeathAnimEnd()
    {       
        bodyAnim.enabled = false;
        feetAnim.enabled = false;
    }
    #endregion Animations



    #region Inputs

    public void IsMoveInput(bool yes)
    {
        if (yes)
        {
            if (waitToLowerWeaponCoroutine != null)
            {
                StopCoroutine(waitToLowerWeaponCoroutine);
                waitToLowerWeaponCoroutine = null;
            }
        }
    }

    void IsFireInput(bool pressed)
    {
        if (pressed)
        {
            if (playerShooter.isBuilding)
                ToggleBuildMode();
            //bodyAnim.SetBool("isBuilding", false);
            bodyAnim.SetBool("isAiming", true);
            isFireInput = true;
        }
        else
        {
            isFireInput = false;
        }
    }

    void IsMeleeInput(bool yes)
    {
        if (yes && hasMeleeStamina && !throwAnimPlaying)
        {
            bodyAnim.SetBool("isMeleeing", true);
            if (waitToLowerWeaponCoroutine != null)
            {
                StopCoroutine(waitToLowerWeaponCoroutine);
                waitToLowerWeaponCoroutine = null;
            }
        }
        else
        {
            bodyAnim.SetBool("isMeleeing", false);
            bodyAnim.SetBool("isAiming", true);
            if (waitToLowerWeaponCoroutine == null)
            {
                waitToLowerWeaponCoroutine = StartCoroutine(WaitToLowerWeapon());
            }

        }
    }

    public void IsThrowInput(bool pressed)
    {
        
        if (pressed && !meleeAnimPlaying && player.grenadeAmmo > 0)
        {
            isThrowInput = true;
            bodyAnim.SetBool("isThrowing", true);
            if (waitToLowerWeaponCoroutine != null)
            {
                StopCoroutine(waitToLowerWeaponCoroutine);
                waitToLowerWeaponCoroutine = null;
            }
        }
        else
        {
            isThrowInput = false;
            bodyAnim.SetBool("isThrowing", false);
            bodyAnim.SetBool("isAiming", true);
            if (waitToLowerWeaponCoroutine == null)
            {
                waitToLowerWeaponCoroutine = StartCoroutine(WaitToLowerWeapon());
            }
        }
    }

    public void SwitchGunsStart(int index)
    {
        bodyAnim.SetBool("isSwitchingGuns", true);
        bodyAnim.SetInteger("gunIndex", index);

        if (waitToLowerWeaponCoroutine != null)
        {
            StopCoroutine(waitToLowerWeaponCoroutine);
            waitToLowerWeaponCoroutine = null;
        }
    }

    public void IsReloadInput()
    {
        if (!reloadAnimPlaying && playerShooter.gunAmmoTotals[playerShooter.currentGunIndex] > 0 &&
            playerShooter.currentGunIndex != 0)
        {
            bodyAnim.SetBool("isReloading", true);
            if (waitToLowerWeaponCoroutine != null)
            {
                StopCoroutine(waitToLowerWeaponCoroutine);
                waitToLowerWeaponCoroutine = null;
            }
        }
        else
        {
            // Play no ammo sound
        }
    }
    #endregion Inputs

}
