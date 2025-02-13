using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] CanvasGroup myCanvasGroup;
    [SerializeField] FadeCanvasGroupsWave fadeCanvasGroups;
    [SerializeField] OptionsPanel optionsPanel;

    Player player;
    QCubeController qCubeController;
    BuildingSystem buildingSystem;
    InputActionMap playerActionMap;
    InputActionMap uIActionMap;
    InputAction escapeAction;
    public bool isPauseMenuOpen = false;
    public bool isOptionsOpen = false;
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
        Debug.Log("Pause Menu Enabled");
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

    void EscapePressed(InputAction.CallbackContext context)
    {
        if (TutorialManager.NumTutorialsOpen > 0)
            return;

        if (isOptionsOpen)
        {
            ToggleOptionsMenu();
            return;
        }

        //Debug.Log("escape pressed");
        if (!player.IsDead && !buildingSystem.isInBuildMode 
            && !qCubeController.isUpgradesOpen)
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
            wasRepairing = player.stats.playerShooter.isRepairing;
            player.stats.playerShooter.isRepairing = false;

            playerActionMap.Disable();
            InputController.SetJoystickMouseControl(true);

            wasMusicPlaying = !MusicManager.Instance.isPaused;
            if (wasMusicPlaying)
                MusicManager.Instance.PlayOrPausePressed();
        }
        else
        {
            if (wasRepairing)
                player.stats.playerShooter.isRepairing = true;
            else
                InputController.SetJoystickMouseControl(false);

            playerActionMap.Enable();
            
            
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
}
