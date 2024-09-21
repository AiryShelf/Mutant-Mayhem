using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    [SerializeField] TextMeshProUGUI currentProfileText;

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
            
            // Update current profile text
            string profileName = ProfileManager.Instance.currentProfile.profileName;
            bool nameExists = !string.IsNullOrEmpty(profileName);
            if (nameExists)
            {
                currentProfileText.text = "Current Profile: " + profileName;
                currentProfileText.color = Color.green;
            }
            else
            {
                currentProfileText.text = "WARNING no profile found. Progress can't be saved!";
                currentProfileText.color = Color.red;
                Debug.LogError("No profile found on gameplay pause menu!");
            }
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
        SceneManager.LoadScene(2);
    }

    public void BackToShip()
    {
        Pause(false);
        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        Pause(false);
        SceneManager.LoadScene(0);
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
