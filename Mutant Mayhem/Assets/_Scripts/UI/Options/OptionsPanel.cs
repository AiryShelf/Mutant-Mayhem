using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OptionsPanel : MonoBehaviour
{
    public FadeCanvasGroupsWave fadeGroup;
    [SerializeField] TMP_Dropdown movementTypeDropdown;
    [SerializeField] Toggle spacebarToggle;
    [SerializeField] Toggle tutorialToggle;

    void OnEnable()
    {
        // Add listeners
        tutorialToggle.onValueChanged.AddListener(delegate {
                                            ToggleTutorial(tutorialToggle); });
        movementTypeDropdown.onValueChanged.AddListener(delegate { 
                                            MoveTypeValueChanged(movementTypeDropdown); });
        spacebarToggle.onValueChanged.AddListener(delegate {
                                            ToggleSpacebar(spacebarToggle); });
    }

    void OnDisable()
    {
        // Add listeners
        tutorialToggle.onValueChanged.RemoveListener(delegate {
                                            ToggleTutorial(tutorialToggle); });
        movementTypeDropdown.onValueChanged.RemoveListener(delegate { 
                                            MoveTypeValueChanged(movementTypeDropdown); });
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

    void DifficultyValueChanged(TMP_Dropdown change)
    {
        if ((DifficultyLevel)change.value != ProfileManager.Instance.currentProfile.difficultyLevel)
        {
            switch (change.value)
            {
                case 0:
                    ProfileManager.Instance.currentProfile.difficultyLevel = DifficultyLevel.Easy;
                    break;
                case 1:
                    ProfileManager.Instance.currentProfile.difficultyLevel = DifficultyLevel.Normal;
                    break;
                case 2:
                    ProfileManager.Instance.currentProfile.difficultyLevel = DifficultyLevel.Hard;
                    break;
                default:
                    Debug.LogError("Failed to change difficulty");
                    return;
            }
        

            ProfileManager.Instance.SaveCurrentProfile();

            MessagePanel.PulseMessage("Difficulty changed to " + (DifficultyLevel)change.value + 
                                    ", saved to currentProfile", Color.yellow);
            Debug.Log($"Difficulty changed to {(DifficultyLevel)change.value} via Dropdown");
        }
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
}
