using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OptionsPanel : MonoBehaviour
{
    public FadeCanvasGroupsWave fadeGroup;
    [SerializeField] TMP_Dropdown difficultyDropdown;
    [SerializeField] TMP_Dropdown movementTypeDropdown;
    [SerializeField] Toggle spacebarToggle;
    [SerializeField] Toggle tutorialToggle;

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

            difficultyDropdown.value = (int)profile.difficultyLevel;
            movementTypeDropdown.value = profile.isStandardWASD ? 1 : 0;
            tutorialToggle.isOn = profile.isTutorialEnabled;
            spacebarToggle.isOn = profile.isSpacebarEnabled;
        }
        else
        {
            difficultyDropdown.value = (int)SettingsManager.Instance.startingDifficulty;
            movementTypeDropdown.value = 1; // Default to standard WASD
            tutorialToggle.isOn = true;
            spacebarToggle.isOn = true;
        }

        // Add listeners
        tutorialToggle.onValueChanged.AddListener(delegate {
                                            ToggleTutorial(tutorialToggle); });
        difficultyDropdown.onValueChanged.AddListener(delegate { 
                                            DifficultyValueChanged(difficultyDropdown); });
        movementTypeDropdown.onValueChanged.AddListener(delegate { 
                                            MoveTypeValueChanged(movementTypeDropdown); });
        spacebarToggle.onValueChanged.AddListener(delegate {
                                            ToggleSpacebar(spacebarToggle); });
    }

    public void ToggleSpacebar(Toggle change)
    {
        ProfileManager.Instance.currentProfile.isSpacebarEnabled = change.isOn;
        ProfileManager.Instance.SaveCurrentProfile();

        SettingsManager.Instance.ApplyControlSettings();
        Debug.Log("Toggled Spacebar");
    }

    public void ToggleTutorial(Toggle change)
    {
        if (ProfileManager.Instance.currentProfile == null)
        {
            Debug.LogError("No current profile to save tutorial setting.");
            return;
        }

        // Change profile settings
        ProfileManager.Instance.currentProfile.isTutorialEnabled = change.isOn;
        ProfileManager.Instance.SaveCurrentProfile(); // Save the profile with updated tutorial state

        TutorialManager.SetTutorialStateAndProfile(change.isOn);
        Debug.Log("Tutorial Enabled: " + change.isOn);
        
    }

    void DifficultyValueChanged(TMP_Dropdown change)
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
                break;
        }

        ProfileManager.Instance.SaveCurrentProfile();
        Debug.Log("Difficulty changed via Dropdown");
    }

    void MoveTypeValueChanged(TMP_Dropdown change)
    {
        switch (change.value)
        {
            case 0:
                ProfileManager.Instance.currentProfile.isStandardWASD = false;
                break;
            case 1:
                ProfileManager.Instance.currentProfile.isStandardWASD = true;
                break;
            default:
                Debug.LogError("Failed to change move type");
                break;
        }

        ProfileManager.Instance.SaveCurrentProfile();
    }
}
