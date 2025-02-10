using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class QCubeController : MonoBehaviour
{
    [SerializeField] GameObject tutorialUpgradePanelPrefab;
    [SerializeField] RectTransform gamePlayCanvas;

    [Header("Death")]
    [SerializeField] TextMeshProUGUI deathTitleText;
    [SerializeField] TextMeshProUGUI deathSubtitleText;
    [SerializeField] List<string> cubeDeathTitles;
    [SerializeField] List<string> cubeDeathSubtitles;
    static bool _isCubeDestroyed; // Backing field
    public static event Action<bool> OnCubeDestroyed;
    public static bool IsCubeDestroyed
    {
        get { return _isCubeDestroyed; }
        set
        {
            if (_isCubeDestroyed != value)
            {
                _isCubeDestroyed = value;
                OnCubeDestroyed?.Invoke(_isCubeDestroyed);
            }
        }
    }
   
    [Header("Interaction")]
    public PanelSwitcher panelSwitcher;
    [SerializeField] float interactRadius = 1.5f;
    [SerializeField] float leaveRadius = 2f;
    public bool isUpgradesOpen;
    [SerializeField] PauseMenuController pauseMenuController;
    [SerializeField] BuildingSystem buildingSystem;
    [SerializeField] Player player;
    public DroneHangar droneHangar;

    InputActionMap playerActionMap;
    InputAction qCubeAction;
    InputAction fireAction;
    InputActionMap uIActionMap;
    InputAction escapeAction;

    void Awake()
    {
        IsCubeDestroyed = false;
        playerActionMap = player.inputAsset.FindActionMap("Player");
        qCubeAction = playerActionMap.FindAction("QCube");
        fireAction = playerActionMap.FindAction("Fire");

        uIActionMap = player.inputAsset.FindActionMap("UI");
        escapeAction = uIActionMap.FindAction("Escape");
    }

    void OnEnable()
    {  
        qCubeAction.performed += OnQCubeInteract;
        escapeAction.started += OnEscapePressed;
    }

    void OnDisable()
    {
        qCubeAction.performed -= OnQCubeInteract;
        escapeAction.started -= OnEscapePressed;
    }

    void FixedUpdate()
    {
        if (isUpgradesOpen)
        {
            // Changed this to only have one circle check, instead of a "leaving circle"
            Collider2D col = Physics2D.OverlapCircle(
                transform.position, interactRadius, LayerMask.GetMask("Player"));
            if (!col)
            {
                CloseUpgradeWindow();
            }
        }
    }

    void OnEscapePressed(InputAction.CallbackContext context)
    {
        if (TutorialManager.NumTutorialsOpen > 0)
            return;

        if (isUpgradesOpen)
        {
            StartCoroutine(WaitToCheckForPause());
        }
    }

    IEnumerator WaitToCheckForPause()
    {
        yield return new WaitForSecondsRealtime(0.05f);

        CloseUpgradeWindow();
    }

    void OnQCubeInteract(InputAction.CallbackContext context)
    {
        if (player.playerShooter.isBuilding)
        {
            player.animControllerPlayer.ToggleBuildMode();
        }

        // Look for player's main collider
        Collider2D[] playerColliders = Physics2D.OverlapCircleAll(
            transform.position, interactRadius, LayerMask.GetMask("Player"));
        foreach (Collider2D col in playerColliders)
        {
            if (col != null && col.CompareTag("Player") )
            {
                //Debug.Log("Player in QCube Range");

                // Open or close menu
                if (!isUpgradesOpen)
                {
                    StartCoroutine(OpenUpgradeWindow());
                    return;
                }
                else
                {
                    CloseUpgradeWindow();
                    return;
                }
            }
        }

        if (!isUpgradesOpen)
            {
                MessagePanel.PulseMessage("Not close enough to access the " +
                                        "Q-Cube. Get closer!", Color.yellow);
                // Player is not in range, show UI message to the player
                //Debug.Log("Player NOT in QCube Range!");
            }
            else
            {
                CloseUpgradeWindow();
                return;
            }
    }

    IEnumerator OpenUpgradeWindow()
    {
        //if (!TutorialManager.TutorialShowedUpgrade && !TutorialManager.TutorialDisabled)
        //{
            //StartCoroutine(DelayTutorialOpen());
        //}
        yield return new WaitForFixedUpdate();
        
        if (InputController.LastUsedDevice == Gamepad.current)
            InputController.SetJoystickMouseControl(true);

        fireAction.Disable();
        panelSwitcher.isTriggered = true;
        isUpgradesOpen = true;
    }

    public void CloseUpgradeWindow()
    {
        InputController.SetJoystickMouseControl(false);
        //Debug.Log("CloseUpgradeWindow ran");
        fireAction.Enable();
        panelSwitcher.isTriggered = false;
        isUpgradesOpen = false;

        StopAllCoroutines();
    }

    public void RandomizeDeathMessages()
    {
        int randomIndex = UnityEngine.Random.Range(0, cubeDeathTitles.Count);
        deathTitleText.text = cubeDeathTitles[randomIndex];

        randomIndex = UnityEngine.Random.Range(0, cubeDeathSubtitles.Count);
        deathSubtitleText.text = cubeDeathSubtitles[randomIndex];
    }   

    IEnumerator DelayTutorialOpen()
    {
        yield return new WaitForSeconds(0.2f);

        Instantiate(tutorialUpgradePanelPrefab, gamePlayCanvas);
    }
}
