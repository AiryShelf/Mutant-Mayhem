using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class AnimationControllerPlayer : MonoBehaviour
{
    [SerializeField] float animSpeedFactor = 1f;
    [SerializeField] float feetReturnSpeed = 0.03f;
    [SerializeField] float lowerWeaponTime = 2f;
    [SerializeField] BuildingSystem buildingSystemController;
    [HideInInspector] public Animator bodyAnim;
    [HideInInspector] public Animator legsAnim;
    [SerializeField] float speedForNotMoving = 0.2f;

    Player player;
    Rigidbody2D playerRb;
    PlayerShooter playerShooter;
    Coroutine waitToLowerWeaponCoroutine;
    MessagePanel messagePanel;

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

    InputActionMap uIActionMap;
    InputAction escapeAction;
    

    void OnEnable()
    {
        player = FindObjectOfType<Player>();
        playerRb = player.GetComponent<Rigidbody2D>();
        playerShooter = player.GetComponent<PlayerShooter>(); 
        bodyAnim = GameObject.FindGameObjectWithTag("PlayerBody").GetComponent<Animator>();
        legsAnim = GameObject.FindGameObjectWithTag("PlayerLegs").GetComponent<Animator>();
        messagePanel = FindObjectOfType<MessagePanel>();
        
        actionMap = player.inputAsset.FindActionMap("Player");
        fireAction = actionMap.FindAction("Fire");
        moveAction = actionMap.FindAction("Move");
        buildAction = actionMap.FindAction("BuildMenu");
        meleeAction = actionMap.FindAction("Melee");
        throwAction = actionMap.FindAction("Throw");
        reloadAction = actionMap.FindAction("Reload"); 

        uIActionMap = player.inputAsset.FindActionMap("UI");
        escapeAction = uIActionMap.FindAction("Escape"); 

        // Reset
        //SceneManager.sceneLoaded -= OnSceneLoaded;
            

        //SceneManager.sceneLoaded += OnSceneLoaded;
        // Lambda expression seperates the input parameters 
        // on left from the lambda body on the right.
        // It allows to subscribe methods which contain parameters.
        // moveAction.performed += ctx => IsMoveInput(true);

        // AAAnnd then I found out I couldn't get the lambda abstract
        // method to go away so switch to named methods
        buildAction.performed += BuildInput_Toggle;
        //buildAction.canceled += BuildInput_Toggle;
        moveAction.performed += MoveInput_Performed;
        // moveAction.canceled += ctx => 
        meleeAction.performed += MeleeInput_Performed;
        meleeAction.canceled += MeleeInput_Cancelled;
        fireAction.performed += FireInput_Performed;
        fireAction.canceled += FireInput_Cancelled;
        throwAction.performed += ThrowInput_Performed;
        throwAction.canceled += ThrowInput_Cancelled;
        reloadAction.performed += IsReloadInput;

        escapeAction.started += OnEscapePressed;

        bodyAnimStartSpeed = bodyAnim.speed;
    }

    void OnDisable()
    {
        //SceneManager.sceneLoaded -= OnSceneLoaded;

        buildAction.performed -= BuildInput_Toggle;
        //buildAction.canceled -= BuildInput_Toggle;
        moveAction.performed -= MoveInput_Performed;
        //moveAction.canceled -= Move;
        meleeAction.performed -= MeleeInput_Performed;
        meleeAction.canceled -= MeleeInput_Cancelled;
        fireAction.performed -= FireInput_Performed;
        fireAction.canceled -= FireInput_Cancelled;
        throwAction.performed -= ThrowInput_Performed;
        throwAction.canceled -= ThrowInput_Cancelled;
        reloadAction.performed -= IsReloadInput;

        escapeAction.started -= OnEscapePressed;
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
                legsAnim.SetBool("isDead", true);
                legsAnim.transform.rotation = bodyAnim.transform.rotation;
                bodyAnim.speed = bodyAnimStartSpeed;
                legsAnim.speed = bodyAnimStartSpeed;
                hasDied = true;
            }
            
        }
        else
            UpdatePlayerStates();
    }

    void UpdatePlayerStates()
    {
        // Stop trowing when out of grenades
        if (player.stats.grenadeAmmo < 1 && isThrowInput)
        {
            bodyAnim.SetBool("isThrowing", false);
            if (waitToLowerWeaponCoroutine != null)
            {
                StopCoroutine(waitToLowerWeaponCoroutine);
                waitToLowerWeaponCoroutine = null;
            }
        }

        // Initial checks
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
        
        // Legs anim speed
        float speed = playerRb.velocity.magnitude;
        legsAnim.speed = speed * animSpeedFactor * Time.fixedDeltaTime;
        float normalizedSpeed = speed / player.stats.moveSpeed;

        legsAnim.SetFloat("BlendSpeed", normalizedSpeed);
        
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

        // Check if not moving 
        bool isMotion = false; 
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
            legsAnim.transform.rotation = Quaternion.Lerp(legsAnim.transform.rotation, 
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
            legsAnim.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Set body Animation Speed
        if (isMotion && !meleeAnimPlaying && !isMeleeing && !throwAnimPlaying && 
            !switchGunsAnimPlaying && !reloadAnimPlaying)
        {
            bodyAnim.speed = speed * animSpeedFactor * Time.fixedDeltaTime;
            AnimatorStateInfo bodyState = bodyAnim.GetCurrentAnimatorStateInfo(0);
            float bodyNormalizedTime = bodyState.normalizedTime;
            legsAnim.Play("Idle_Walk_Run BLEND TREE", 0, bodyNormalizedTime);
        }
        else
        {
            bodyAnim.speed = bodyAnimStartSpeed;
        }

        // Apply reload speed upgrade multiplier
        AnimatorStateInfo state = bodyAnim.GetCurrentAnimatorStateInfo(0);
        if (state.IsTag("Reload"))
        {
            bodyAnim.speed = animSpeedFactor * Time.fixedDeltaTime
                             * player.stats.reloadFactor;
        }

        // Apply melee attack speed upgrade multiplier, might not use this
        /*
        if (state.IsTag("Melee"))
        {
            bodyAnim.speed = animSpeedFactor * Time.deltaTime
                             * player.stats.meleeSpeedFactor;
        }
        */

        // Set extra gun collider for walls
        if ((!isMotion && !bodyAnim.GetBool("isAiming")) || isBuilding)
        {
            player.gunCollider.enabled = false;
        }
        else
        {
            player.gunCollider.enabled = true;
        }
    }

    public void ToggleBuildMode()
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

    #region Animations

    public void MeleeAnimationPlaying(bool playing)
    {
        if (playing)
        {
            meleeAnimPlaying = true;
            playerShooter.isMeleeing = true;
        }

        else
        {
            meleeAnimPlaying = false;
            playerShooter.isMeleeing = false;
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
        legsAnim.enabled = false;
    }
    #endregion Animations

    #region Inputs

    void OnEscapePressed(InputAction.CallbackContext context)
    {
        if (playerShooter.isBuilding)
        {
            StartCoroutine(WaitToCheckForPause());
        }
    }

    IEnumerator WaitToCheckForPause()
    {
        yield return new WaitForSecondsRealtime(0.05f);

        ToggleBuildMode();
    }

    public void BuildInput_Toggle(InputAction.CallbackContext context)
    {
        ToggleBuildMode();
    }

    public void MoveInput_Performed(InputAction.CallbackContext context)
    {    
        if (waitToLowerWeaponCoroutine != null)
        {
            StopCoroutine(waitToLowerWeaponCoroutine);
            waitToLowerWeaponCoroutine = null;
        }
    }

    void FireInput_Performed(InputAction.CallbackContext context)
    {
        if (playerShooter.isBuilding)
            ToggleBuildMode();
        bodyAnim.SetBool("isAiming", true);
        isFireInput = true;
    }
    void FireInput_Cancelled(InputAction.CallbackContext context)
    {   
        isFireInput = false;       
    }

    void MeleeInput_Performed(InputAction.CallbackContext context)
    {
        if (hasMeleeStamina && !throwAnimPlaying)
        {
            if (playerShooter.isBuilding)
                ToggleBuildMode();
            bodyAnim.SetBool("isMeleeing", true);
            if (waitToLowerWeaponCoroutine != null)
            {
                StopCoroutine(waitToLowerWeaponCoroutine);
                waitToLowerWeaponCoroutine = null;
            }
        }   
    }
    void MeleeInput_Cancelled(InputAction.CallbackContext context)
    {
        bodyAnim.SetBool("isMeleeing", false);
        bodyAnim.SetBool("isAiming", true);
        if (waitToLowerWeaponCoroutine == null)
        {
            waitToLowerWeaponCoroutine = StartCoroutine(WaitToLowerWeapon());
        }
    }

    public void ThrowInput_Performed(InputAction.CallbackContext context)
    {  
        if (!meleeAnimPlaying && player.stats.grenadeAmmo > 0)
        {
            if (playerShooter.isBuilding)
                ToggleBuildMode();
            isThrowInput = true;
            bodyAnim.SetBool("isThrowing", true);
            if (waitToLowerWeaponCoroutine != null)
            {
                StopCoroutine(waitToLowerWeaponCoroutine);
                waitToLowerWeaponCoroutine = null;
            }
        }
    }
    public void ThrowInput_Cancelled(InputAction.CallbackContext context)
    {       
        isThrowInput = false;
        bodyAnim.SetBool("isThrowing", false);
        bodyAnim.SetBool("isAiming", true);
        if (waitToLowerWeaponCoroutine == null)
        {
            waitToLowerWeaponCoroutine = StartCoroutine(WaitToLowerWeapon());
        }       
    }

    public void SwitchGunsStart(int index)
    {
        if (playerShooter.gunList[index] == null)
        {
            messagePanel.ShowMessage("Weapon not unlocked!", Color.yellow);
            return;
        }
        if (!playerShooter.gunsUnlocked[index])
        {
            messagePanel.ShowMessage("Weapon not unlocked!", Color.yellow);
            return;
        }

        bodyAnim.SetBool("isSwitchingGuns", true);
        bodyAnim.SetInteger("gunIndex", index);

        if (waitToLowerWeaponCoroutine != null)
        {
            StopCoroutine(waitToLowerWeaponCoroutine);
            waitToLowerWeaponCoroutine = null;
        }
    }

    public void IsReloadInput(InputAction.CallbackContext context)
    {
        ReloadTrigger();        
    }
    public void ReloadTrigger()
    {
        if (!reloadAnimPlaying && playerShooter.gunsAmmo[playerShooter.currentGunIndex] > 0 &&
            playerShooter.currentGunIndex != 0 && 
            playerShooter.gunsAmmoInClips[playerShooter.currentGunIndex] !=
            playerShooter.gunList[playerShooter.currentGunIndex].clipSize)
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
