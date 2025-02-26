using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsPanel : MonoBehaviour
{
    public FadeCanvasGroupsWave fadeGroup;
    [SerializeField] TMP_Dropdown qualityDropdown;
    [SerializeField] TMP_Dropdown resolutionDropdown;
    [SerializeField] Toggle vSyncToggle;
    [SerializeField] Toggle tutorialToggle;
    [SerializeField] Toggle virtualAimStickToggle;

    QualityManager qualityManager;
    List<Resolution> validResolutions = new List<Resolution>();

    void OnEnable()
    {
        // CHANGED: Use method groups matching UnityEvent signatures.
        tutorialToggle.onValueChanged.AddListener(ToggleTutorial);
        qualityDropdown.onValueChanged.AddListener(QualityValueChanged);
        vSyncToggle.onValueChanged.AddListener(ToggleVSync);
        virtualAimStickToggle.onValueChanged.AddListener(DisableVirtualAimJoystick);
        //resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

        //Initialize();
    }

    void OnDisable()
    {
        // CHANGED: Remove listeners using the same method groups.
        tutorialToggle.onValueChanged.RemoveListener(ToggleTutorial);
        qualityDropdown.onValueChanged.RemoveListener(QualityValueChanged);
        vSyncToggle.onValueChanged.RemoveListener(ToggleVSync);
        virtualAimStickToggle.onValueChanged.RemoveListener(DisableVirtualAimJoystick);
        //resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
    }

    void Start()
    {
        qualityManager = SettingsManager.Instance.GetComponent<QualityManager>();
        if (qualityManager == null)
            Debug.LogError("OptionsPanel: Could not find QualityManager on SettingsManager object");

        Initialize();
    }

    public void Initialize()
    {
        // Check if a current profile exists, then set option values
        int qualityLevel = QualitySettings.GetQualityLevel();

        PlayerProfile currentProfile = ProfileManager.Instance.currentProfile;
        if (currentProfile != null)
        {
            qualityLevel = currentProfile.qualityLevel;
            if (qualityLevel == -1)
                qualityLevel = QualitySettings.GetQualityLevel();

            tutorialToggle.SetIsOnWithoutNotify(currentProfile.isTutorialEnabled);
            virtualAimStickToggle.SetIsOnWithoutNotify(currentProfile.virtualAimJoystickDisabled);
        }
        else
        {
            tutorialToggle.SetIsOnWithoutNotify(true);
            virtualAimStickToggle.SetIsOnWithoutNotify(true);
        }

        qualityDropdown.SetValueWithoutNotify(qualityLevel);
        vSyncToggle.SetIsOnWithoutNotify(QualitySettings.vSyncCount > 0);
        
        //PopulateResolutionDropdown();
    }

    public void ToggleTutorial(bool isOn)
    {
        TutorialManager.SetTutorialState(isOn);
    }

    void DisableVirtualAimJoystick(bool disabled)
    {
        PlayerProfile currentProfile = ProfileManager.Instance.currentProfile;
        if (currentProfile != null)
        {
            currentProfile.virtualAimJoystickDisabled = disabled;
            ProfileManager.Instance.SaveCurrentProfile();
            SettingsManager.Instance.RefreshSettingsFromProfile(currentProfile);
        }
    }

    #region Graphics

    void QualityValueChanged(int qualityLevel)
    {
        qualityManager.SetGraphicsQuality(qualityLevel);
        if (ProfileManager.Instance.currentProfile != null)
        {
            ProfileManager.Instance.currentProfile.qualityLevel = qualityLevel;
            ProfileManager.Instance.SaveCurrentProfile();
        }
        Initialize();
    }

    void ToggleVSync(bool isOn)
    {
        qualityManager.SetVSync(isOn);
    }

    void PopulateResolutionDropdown() // CHANGED
    {
        // Clear existing items
        resolutionDropdown.ClearOptions();

        // This fetches all the resolutions supported by the device
        Resolution[] allResolutions = Screen.resolutions;

        // For demonstration, let's store only 4 or 5 resolution settings
        // You can filter them however you like.
        // e.g., a few from the smallest to largest
        // Also, you might want to skip duplicates or certain aspect ratios.
        validResolutions.Clear();

        // Example approach: pick a handful of common aspect ratio / resolution combos
        // Or you can just pick the first 5 or so from allResolutions.
        // The below is purely an example—customize as you see fit.
        foreach (Resolution res in allResolutions)
        {
            // Add basic filter to reduce duplicates:
            // (Try storing only distinct widths & heights, skipping weird aspect ratios if desired.)
            if ((res.width == 1280 && res.height == 720) ||
                (res.width == 1600 && res.height == 900) ||
                (res.width == 1920 && res.height == 1080) ||
                (res.width == 2560 && res.height == 1440) ||
                (res.width == 3840 && res.height == 2160))
            {
                if (!validResolutions.Contains(res))
                {
                    validResolutions.Add(res);
                }
            }
        }

        // If we didn't find anything in that filter, as a fallback just keep the first few
        if (validResolutions.Count == 0)
        {
            int limit = Mathf.Min(5, allResolutions.Length);
            for (int i = 0; i < limit; i++)
            {
                validResolutions.Add(allResolutions[i]);
            }
        }

        // Build the list of option labels
        List<string> options = new List<string>();
        foreach (Resolution res in validResolutions)
        {
            options.Add(res.width + " x " + res.height);
        }

        resolutionDropdown.AddOptions(options);

        // Find the current resolution in our subset and set dropdown accordingly
        Resolution current = Screen.currentResolution;
        int currentIndex = validResolutions.FindIndex(r => r.width == current.width && r.height == current.height);
        if (currentIndex >= 0)
        {
            resolutionDropdown.SetValueWithoutNotify(currentIndex);
        }
        else
        {
            resolutionDropdown.SetValueWithoutNotify(0);
        }
    }

    // CHANGED: Add this method to handle resolution changes
    void OnResolutionChanged(int index) // CHANGED
    {
        // If your game uses full screen, use Screen.SetResolution(width, height, fullscreen).
        // For windowed, you can pass false for fullscreen:
        Resolution chosenRes = validResolutions[index];

        // FullScreenMode.ExclusiveFullScreen or FullScreenMode.FullScreenWindow can be set here if needed.
        Screen.SetResolution(chosenRes.width, chosenRes.height, true); 
    }

    #endregion
}