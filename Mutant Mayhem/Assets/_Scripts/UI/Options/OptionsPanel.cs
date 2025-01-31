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
    [SerializeField] Toggle spacebarToggle;
    [SerializeField] Toggle tutorialToggle;

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
    }

    void Start()
    {
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
    }

    public void ToggleSpacebar(Toggle change)
    {
        if (ProfileManager.Instance.currentProfile != null)
        {
            ProfileManager.Instance.currentProfile.isSpacebarEnabled = change.isOn;
            ProfileManager.Instance.SaveCurrentProfile();
        }

        SettingsManager.Instance.RefreshSettingsFromProfile(ProfileManager.Instance.currentProfile);
        Debug.Log("Toggled Spacebar");
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
        if (QualitySettings.count <= change.value)
        {
            Debug.LogError($"Failed to set graphics quality! Index: {change.value} is out of range");
            return;
        }

        PlayerPrefs.SetInt("Graphics_Quality", change.value);
        PlayerPrefs.Save();

        QualitySettings.SetQualityLevel(change.value, true);

        Debug.Log("Graphics Quality set to: " + change.value);
    }
}
