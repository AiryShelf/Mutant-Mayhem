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
    [SerializeField] DeathCanvas deathCanvas;
    [SerializeField] TextMeshProUGUI deathTitleText;
    [SerializeField] TextMeshProUGUI deathSubtitleText;
    [SerializeField] List<string> deathTitles;
    [SerializeField] List<string> deathSubtitles;
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
    [SerializeField] PanelSwitcher panelSwitcher;
    [SerializeField] float interactRadius = 1.5f;
    [SerializeField] float leaveRadius = 2f;
    public bool isUpgradesOpen;
    [SerializeField] PauseMenuController pauseMenuController;
    [SerializeField] BuildingSystem buildingSystem;
    [SerializeField] Player player;

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
        if (IsCubeDestroyed && !player.IsDead)
        {
            // Implement different ending, animation of cube exploding or the like
            RandomizeDeathMessages();
            deathCanvas.TransitionToDeathPanel(true);
            player.IsDead = true;
        }

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
            else if (!isUpgradesOpen)
            {
                MessagePanel.ShowMessage("Not close enough to access " +
                                        "Cube. Get closer!", Color.black);
                // Player is not in range, show UI message to the player
                //Debug.Log("Player NOT in QCube Range!");
            }
            else
            {
                CloseUpgradeWindow();
                return;
            }
        }
    }

    IEnumerator OpenUpgradeWindow()
    {
        if (!TutorialManager.tutorialShowedUpgrade && !TutorialManager.TutorialDisabled)
        {
            StartCoroutine(DelayTutorialOpen());
        }
        yield return new WaitForFixedUpdate();
        
        fireAction.Disable();
        panelSwitcher.isTriggered = true;
        isUpgradesOpen = true;
    }

    public void CloseUpgradeWindow()
    {
        //Debug.Log("CloseUpgradeWindow ran");
        fireAction.Enable();
        panelSwitcher.isTriggered = false;
        isUpgradesOpen = false;

        StopAllCoroutines();
    }

    public void RandomizeDeathMessages()
    {
        int randomIndex = UnityEngine.Random.Range(0, deathTitles.Count);
        deathTitleText.text = deathTitles[randomIndex];

        randomIndex = UnityEngine.Random.Range(0, deathSubtitles.Count);
        deathSubtitleText.text = deathSubtitles[randomIndex];
    }   

    IEnumerator DelayTutorialOpen()
    {
        yield return new WaitForSeconds(0.5f);

        Instantiate(tutorialUpgradePanelPrefab, gamePlayCanvas);
    }
}
