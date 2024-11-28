using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] FadeCanvasGroupsWave mainMenuFadeGroup;
    [SerializeField] OptionsPanel optionsPanel;
    [SerializeField] FadeCanvasGroupsWave profileFadeGroup;
    [SerializeField] InputActionAsset inputAsset;
    [SerializeField] ProfileSelectionUI profileSelectionUI;

    InputAction escapeKeyPressed;
    bool isOptionsOpen;
    bool isProfilesOpen;

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
        if (isOptionsOpen)
            ToggleOptions();
        else if (profileSelectionUI.isAreYouSurePanelOpen)
            profileSelectionUI.OnCancelDeleteProfile();
        else if (isProfilesOpen)
            ToggleProfiles();
    }
}
