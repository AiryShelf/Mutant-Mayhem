using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] UIBuildMenuController uIBuildMenuController;
    [SerializeField] CanvasGroup myCanvasGroup;
    [SerializeField] FadeCanvasGroupsWave fadeCanvasGroups;
    [SerializeField] FadeCanvasGroupsWave optionsFadeGroup;

    Player player;
    QCubeController qCubeController;
    BuildingSystem buildingSystem;
    InputActionMap playerActionMap;
    InputActionMap uIActionMap;
    InputAction escapeAction;
    public bool isPaused;
    public bool isOptionsOpen;

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

    void EscapePressed(InputAction.CallbackContext context)
    {
        if (isOptionsOpen)
            {
                ToggleOptionsMenu();
                return;
            }
        //Debug.Log("escape pressed");
        if (!player.IsDead && !buildingSystem.inBuildMode 
            && !qCubeController.isUpgradesOpen)
        {
            //Debug.Log("Pause passed checks");
            if (!isPaused)
                OpenPauseMenu(true);
            else
                OpenPauseMenu(false);
        }
    }

    public void OpenPauseMenu(bool open)
    {
        if (open)
        {
            fadeCanvasGroups.isTriggered = true;
            myCanvasGroup.blocksRaycasts = true;
            Pause(true);
        }
        else
        {
            fadeCanvasGroups.isTriggered = false;
            myCanvasGroup.blocksRaycasts = false;
            Pause(false);
        }
    }

    public void Continue()
    {
        OpenPauseMenu(false);
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
            EventSystem.current.SetSelectedGameObject(null);
            playerActionMap.Enable();
            Time.timeScale = 1;
            isPaused = false;
        }
    }

    public void ToggleOptionsMenu()
    {
        if (!isOptionsOpen)
        {
            isOptionsOpen = true;
            optionsFadeGroup.isTriggered = true;
        }
        else
        {
            isOptionsOpen = false;
            optionsFadeGroup.isTriggered = false;
        }
    }
}
