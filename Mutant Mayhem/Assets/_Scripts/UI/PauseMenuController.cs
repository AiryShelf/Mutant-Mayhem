using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] UIBuildMenuController uIBuildMenuController;
    [SerializeField] FadeCanvasGroupsWave fadeCanvasGroups;

    Player player;
    QCubeController qCubeController;
    BuildingSystem buildingSystem;
    InputActionMap playerActionMap;
    InputActionMap uIActionMap;
    InputAction escapeAction;
    public bool isPaused;
    bool wasBuildPanelOpen;

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
        
    }

    void EscapePressed(InputAction.CallbackContext context)
    {
        Debug.Log("escape pressed");
        if (!player.isDead && !buildingSystem.inBuildMode 
            && !qCubeController.isUpgradesOpen)
        {
            Debug.Log("Pause passed checks");
            if (!isPaused)
                OpenPanel(true);
            else
                OpenPanel(false);
        }
    }

    public void OpenPanel(bool open)
    {
        if (open)
        {
            if (uIBuildMenuController.fadeCanvasGroups.isTriggered == true)
            {
                wasBuildPanelOpen = true;
                uIBuildMenuController.fadeCanvasGroups.isTriggered = false;
            }

            fadeCanvasGroups.isTriggered = true;
            Pause(true);
        }
        else
        {
            if (wasBuildPanelOpen)
            {
                wasBuildPanelOpen = false;
                uIBuildMenuController.fadeCanvasGroups.isTriggered = true;
            }

            fadeCanvasGroups.isTriggered = false;
            Pause(false);
        }
    }



    public void Continue()
    {
        OpenPanel(false);
    }

    public void Restart()
    {
        Pause(false);
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        //  If the editor is running, else if compiled quit.
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
            Application.Quit();
    }

    public void Pause(bool pause)
    {
        if (pause)
        {
            playerActionMap.Disable();
            Time.timeScale = 0;
            isPaused = true;
        }
        else
        {
            playerActionMap.Enable();
            Time.timeScale = 1;
            isPaused = false;
        }
    }
}
