using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ProfileSelectionUI : MonoBehaviour
{
    [Header("Current Profile")]
    public TMP_Dropdown profileDropdown;
    [SerializeField] Button deleteButton;
    [SerializeField] TextMeshProUGUI researchPointsValueText;
    [SerializeField] TextMeshProUGUI maxWaveReachedValueText;
    [SerializeField] TextMeshProUGUI clonesUsedValueText;
    
    [Header("New Profile")]
    [SerializeField] TextMeshProUGUI noProfilesText;
    public TMP_Dropdown chooseDifficultyDropdown;
    public TMP_InputField newProfileNameInput;
    [SerializeField] string emptyProfileListWarning;
    [SerializeField] FadeCanvasGroupsWave areYouSureFadeGroup;
    [SerializeField] InputActionAsset inputAsset;
    
    InputAction enterKeyPressed;
    TextMeshProUGUI inputFieldPlaceholder;
    string originalInputFieldPlaceholderText;
    Color originalInputFieldPlaceholderColor;
    bool isDropdownSyncing;
    public bool isAreYouSurePanelOpen;

    void OnEnable()
    {
        InputActionMap uiActionMap = inputAsset.FindActionMap("UI");
        enterKeyPressed = uiActionMap.FindAction("Submit");
        enterKeyPressed.started += OnEnterKeyPressed;

        inputFieldPlaceholder = newProfileNameInput.placeholder.GetComponent<TextMeshProUGUI>();
    }

    void OnDisable()
    {
        enterKeyPressed.started -= OnEnterKeyPressed;
    }

    void Start()
    { 
        // Store the current dropdown menu's placeholder text properties
        originalInputFieldPlaceholderText = inputFieldPlaceholder.text;
        originalInputFieldPlaceholderColor = inputFieldPlaceholder.color;

        //ProfileManager.Instance.LoadAllProfiles();
        UpdateProfilePanel();
    }

    public IEnumerator DelayUpdateProfilePanel()
    {
        yield return new WaitForFixedUpdate();
        UpdateProfilePanel();
    }

    // Updates the dropdown with the current list of profiles, toggles delete button, sets other text
    public void UpdateProfilePanel()
    {
        if (ProfileManager.Instance == null)
        {
            Debug.LogError("ProfileManager.Instance is null!");
            return;
        }

        //Debug.Log("ProfileManager.Instance is valid.");

        if (ProfileManager.Instance.profiles == null)
        {
            Debug.LogError("ProfileManager.Instance.profiles is null!");
            return;
        }

        Debug.Log($"ProfileManager.Instance.profiles.Count: {ProfileManager.Instance.profiles.Count}");

        if (ProfileManager.Instance.profiles.Count == 0)
        {
            Debug.LogWarning("No profiles found in ProfileManager.");
        }

        if (ProfileManager.Instance.currentProfile == null)
        {
            Debug.LogWarning("Current profile is null.");
        }

        // Clear and ensure that the lsit is not null
        profileDropdown.ClearOptions();
        List<string> profileNames = new List<string>();

        // Handle empty profile list
        if (ProfileManager.Instance.profiles.Count < 1)
        {
            deleteButton.interactable = false;
            noProfilesText.text = emptyProfileListWarning;
            researchPointsValueText.text = "N/A";
            maxWaveReachedValueText.text = "N/A";
            clonesUsedValueText.text = "N/A";
            ProfileManager.Instance.currentProfile = null;
            Debug.Log("ProfileSelectionUI did not find any profiles");
            return;
        }

        // Populate dropdown with profile names
        foreach (PlayerProfile profile in ProfileManager.Instance.profiles)
        {
            profileNames.Add(profile.profileName);
            //Debug.Log("Added " + profile.profileName + " Profile to dropdown menu list");
        }

        if (profileNames.Count > 0)
            profileDropdown.AddOptions(profileNames);

        // Update delete button, current profile and reasearch text
        deleteButton.interactable = true;
        noProfilesText.text = "";
        researchPointsValueText.text = ProfileManager.Instance.currentProfile.researchPoints.ToString();
        maxWaveReachedValueText.text = ProfileManager.Instance.currentProfile.maxWaveSurvived.ToString();
        clonesUsedValueText.text = ProfileManager.Instance.currentProfile.playthroughs.ToString();

        
        SyncDropdownWithCurrentProfile();
    }

    public void SyncDropdownWithCurrentProfile()
    {
        if (isDropdownSyncing)
            return;

        isDropdownSyncing = true;

        string currentProfileName = ProfileManager.Instance.currentProfile.profileName;

        if (!string.IsNullOrEmpty(currentProfileName))
        {
            // Find the index of the current profile in the dropdown
            int index = ProfileManager.Instance.profiles.FindIndex(profile => profile.profileName == currentProfileName);

            // If the profile is found, set it as the selected value
            if (index >= 0)
            {
                profileDropdown.SetValueWithoutNotify(index);
                profileDropdown.RefreshShownValue(); // Refresh the dropdown to update the UI
                Debug.Log("Dropdown Sync found profile: " + currentProfileName + ", set value to index: " + index);
            }
            else
                Debug.Log("Dropdown Sync did not find a profile to sync to");

        }

        isDropdownSyncing = false;
        //StartCoroutine(SyncComplete());
    }

    IEnumerator SyncComplete()
    {
        yield return new WaitForSeconds(0.1f);
        isDropdownSyncing = false;
    }

    // Called when the player selects a profile from the dropdown
    public void SelectProfile()
    {
        if (profileDropdown.options.Count == 0)
            return;

        string selectedProfileName = profileDropdown.options[profileDropdown.value].text;

        // Check if the selected profile is different from the current profile
        if (ProfileManager.Instance.currentProfile == null || 
            !ProfileManager.Instance.currentProfile.profileName.Equals(selectedProfileName))
        {
            ProfileManager.Instance.SetCurrentProfile(selectedProfileName);
            UpdateProfilePanel(); // Refresh UI after profile change
        }
    }

    // Called when the player clicks create new profile
    public void CreateNewProfile()
    {
        string newProfileName = newProfileNameInput.text.Trim();

        if (string.IsNullOrEmpty(newProfileNameInput.text))
        {
            inputFieldPlaceholder.text = "Must enter name!";
            inputFieldPlaceholder.color = Color.red;
            Debug.LogWarning("The new profile name is empty.");
            return;
        }

        // Check if profile name already exists
        if (ProfileManager.Instance.profiles.Count > 0)
            foreach (PlayerProfile profile in ProfileManager.Instance.profiles)
            {
                if (profile.profileName.Equals(newProfileName, System.StringComparison.OrdinalIgnoreCase))
                {
                    newProfileNameInput.text = "";
                    inputFieldPlaceholder.text = "Name already used!";
                    inputFieldPlaceholder.color = Color.red;
                    Debug.LogWarning("The new profile name is already used.");
                    return;
                }
            }

        // Create new profile and set as current
        ProfileManager.Instance.CreateProfile(newProfileNameInput.text, (DifficultyLevel)chooseDifficultyDropdown.value);
        newProfileNameInput.text = "";
        chooseDifficultyDropdown.value = (int)DifficultyLevel.Normal;
        UpdateProfilePanel();

        MessageBanner.PulseMessage("New profile created!", Color.green);

        // Refresh input field placehold in case of failed input entry
        inputFieldPlaceholder.text = originalInputFieldPlaceholderText;
        inputFieldPlaceholder.color = originalInputFieldPlaceholderColor; 
    }

    public void OnDeleteProfileClicked()
    {
        areYouSureFadeGroup.isTriggered = true;
        isAreYouSurePanelOpen = true;
    }

    public void OnConfirmDeleteCurrentProfile()
    {
        ProfileManager.Instance.RemoveProfile(ProfileManager.Instance.currentProfile.profileName);

        UpdateProfilePanel();
        areYouSureFadeGroup.isTriggered = false;
        isAreYouSurePanelOpen = false;
    }

    public void OnCancelDeleteProfile()
    {
        areYouSureFadeGroup.isTriggered = false;
        isAreYouSurePanelOpen = false;
    }

    void OnEnterKeyPressed(InputAction.CallbackContext context)
    {
        CreateNewProfile();
    }
}
