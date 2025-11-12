using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] CanvasGroup mainCanvasGroup;
    [SerializeField] CanvasGroup creditsCanvasGroup;
    [SerializeField] FadeCanvasGroupsWave mainMenuFadeGroup;
    [SerializeField] FadeCanvasGroupsWave musicPlayerFadeGroup;
    [SerializeField] OptionsPanel optionsPanel;
    [SerializeField] ControlSettingsPanel controlsPanel;
    [SerializeField] FadeCanvasGroupsWave profileFadeGroup;
    [SerializeField] CreditsRoll creditsRoll;
    [SerializeField] InputActionAsset inputAsset;
    [SerializeField] ProfileSelectionUI profileSelectionUI;
    [SerializeField] List<GraphicRaycaster> graphicRaycasters;

    InputAction escapeKeyPressed;
    bool isProfilesOpen;
    bool isOptionsOpen;
    bool isControlsOpen;
    bool isCreditsOpen;

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
        StartCoroutine(DelayStartActions());
    }

    IEnumerator DelayStartActions()
    {
        yield return new WaitForFixedUpdate();

        if (InputManager.LastUsedDevice == Touchscreen.current)
        {
            CursorManager.Instance.MoveCustomCursorTo(Vector2.zero, CursorRangeType.Bounds, Vector2.zero, 0, new Rect(0, 0, 1, 1));
            CursorManager.Instance.SetCustomCursorVisible(false);
        }
    }

    public void OnStartGame()
    {
        if (ProfileManager.Instance.currentProfile == null)
        {
            ToggleProfiles();
            return;
        }

        SceneManager.LoadScene(2);
    }

    public void OnStartTutorial()
    {
        if (ProfileManager.Instance.currentProfile == null)
        {
            ToggleProfiles();
            return;
        }

        PlanetManager.Instance.SetTutorialPlanet();
        var hideGroups = new List<CanvasGroup> { mainCanvasGroup, creditsCanvasGroup };
        VideoPlayerManager.Instance.PlayTutorialVideo(2, hideGroups); // Loads game level after video
    }

    public void OnAnimTriggerMenu()
    {
        mainMenuFadeGroup.isTriggered = true;
        if (musicPlayerFadeGroup == null)
        {
            musicPlayerFadeGroup = UI_MusicPlayerPanel.Instance.GetComponent<FadeCanvasGroupsWave>();
        }
        musicPlayerFadeGroup.isTriggered = true;
    }

    public void ToggleOptions()
    {
        if (!isOptionsOpen)
        {
            // Open options panel
            optionsPanel.fadeGroup.isTriggered = true;
            optionsPanel.Initialize();
            isOptionsOpen = true;
            ScreenScaleChecker.InvokeAspectRatioChanged();
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
            ScreenScaleChecker.InvokeAspectRatioChanged();
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
            ScreenScaleChecker.InvokeAspectRatioChanged();
        }
        else
        {
            // Close profiles panel
            profileFadeGroup.isTriggered = false;
            isProfilesOpen = false;
        }
    }

    public void ToggleCredits()
    {
        if (!isCreditsOpen)
        {
            creditsRoll.RollCredits();
            isCreditsOpen = true;
        }
        else
        {
            creditsRoll.StopCredits();
            isCreditsOpen = false;
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
