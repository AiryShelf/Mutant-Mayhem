using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsPanel : MonoBehaviour
{
    public FadeCanvasGroupsWave fadeGroup;
    [SerializeField] TMP_Dropdown movementTypeDropdown;
    [SerializeField] Toggle fastJoystickAimToggle;
    [SerializeField] Slider joystickCursorSpeedSlider;
    [SerializeField] TMP_Dropdown qualityDropdown;
    [SerializeField] Toggle vSyncToggle;
    [SerializeField] Toggle spacebarToggle;
    [SerializeField] Toggle tutorialToggle;

    QualityManager qualityManager;

    void OnEnable()
    {
        // CHANGED: Use method groups matching UnityEvent signatures.
        tutorialToggle.onValueChanged.AddListener(ToggleTutorial);
        movementTypeDropdown.onValueChanged.AddListener(MoveTypeValueChanged);
        fastJoystickAimToggle.onValueChanged.AddListener(FastJoystickAimToggle);
        joystickCursorSpeedSlider.onValueChanged.AddListener(JoystickCursorSpeedChanged);
        qualityDropdown.onValueChanged.AddListener(QualityValueChanged);
        spacebarToggle.onValueChanged.AddListener(ToggleSpacebar);
        vSyncToggle.onValueChanged.AddListener(ToggleVSync);

        //Initialize();
    }

    void OnDisable()
    {
        // CHANGED: Remove listeners using the same method groups.
        tutorialToggle.onValueChanged.RemoveListener(ToggleTutorial);
        movementTypeDropdown.onValueChanged.RemoveListener(MoveTypeValueChanged);
        fastJoystickAimToggle.onValueChanged.RemoveListener(FastJoystickAimToggle);
        joystickCursorSpeedSlider.onValueChanged.RemoveListener(JoystickCursorSpeedChanged);
        qualityDropdown.onValueChanged.RemoveListener(QualityValueChanged);
        spacebarToggle.onValueChanged.RemoveListener(ToggleSpacebar);
        vSyncToggle.onValueChanged.RemoveListener(ToggleVSync);
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
        if (ProfileManager.Instance.currentProfile != null)
        {
            PlayerProfile profile = ProfileManager.Instance.currentProfile;

            movementTypeDropdown.SetValueWithoutNotify(profile.isStandardWASD ? 1 : 0);
            tutorialToggle.SetIsOnWithoutNotify(profile.isTutorialEnabled);
            spacebarToggle.SetIsOnWithoutNotify(profile.isSpacebarEnabled);
            fastJoystickAimToggle.SetIsOnWithoutNotify(profile.isFastJoystickAimEnabled);

            float sliderValue = Mathf.InverseLerp(
                CursorManager.Instance.cursorSpeedMin,
                CursorManager.Instance.cursorSpeedMax,
                profile.joystickCursorSpeed
            );
            joystickCursorSpeedSlider.SetValueWithoutNotify(sliderValue);
        }
        else
        {
            movementTypeDropdown.SetValueWithoutNotify(1); // Default to standard WASD
            tutorialToggle.SetIsOnWithoutNotify(true);
            spacebarToggle.SetIsOnWithoutNotify(true);
            fastJoystickAimToggle.SetIsOnWithoutNotify(false);
            joystickCursorSpeedSlider.SetValueWithoutNotify(0.5f);
        }

        qualityDropdown.SetValueWithoutNotify(QualitySettings.GetQualityLevel());
        vSyncToggle.SetIsOnWithoutNotify(QualitySettings.vSyncCount > 0);
    }

    // CHANGED: Now accepts a bool instead of a Toggle
    public void ToggleTutorial(bool isOn) // CHANGED: Parameter type updated
    {
        TutorialManager.SetTutorialState(isOn);
    }

    // CHANGED: Now accepts an int (the new value) instead of a TMP_Dropdown
    void MoveTypeValueChanged(int value) // CHANGED: Parameter type updated
    {
        PlayerProfile profile = ProfileManager.Instance.currentProfile;
        switch (value)
        {
            case 0:
                profile.isStandardWASD = false;
                break;
            case 1:
                profile.isStandardWASD = true;
                break;
            default:
                Debug.LogError("Failed to change move type");
                break;
        }

        ProfileManager.Instance.SaveCurrentProfile();
        SettingsManager.Instance.RefreshSettingsFromProfile(profile);

        Debug.Log("useStandardWASD set to: " + value);
    }

    // CHANGED: Now accepts a float (the slider value) instead of a Slider
    void JoystickCursorSpeedChanged(float sliderValue) // CHANGED: Parameter type updated
    {
        float value = CursorManager.Instance.cursorSpeedFactor;
        if (ProfileManager.Instance.currentProfile != null)
        {
            value = Mathf.Lerp(CursorManager.Instance.cursorSpeedMin, CursorManager.Instance.cursorSpeedMax, sliderValue);
            ProfileManager.Instance.currentProfile.joystickCursorSpeed = value;
            ProfileManager.Instance.SaveCurrentProfile();
        }

        SettingsManager.Instance.RefreshSettingsFromProfile(ProfileManager.Instance.currentProfile);
        Debug.Log("Joystick Cursor Speed set to " + value);
    }

    // CHANGED: Now accepts a bool instead of a Toggle
    void FastJoystickAimToggle(bool isOn) // CHANGED: Parameter type updated
    {
        if (ProfileManager.Instance.currentProfile != null)
        {
            ProfileManager.Instance.currentProfile.isFastJoystickAimEnabled = isOn;
            ProfileManager.Instance.SaveCurrentProfile();
        }

        SettingsManager.Instance.RefreshSettingsFromProfile(ProfileManager.Instance.currentProfile);
        Debug.Log("Fast Joystick Aim set to " + isOn);
    }

    // CHANGED: Now accepts an int instead of a TMP_Dropdown
    void QualityValueChanged(int qualityLevel) // CHANGED: Parameter type updated
    {
        qualityManager.SetGraphicsQuality(qualityLevel);
        Initialize();
    }

    // CHANGED: Now accepts a bool instead of a Toggle
    void ToggleVSync(bool isOn) // CHANGED: Parameter type updated
    {
        qualityManager.SetVSync(isOn);
    }

    // CHANGED: Now accepts a bool instead of a Toggle
    public void ToggleSpacebar(bool isOn) // CHANGED: Parameter type updated
    {
        if (ProfileManager.Instance.currentProfile != null)
        {
            ProfileManager.Instance.currentProfile.isSpacebarEnabled = isOn;
            ProfileManager.Instance.SaveCurrentProfile();
        }

        SettingsManager.Instance.RefreshSettingsFromProfile(ProfileManager.Instance.currentProfile);
        Debug.Log("Spacebar throws grenades set to " + isOn);
    }
}







