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
        tutorialToggle.onValueChanged.AddListener(delegate {
                                            ToggleTutorial(tutorialToggle); });
        movementTypeDropdown.onValueChanged.AddListener(delegate { 
                                            MoveTypeValueChanged(movementTypeDropdown); });
        fastJoystickAimToggle.onValueChanged.AddListener(delegate {
                                            FastJoystickAimChanged(fastJoystickAimToggle); });
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
                                            FastJoystickAimChanged(fastJoystickAimToggle); });
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

        qualityDropdown.value = QualitySettings.GetQualityLevel();
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

    public void ToggleTutorial(Toggle change)
    {
        TutorialManager.SetTutorialState(change.isOn);
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
        if (ProfileManager.Instance.currentProfile != null)
        {
            float value = Mathf.Lerp(CursorManager.Instance.cursorSpeedMin, CursorManager.Instance.cursorSpeedMax, change.value);
            ProfileManager.Instance.currentProfile.joystickCursorSpeed = value;
            ProfileManager.Instance.SaveCurrentProfile();
        }

        SettingsManager.Instance.RefreshSettingsFromProfile(ProfileManager.Instance.currentProfile);
        Debug.Log("Joystick Cursor Speed set to " + change.value);
    }

    void FastJoystickAimChanged(Toggle change)
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
