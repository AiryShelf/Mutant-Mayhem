using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] FadeCanvasGroupsWave mainMenuFadeGroup;
    [SerializeField] OptionsPanel optionsPanel;
    [SerializeField] ControlSettingsPanel controlsPanel;
    [SerializeField] FadeCanvasGroupsWave profileFadeGroup;
    [SerializeField] InputActionAsset inputAsset;
    [SerializeField] ProfileSelectionUI profileSelectionUI;
    [SerializeField] List<GraphicRaycaster> graphicRaycasters;

    InputAction escapeKeyPressed;
    bool isProfilesOpen;
    bool isOptionsOpen;
    bool isControlsOpen;

    void OnEnable() 
    {
        InputActionMap uiActionMap = inputAsset.FindActionMap("UI");
        uiActionMap.Enable();
        escapeKeyPressed = uiActionMap.FindAction("Escape");
        escapeKeyPressed.performed += EscapeKeyPressed;
    }

    void OnDisable()
    {
        escapeKeyPressed.performed -= EscapeKeyPressed;
        //InputController.SetJoystickMouseControl(false);
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        
        InputManager.SetJoystickMouseControl(true);
        CursorManager.Instance.inMenu = true;
        CursorManager.Instance.SetGraphicRaycasters(graphicRaycasters);
        TouchManager.Instance.SetVirtualJoysticksActive(false);
    }

    public void OnStartGame()
    {
        if (ProfileManager.Instance.currentProfile == null)
        {
            ToggleProfiles();
            return;
        }

        SceneManager.LoadScene(1);
    }

    public void OnAnimTriggerMenu()
    {
        mainMenuFadeGroup.isTriggered = true;
    }

    public void ToggleOptions()
    {
        if (!isOptionsOpen)
        {
            // Open options panel
            optionsPanel.fadeGroup.isTriggered = true;
            optionsPanel.Initialize();
            isOptionsOpen = true;
        }
        else
        {
            // Close options panel
            optionsPanel.fadeGroup.isTriggered = false;
            isOptionsOpen = false;
        }
    }

    public void ToggleControls()
    {
        if (!isControlsOpen)
        {
            // Open controls panel
            controlsPanel.fadeGroup.isTriggered = true;
            controlsPanel.Initialize();
            isControlsOpen = true;
        }
        else
        {
            // Close controls panel
            controlsPanel.fadeGroup.isTriggered = false;
            isControlsOpen = false;
        }
    }

    public void ToggleProfiles()
    {
        if (!isProfilesOpen)
        {
            // Open profiles panel
            profileFadeGroup.isTriggered = true;
            isProfilesOpen = true;
            profileSelectionUI.UpdateProfilePanel();
        }
        else
        {
            // Close profiles panel
            profileFadeGroup.isTriggered = false;
            isProfilesOpen = false;
        }
    }

    public void QuitGame()
    {
        //  If the editor is running, else if compiled quit.
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
            Application.Quit();
    }

    void EscapeKeyPressed(InputAction.CallbackContext context)
    {
        if (isControlsOpen)
            ToggleControls();
        if (isOptionsOpen)
            ToggleOptions();
        else if (profileSelectionUI.isAreYouSurePanelOpen)
            profileSelectionUI.OnCancelDeleteProfile();
        else if (isProfilesOpen)
            ToggleProfiles();
    }
}
