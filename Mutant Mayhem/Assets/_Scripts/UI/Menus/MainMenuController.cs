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
    [SerializeField] CanvasGroup fpsCounter;
    [SerializeField] List<CanvasGroup> titleGroupsToHide;
    [SerializeField] OptionsPanel optionsPanel;
    [SerializeField] ControlSettingsPanel controlsPanel;
    [SerializeField] FadeCanvasGroupsWave newProfileFadeGroup;
    [SerializeField] CreditsRoll creditsRoll;
    [SerializeField] InputActionAsset inputAsset;
    [SerializeField] ProfileSelectionUI profileSelectionUI;
    [SerializeField] List<GraphicRaycaster> graphicRaycasters;

    InputAction escapeKeyPressed;
    bool isProfilesOpen;
    bool isOptionsOpen;
    bool isControlsOpen;
    bool isCreditsOpen;
    LoadingPanel loadingPanel;
    bool playPressedWithNoProfile;
    bool tutorialPressedWithNoProfile;

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
        MessageManager.Instance.StopAllConversations();
        
        fpsCounter.alpha = 0f;
        foreach (var group in titleGroupsToHide)
        {
            group.alpha = 0f;
        }
        
        Application.targetFrameRate = 60;
        
        InputManager.SetJoystickMouseControl(true);
        CursorManager.Instance.inMenu = true;
        CursorManager.Instance.SetGraphicRaycasters(graphicRaycasters);
        TouchManager.Instance.ShowVirtualJoysticks(false);
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
            playPressedWithNoProfile = true;
            ToggleNewProfilePanel();
            return;
        }

        if (AnalyticsManager.ConsentStatus == AnalyticsConsentStatus.Unknown)
        {
            AnalyticsManager.Instance.OpenPermissionPanel(
                () => {
                    AnalyticsManager.Instance.GrantConsent();
                    SceneManager.LoadSceneAsync(2);
                },
                () => {
                    AnalyticsManager.Instance.DenyConsent();
                    SceneManager.LoadSceneAsync(2);
                }
            );
            return;
        }

        SceneManager.LoadSceneAsync(2);
    }

    public void OnStartTutorial()
    {
        if (ProfileManager.Instance.currentProfile == null)
        {
            tutorialPressedWithNoProfile = true;
            ToggleNewProfilePanel();
            return;
        }

        if (AnalyticsManager.ConsentStatus == AnalyticsConsentStatus.Unknown)
        {
            AnalyticsManager.Instance.OpenPermissionPanel(
                () => {
                    AnalyticsManager.Instance.GrantConsent();
                    StartTutorial();
                },
                () => {
                    AnalyticsManager.Instance.DenyConsent();
                    StartTutorial();
                }
            );
            return;
        }

        StartTutorial();
    }

    public IEnumerator LoadSceneCoroutine(int sceneIndex)
    {
        if (loadingPanel == null)
        {
            loadingPanel = FindObjectOfType<LoadingPanel>();
        }
        loadingPanel.canvasGroup.alpha = 1f;
        mainCanvasGroup.alpha = 0f;
        creditsCanvasGroup.alpha = 0f;

        // Wait one frame to ensure loading canvas groups update
        // force update canvases
        yield return null;

        SceneManager.LoadSceneAsync(sceneIndex);
    }

    void StartTutorial()
    {
        PlanetManager.Instance.SetTutorialPlanet();
        var hideGroups = new List<CanvasGroup> { mainCanvasGroup, creditsCanvasGroup };
        VideoPlayerManager.Instance.PlayTutorialVideo(3, hideGroups); // Loads game level after video
    }

    public void OnAnimTriggerMenu()
    {
        mainMenuFadeGroup.isTriggered = true;
        if (musicPlayerFadeGroup == null)
        {
            musicPlayerFadeGroup = UI_MusicPlayerPanel.Instance.GetComponent<FadeCanvasGroupsWave>();
        }

        if (fpsCounter == null)
        {
            fpsCounter = FpsCounter.Instance.GetComponent<CanvasGroup>();
        }
        fpsCounter.alpha = 1f;

        foreach (var group in titleGroupsToHide)
        {
            group.alpha = 1f;
        }

        musicPlayerFadeGroup.isTriggered = true;
    }

    public void ToggleOptions()
    {
        if (!isOptionsOpen)
        {
            // Open options panel
            optionsPanel.fadeGroup.isTriggered = true;
            optionsPanel.InitializeUI();
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

    // Also called by button press
    public void ToggleNewProfilePanel()
    {
        Debug.Log("Toggle New Profile Panel");
        if (profileSelectionUI.justClickedCreateProfile)
        {
            Debug.Log("Ignored Toggle New Profile Panel due to just clicked create profile");
            profileSelectionUI.justClickedCreateProfile = false;
            return;
        }

        if (!isProfilesOpen)
        {
            Debug.Log("New Profile Open Profiles Panel started");
            // Open profiles panel
            newProfileFadeGroup.isTriggered = true;
            isProfilesOpen = true;
            profileSelectionUI.playPressedWithNoProfile = playPressedWithNoProfile;
            profileSelectionUI.tutorialPressedWithNoProfile = tutorialPressedWithNoProfile;
            profileSelectionUI.UpdateProfilePanel();
            profileSelectionUI.FocusNameInput();
            ScreenScaleChecker.InvokeAspectRatioChanged();
            Debug.Log("New Profile Open Profiles Panel finished");
        }
        else
        {
            Debug.Log("New Profile Close Profiles Panel started");
            // Close profiles panel
            playPressedWithNoProfile = false;
            tutorialPressedWithNoProfile = false;
            profileSelectionUI.playPressedWithNoProfile = false;
            profileSelectionUI.tutorialPressedWithNoProfile = false;
            newProfileFadeGroup.isTriggered = false;
            isProfilesOpen = false;
            Debug.Log("New Profile Close Profiles Panel finished");
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
            ToggleNewProfilePanel();
    }
}
