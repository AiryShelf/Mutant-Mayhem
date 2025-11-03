using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] CanvasGroup myCanvasGroup;
    [SerializeField] FadeCanvasGroupsWave fadeCanvasGroups;
    [SerializeField] OptionsPanel optionsPanel;
    [SerializeField] ControlSettingsPanel controlsPanel;

    Player player;
    QCubeController qCubeController;
    BuildingSystem buildingSystem;
    InputActionMap playerActionMap;
    InputActionMap uIActionMap;
    InputAction escapeAction;
    public bool isPauseMenuOpen = false;
    public bool isOptionsOpen = false;
    public bool isControlsOpen = false;
    bool wasMusicPlaying;
    bool wasRepairing = false;

    void Awake()
    {
        player = FindObjectOfType<Player>();
        qCubeController = FindObjectOfType<QCubeController>();
        buildingSystem = FindObjectOfType<BuildingSystem>();

        playerActionMap = player.inputAsset.FindActionMap("Player");

        uIActionMap = player.inputAsset.FindActionMap("UI");
        escapeAction = uIActionMap.FindAction("Escape");

        uIActionMap.Enable();
    }

    void OnEnable()
    {
        escapeAction.started += EscapePressed;
    }

    void OnDisable()
    {
        escapeAction.started -= EscapePressed;
    }

    void Start()
    {
        myCanvasGroup.blocksRaycasts = false;
    }

    public void PauseButtonPressed()
    {
        if (player.IsDead) return;

        if (UpgradePanelManager.Instance.isOpen)
            UpgradePanelManager.Instance.CloseAllPanels();
        if (buildingSystem.isInBuildMode)
            buildingSystem.ToggleBuildMenu();
            
        EscapePressed(new InputAction.CallbackContext());
    }

    void EscapePressed(InputAction.CallbackContext context)
    {
        //if (TutorialManager.NumTutorialsOpen > 0)
            //return;

        if (isOptionsOpen)
        {
            ToggleOptionsMenu();
            return;
        }

        if (isControlsOpen)
        {
            ToggleControlsMenu();
            return;
        }

        //Debug.Log("escape pressed");
        if (!player.IsDead && !buildingSystem.isInBuildMode 
            && !UpgradePanelManager.Instance.isOpen)
        {
            //Debug.Log("Pause passed checks");
            if (!isPauseMenuOpen)
            {
                OpenPauseMenu(true);
            }
            else
            {
                OpenPauseMenu(false);
            }
        }
    }

    void OpenPauseMenu(bool open)
    {
        if (open)
        {
            CursorManager.Instance.inMenu = true;
            TouchManager.Instance.SetVirtualJoysticksActive(false);
            InputManager.SetJoystickMouseControl(true);

            wasRepairing = player.stats.playerShooter.isRepairing;
            player.stats.playerShooter.isRepairing = false;
            playerActionMap.Disable();

            MessageManager.Instance.PauseMessage();

            wasMusicPlaying = !MusicManager.Instance.isPaused;
            if (wasMusicPlaying)
                MusicManager.Instance.PlayOrPausePressed();
        }
        else
        {
            CursorManager.Instance.inMenu = false;
            TouchManager.Instance.SetVirtualJoysticksActive(true);
            playerActionMap.Enable();

            if (wasRepairing)
                player.stats.playerShooter.isRepairing = true;
            else
                InputManager.SetJoystickMouseControl(!SettingsManager.Instance.useFastJoystickAim);
            
            MessageManager.Instance.UnPauseMessage();
            
            if (wasMusicPlaying)
                MusicManager.Instance.PlayOrPausePressed();
        }
    
        fadeCanvasGroups.isTriggered = open;
        myCanvasGroup.blocksRaycasts = open;
        TimeControl.Instance.PauseGame(open);
        isPauseMenuOpen = open;
        
        Debug.Log("Pause Menu open: " + open);
    }

    public void Continue()
    {
        OpenPauseMenu(false);
    }

    public void Restart()
    {
        TimeControl.Instance.PauseGame(false);
        SceneManager.LoadScene(2);
    }

    public void BackToShip()
    {
        TimeControl.Instance.PauseGame(false);
        SceneManager.LoadScene(1);
    }

    public void MainMenu()
    {
        TimeControl.Instance.PauseGame(false);
        SceneManager.LoadScene(0);
    }

    public void GiveUp()
    {
        OpenPauseMenu(false);
        player.IsDead = true;
    }

    public void QuitGame()
    {
        //  If the editor is running, stop.  Else if compiled, quit.
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
            Application.Quit();
    }

    public void ToggleOptionsMenu()
    {
        if (!isOptionsOpen)
        {
            isOptionsOpen = true;
            optionsPanel.Initialize();
            optionsPanel.fadeGroup.isTriggered = true;
            Debug.Log("Opened options menu");
        }
        else
        {
            isOptionsOpen = false;
            optionsPanel.fadeGroup.isTriggered = false;
            Debug.Log("Closed options menu");
        }
    }

    public void ToggleControlsMenu()
    {
        if (!isControlsOpen)
        {
            isControlsOpen = true;
            controlsPanel.Initialize();
            controlsPanel.fadeGroup.isTriggered = true;
            Debug.Log("Opened controls menu");
        }
        else
        {
            isControlsOpen = false;
            controlsPanel.fadeGroup.isTriggered = false;
            Debug.Log("Closed controls menu");
        }
    }
}
