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
    [SerializeField] FadeCanvasGroupsWave optionsFadeGroup;
    [SerializeField] FadeCanvasGroupsWave profileFadeGroup;
    [SerializeField] TextMeshProUGUI currentProfileText;
    [SerializeField] Button playButton;
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

    void Update()
    {
        // Update current profile text
        PlayerProfile currentProfile = ProfileManager.Instance.currentProfile;

        if (currentProfile != null && !string.IsNullOrEmpty(currentProfile.profileName))
        {
            currentProfileText.text = "Current Profile: " + currentProfile.profileName;
            currentProfileText.color = Color.green;
            playButton.interactable = true;
        }
        else
        {
            currentProfileText.text = "Create a profile before playing!";
            currentProfileText.color = Color.red;
            playButton.interactable = false;
        }
    }

    public void OnStartGame()
    {
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
            optionsFadeGroup.isTriggered = true;
            isOptionsOpen = true;
        }
        else
        {
            // Close options panel
            optionsFadeGroup.isTriggered = false;
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
        if (profileSelectionUI.isAreYouSurePanelOpen)
            profileSelectionUI.OnCancelDeleteProfile();
        else
            ToggleProfiles();
    }
}