/*
public class OptionsPanel : MonoBehaviour
{
    public FadeCanvasGroupsWave fadeGroup;
    [SerializeField] TMP_Dropdown movementTypeDropdown;
    [SerializeField] Toggle fastJoystickAimToggle;
    [SerializeField] Slider joystickCursorSpeedSlider;
    [SerializeField] TMP_Dropdown qualityDropdown;
    [SerializeField] Toggle vSyncToggle;
    [SerializeField] Toggle spacebarToggle;
    [SerializeField] Toggle tutorialToggle;

    QualityManager qualityManager;

    void OnEnable()
    {
        tutorialToggle.onValueChanged.AddListener(delegate {
                                            ToggleTutorial(tutorialToggle); });
        movementTypeDropdown.onValueChanged.AddListener(delegate { 
                                            MoveTypeValueChanged(movementTypeDropdown); });
        fastJoystickAimToggle.onValueChanged.AddListener(delegate {
                                            FastJoystickAimToggle(fastJoystickAimToggle); });
        joystickCursorSpeedSlider.onValueChanged.AddListener(delegate {
                                            JoystickCursorSpeedChanged(joystickCursorSpeedSlider); });
        qualityDropdown.onValueChanged.AddListener(delegate { 
                                            QualityValueChanged(qualityDropdown); });
        spacebarToggle.onValueChanged.AddListener(delegate {
                                            ToggleSpacebar(spacebarToggle); });
        vSyncToggle.onValueChanged.AddListener(delegate {
                                            ToggleVSync(vSyncToggle); });

        Initialize();
    }

    void OnDisable()
    {
        tutorialToggle.onValueChanged.RemoveListener(delegate {
                                            ToggleTutorial(tutorialToggle); });
        movementTypeDropdown.onValueChanged.RemoveListener(delegate { 
                                            MoveTypeValueChanged(movementTypeDropdown); });
        fastJoystickAimToggle.onValueChanged.RemoveListener(delegate {
                                            FastJoystickAimToggle(fastJoystickAimToggle); });
        joystickCursorSpeedSlider.onValueChanged.RemoveListener(delegate {
                                            JoystickCursorSpeedChanged(joystickCursorSpeedSlider); });
        qualityDropdown.onValueChanged.RemoveListener(delegate { 
                                            QualityValueChanged(qualityDropdown); });
        spacebarToggle.onValueChanged.RemoveListener(delegate {
                                            ToggleSpacebar(spacebarToggle); });
        vSyncToggle.onValueChanged.RemoveListener(delegate {
                                            ToggleVSync(vSyncToggle); });
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
        // Check if a current profile exists, set options values
        if (ProfileManager.Instance.currentProfile != null)
        {
            PlayerProfile profile = ProfileManager.Instance.currentProfile;

            movementTypeDropdown.SetValueWithoutNotify(profile.isStandardWASD ? 1 : 0);
            tutorialToggle.SetIsOnWithoutNotify(profile.isTutorialEnabled);
            spacebarToggle.SetIsOnWithoutNotify(profile.isSpacebarEnabled);
            fastJoystickAimToggle.SetIsOnWithoutNotify(profile.isFastJoystickAimEnabled);

            float sliderValue = Mathf.InverseLerp(
                CursorManager.Instance.cursorSpeedMin,
                CursorManager.Instance.cursorSpeedMax,
                profile.joystickCursorSpeed
            );
            joystickCursorSpeedSlider.SetValueWithoutNotify(sliderValue);
        }
        else
        {
            movementTypeDropdown.SetValueWithoutNotify(1); // Default to standard WASD
            tutorialToggle.SetIsOnWithoutNotify(true);
            spacebarToggle.SetIsOnWithoutNotify(true);
            fastJoystickAimToggle.SetIsOnWithoutNotify(false);
            joystickCursorSpeedSlider.SetValueWithoutNotify(0.5f);
        }

        qualityDropdown.SetValueWithoutNotify(QualitySettings.GetQualityLevel());
        vSyncToggle.SetIsOnWithoutNotify(QualitySettings.vSyncCount > 0);
    }

    public void ToggleSpacebar(Toggle change)
    {
        if (ProfileManager.Instance.currentProfile != null)
        {
            ProfileManager.Instance.currentProfile.isSpacebarEnabled = change.isOn;
            ProfileManager.Instance.SaveCurrentProfile();
        }

        SettingsManager.Instance.RefreshSettingsFromProfile(ProfileManager.Instance.currentProfile);
        Debug.Log("Spacebar throws grenades set to " + change.isOn);
    }

    public void ToggleTutorial(bool change)
    {
        TutorialManager.SetTutorialState(change);
    }

    void MoveTypeValueChanged(TMP_Dropdown change)
    {
        PlayerProfile profile = ProfileManager.Instance.currentProfile;
        switch (change.value)
        {
            case 0:
                profile.isStandardWASD = false;
                break;
            case 1:
                profile.isStandardWASD = true;
                break;
            default:
                Debug.LogError("Failed to change move type");
                break;
        }

        ProfileManager.Instance.SaveCurrentProfile();
        SettingsManager.Instance.RefreshSettingsFromProfile(profile);

        Debug.Log("useStandardWASD set to: " + change.value);
    }

    void JoystickCursorSpeedChanged(Slider change)
    {
        float value = CursorManager.Instance.cursorSpeedFactor;
        if (ProfileManager.Instance.currentProfile != null)
        {
            value = Mathf.Lerp(CursorManager.Instance.cursorSpeedMin, CursorManager.Instance.cursorSpeedMax, change.value);
            ProfileManager.Instance.currentProfile.joystickCursorSpeed = value;
            ProfileManager.Instance.SaveCurrentProfile();
        }

        SettingsManager.Instance.RefreshSettingsFromProfile(ProfileManager.Instance.currentProfile);
        Debug.Log("Joystick Cursor Speed set to " + value);
    }

    void FastJoystickAimToggle(Toggle change)
    {
        if (ProfileManager.Instance.currentProfile != null)
        {
            ProfileManager.Instance.currentProfile.isFastJoystickAimEnabled = change.isOn;
            ProfileManager.Instance.SaveCurrentProfile();
        }

        SettingsManager.Instance.RefreshSettingsFromProfile(ProfileManager.Instance.currentProfile);
        Debug.Log("Fast Joystick Aim set to " + change.isOn);
    }

    void QualityValueChanged(TMP_Dropdown change)
    {
        qualityManager.SetGraphicsQuality(change.value);
        Initialize();
    }

    void ToggleVSync(Toggle change)
    {
        qualityManager.SetVSync(change.isOn);
    }
}

*/