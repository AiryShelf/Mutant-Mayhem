using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsPanel : MonoBehaviour
{
    public FadeCanvasGroupsWave fadeGroup;
    [SerializeField] TMP_Dropdown movementTypeDropdown;
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
        qualityDropdown.onValueChanged.AddListener(delegate { 
                                            QualityValueChanged(qualityDropdown); });
        spacebarToggle.onValueChanged.AddListener(delegate {
                                            ToggleSpacebar(spacebarToggle); });
        vSyncToggle.onValueChanged.AddListener(delegate {
                                            ToggleVSync(vSyncToggle); });
    }

    void OnDisable()
    {
        tutorialToggle.onValueChanged.RemoveListener(delegate {
                                            ToggleTutorial(tutorialToggle); });
        movementTypeDropdown.onValueChanged.RemoveListener(delegate { 
                                            MoveTypeValueChanged(movementTypeDropdown); });
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
        }
        else
        {
            movementTypeDropdown.SetValueWithoutNotify(1); // Default to standard WASD
            tutorialToggle.SetIsOnWithoutNotify(true);
            spacebarToggle.SetIsOnWithoutNotify(true);
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
