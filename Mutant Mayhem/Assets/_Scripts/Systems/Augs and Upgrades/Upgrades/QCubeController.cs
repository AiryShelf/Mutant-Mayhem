using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class QCubeController : MonoBehaviour
{
    public static QCubeController Instance;

    [SerializeField] CameraController cameraController;

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

    [Header("Upgrade Panels")]
    public UiUpgradePanel lasersPanel;
    public UiUpgradePanel bulletsPanel;
    public UiUpgradePanel structuresPanel;
    public UiUpgradePanel repairPanel;
    public UiUpgradePanel explosivesPanel;
    public UiUpgradePanel dronesPanel;
   
    [Header("Interaction")]
    public PanelSwitcher panelSwitcher;
    public RectTransform backPanel;
    [SerializeField] float interactRadius = 1.5f;
    public bool isUpgradesOpen;
    [SerializeField] BuildingSystem buildingSystem;
    [SerializeField] Player player;
    public DroneHangar droneHangar;

    bool wasRepairing = false;
    InputActionMap playerActionMap;
    InputAction qCubeAction;
    InputAction fireAction;
    InputAction throwAction;
    InputAction toolbarAction;
    InputActionMap uIActionMap;
    InputAction escapeAction;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } 
        else
        {
            Destroy(gameObject);
            return;
        }

        IsCubeDestroyed = false;
        playerActionMap = player.inputAsset.FindActionMap("Player");
        qCubeAction = playerActionMap.FindAction("QCube");
        fireAction = playerActionMap.FindAction("Fire");
        throwAction = playerActionMap.FindAction("Throw");
        toolbarAction = playerActionMap.FindAction("Toolbar");

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

    public void UnlockUpgradePanel(StructureSO structure, bool playEffect)
    {
        switch (structure.panelToUnlock)
        {
            case UpgradePanelType.Lasers:
                lasersPanel.OnUnlocked(structure, playEffect);
                break;
            case UpgradePanelType.Bullets:
                bulletsPanel.OnUnlocked(structure, playEffect);
                break;
            case UpgradePanelType.Structures:
                structuresPanel.OnUnlocked(structure, playEffect);
                break;
            case UpgradePanelType.Repair:
                repairPanel.OnUnlocked(structure, playEffect);
                break;
            case UpgradePanelType.Explosives:
                explosivesPanel.OnUnlocked(structure, playEffect);
                break;
            case UpgradePanelType.Drones:
                dronesPanel.OnUnlocked(structure, playEffect);
                break;
        }
    }

    public void LockUpgradePanel(StructureSO structure, bool playEffect)
    {
        switch (structure.panelToUnlock)
        {
            case UpgradePanelType.Lasers:
                lasersPanel.OnLocked(structure, playEffect);
                break;
            case UpgradePanelType.Bullets:
                bulletsPanel.OnLocked(structure, playEffect);
                break;
            case UpgradePanelType.Structures:
                structuresPanel.OnLocked(structure, playEffect);
                break;
            case UpgradePanelType.Repair:
                repairPanel.OnLocked(structure, playEffect);
                break;
            case UpgradePanelType.Explosives:
                explosivesPanel.OnLocked(structure, playEffect);
                break;
            case UpgradePanelType.Drones:
                dronesPanel.OnLocked(structure, playEffect);
                break;
        }
    }

    void OnEscapePressed(InputAction.CallbackContext context)
    {
        if (PanelManager.NumPanelsOpen > 0)
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
        TryInteract();
    }

    public void TryInteract()
    {
        // Look for player's main collider
        Collider2D[] playerColliders = Physics2D.OverlapCircleAll(
            transform.position, interactRadius, LayerMask.GetMask("Player"));
        foreach (Collider2D col in playerColliders)
        {
            if (col != null && col.CompareTag("Player") )
            {
                //Debug.Log("Player in QCube Range");

                // Open or close menu
                if (!isUpgradesOpen && !CursorManager.Instance.inMenu)
                {
                    StartCoroutine(OpenUpgradeWindow());
                    if (player.playerShooter.isBuilding)
                    {
                        player.animControllerPlayer.ToggleBuildMode();
                    }
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
        yield return new WaitForFixedUpdate();
        
        wasRepairing = player.stats.playerShooter.isRepairing;
        player.stats.playerShooter.isRepairing = false;

        if (InputManager.LastUsedDevice == Touchscreen.current)
            CursorManager.Instance.SetCustomCursorVisible(false);

        InputManager.SetJoystickMouseControl(true);
        CursorManager.Instance.inMenu = true;
        cameraController.ZoomAndFocus(player.transform, 0, 0.25f, 0.35f, true, false);
        cameraController.SetTouchscreenOffset(false);
        droneHangar.ShowRangeCircle(true);

        player.animControllerPlayer.FireInput_Cancelled(new InputAction.CallbackContext());
        fireAction.Disable();
        throwAction.Disable();
        if (InputManager.LastUsedDevice == Gamepad.current)
            toolbarAction.Disable();
        panelSwitcher.isTriggered = true;
        isUpgradesOpen = true;
    }

    public void CloseUpgradeWindow()
    {
        if (player.stats.playerShooter.currentGunIndex == 4) // Repair Gun
            player.stats.playerShooter.isRepairing = true;
        else
            InputManager.SetJoystickMouseControl(!SettingsManager.Instance.useFastJoystickAim);

        CursorManager.Instance.inMenu = false;
        if (!player.stats.playerShooter.isBuilding && !player.stats.playerShooter.isRepairing)
        {
            cameraController.ZoomAndFocus(player.transform, 0, 1, 1f, false, false);
            cameraController.SetTouchscreenOffset(true);
        }
        else 
            buildingSystem.LockCameraToPlayer(true);

        CursorManager.Instance.SetCustomCursorVisible(true);
        droneHangar.ShowRangeCircle(false);

        //Debug.Log("CloseUpgradeWindow ran");
        fireAction.Enable();
        throwAction.Enable();
        toolbarAction.Enable();
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
}
