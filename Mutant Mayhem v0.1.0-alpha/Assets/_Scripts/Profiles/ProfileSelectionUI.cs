using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileSelectionUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI currentProfileNameText;
    [SerializeField] TextMeshProUGUI researchPointsValueText;
    [SerializeField] Button deleteButton;
    public TMP_Dropdown profileDropdown;
    public TMP_InputField newProfileNameInput;
    [SerializeField] string emptyProfileListWarning;
    

    TextMeshProUGUI inputFieldPlaceholder;
    string originalInputFieldPlaceholderText;
    Color originalInputFieldPlaceholderColor;

    void Start()
    {
        ProfileManager.Instance.LoadProfiles();
        UpdateProfilePanel();
        inputFieldPlaceholder = newProfileNameInput.placeholder.GetComponent<TextMeshProUGUI>();
        originalInputFieldPlaceholderText = inputFieldPlaceholder.text;
        originalInputFieldPlaceholderColor = inputFieldPlaceholder.color;
    }

    // Updates the dropdown with the current list of profiles, toggles delete button, sets other text
    public void UpdateProfilePanel()
    {
        // Clear and ensure that the lsit is not null
        profileDropdown.ClearOptions();
        List<string> profileNames = new List<string>();

        // Handle empty profile list
        if (ProfileManager.Instance.profiles.Count < 1)
        {
            deleteButton.interactable = false;
            currentProfileNameText.text = emptyProfileListWarning;
            researchPointsValueText.text = "N/A";
            return;
        }

        // Update current profile and reasearch text
        deleteButton.interactable = true;
        currentProfileNameText.text = ProfileManager.Instance.currentProfile.profileName;
        researchPointsValueText.text = ProfileManager.Instance.currentProfile.researchPoints.ToString();

        // Populate and sync dropdown menu
        foreach (PlayerProfile profile in ProfileManager.Instance.profiles)
        {
            profileNames.Add(profile.profileName);
        }
        profileDropdown.AddOptions(profileNames);
        SyncDropdownWithCurrentProfile();
    }

    public void SyncDropdownWithCurrentProfile()
    {
        string currentProfileName = ProfileManager.Instance.currentProfile.profileName;

        if (!string.IsNullOrEmpty(currentProfileName))
        {
            // Find the index of the current profile in the dropdown
            int index = ProfileManager.Instance.profiles.FindIndex(profile => profile.profileName == currentProfileName);

            // If the profile is found, set it as the selected value
            if (index >= 0)
            {
                profileDropdown.value = index;
                profileDropdown.RefreshShownValue(); // Refresh the dropdown to update the UI
            }
        }
    }

    // Called when the player selects a profile from the dropdown
    public void SelectProfile()
    {
        string selectedProfileName = profileDropdown.options[profileDropdown.value].text;
        ProfileManager.Instance.SetCurrentProfile(selectedProfileName);
        UpdateProfilePanel();
    }

    // Called when the player creates a new profile
    public void CreateNewProfile()
    {
        string newProfileName = newProfileNameInput.text.Trim();

        if (string.IsNullOrEmpty(newProfileNameInput.text))
        {
            inputFieldPlaceholder.text = "Must Enter Name!";
            inputFieldPlaceholder.color = Color.red;
            Debug.LogWarning("The new profile name is empty.");
            return;
        }

        if (ProfileManager.Instance.profiles.Count > 0)
            foreach (PlayerProfile profile in ProfileManager.Instance.profiles)
            {
                if (profile.profileName.Equals(newProfileName, System.StringComparison.OrdinalIgnoreCase))
                {
                    newProfileNameInput.text = null;
                    inputFieldPlaceholder.text = "Name already used!";
                    inputFieldPlaceholder.color = Color.red;
                    Debug.LogWarning("The new profile name is already used.");
                    return;
                }
            }

            ProfileManager.Instance.CreateProfile(newProfileNameInput.text);
            newProfileNameInput.text = "";
            UpdateProfilePanel();

            // Refresh input field placehold in case of failed input entry
            inputFieldPlaceholder.text = originalInputFieldPlaceholderText;
            inputFieldPlaceholder.color = originalInputFieldPlaceholderColor; 
    }

    public void RemoveCurrentProfile()
    {
        ProfileManager.Instance.RemoveProfile(ProfileManager.Instance.currentProfile.profileName);

        // Check if any profiles remain
        if (ProfileManager.Instance.profiles.Count > 0)
        {
            // Set the first profile in the list as the new current profile
            ProfileManager.Instance.SetCurrentProfile(ProfileManager.Instance.profiles[0].profileName);
        }
        else
        {
            // No profiles left, clear currentProfile and prompt for a new profile
            ProfileManager.Instance.currentProfile = null;
            Debug.LogWarning("No profiles left. Please create a new profile.");
            
            // Optionally, prompt the player to create a new profile
            // You could display a message or open the create profile panel
        }


        UpdateProfilePanel();
    }
    
}
