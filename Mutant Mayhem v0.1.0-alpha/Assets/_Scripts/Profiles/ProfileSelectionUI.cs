using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ProfileSelectionUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI currentProfileNameText;
    [SerializeField] TextMeshProUGUI researchPointsValueText;
    [SerializeField] TextMeshProUGUI maxWaveReachedValueText;
    [SerializeField] Button deleteButton;
    public TMP_Dropdown profileDropdown;
    public TMP_InputField newProfileNameInput;
    [SerializeField] string emptyProfileListWarning;
    [SerializeField] FadeCanvasGroupsWave areYouSureFadeGroup;
    [SerializeField] InputActionAsset inputAsset;
    
    FadeCanvasGroupsWave myFadeGroup;
    InputAction enterKeyPressed;
    InputAction escapeKeyPressed;
    TextMeshProUGUI inputFieldPlaceholder;
    string originalInputFieldPlaceholderText;
    Color originalInputFieldPlaceholderColor;
    bool isDropdownSyncing;
    public bool isAreYouSurePanelOpen;

    void OnEnable()
    {
        InputActionMap uiActionMap = inputAsset.FindActionMap("UI");
        enterKeyPressed = uiActionMap.FindAction("Submit");
        escapeKeyPressed = uiActionMap.FindAction("Escape");
        enterKeyPressed.performed += OnEnterKeyPressed;

        myFadeGroup = GetComponent<FadeCanvasGroupsWave>();
        inputFieldPlaceholder = newProfileNameInput.placeholder.GetComponent<TextMeshProUGUI>();
    }

    void OnDisable()
    {
        enterKeyPressed.performed -= OnEnterKeyPressed;
    }

    void Start()
    { 
        // Store the current dropdown menu's placeholder text properties
        originalInputFieldPlaceholderText = inputFieldPlaceholder.text;
        originalInputFieldPlaceholderColor = inputFieldPlaceholder.color;

        ProfileManager.Instance.LoadAllProfiles();
        UpdateProfilePanel();
    }

    // Updates the dropdown with the current list of profiles, toggles delete button, sets other text
    public void UpdateProfilePanel()
    {
        // Clear and ensure that the lsit is not null
        profileDropdown.ClearOptions();
        List<string> profileNames = new List<string>();

        // Handle empty profile list
        if (ProfileManager.Instance.profiles == null || ProfileManager.Instance.profiles.Count < 1)
        {
            deleteButton.interactable = false;
            currentProfileNameText.text = emptyProfileListWarning;
            researchPointsValueText.text = "N/A";
            maxWaveReachedValueText.text = "N/A";
            ProfileManager.Instance.currentProfile = null;
            return;
        }

        // Populate dropdown with profile names
        foreach (PlayerProfile profile in ProfileManager.Instance.profiles)
        {
            profileNames.Add(profile.profileName);
        }

        // Ensure there is a valid current profile after deleting
    if (ProfileManager.Instance.currentProfile == null && ProfileManager.Instance.profiles.Count > 0)
    {
        ProfileManager.Instance.SetCurrentProfile(profileNames[0]); // Set the first profile as current
    }

        // Update delete button, current profile and reasearch text
        deleteButton.interactable = true;
        currentProfileNameText.text = ProfileManager.Instance.currentProfile.profileName;
        researchPointsValueText.text = ProfileManager.Instance.currentProfile.researchPoints.ToString();
        maxWaveReachedValueText.text = ProfileManager.Instance.currentProfile.maxWaveReached.ToString();

        profileDropdown.AddOptions(profileNames);
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
                profileDropdown.value = index;
                profileDropdown.RefreshShownValue(); // Refresh the dropdown to update the UI
            }
        }

        StartCoroutine(SyncComplete());
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

    // Called when the player clicks creates a new profile
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
                    newProfileNameInput.text = null;
                    inputFieldPlaceholder.text = "Name already used!";
                    inputFieldPlaceholder.color = Color.red;
                    Debug.LogWarning("The new profile name is already used.");
                    return;
                }
            }

        // Create new profile and set as current
        ProfileManager.Instance.CreateProfile(newProfileNameInput.text);
        newProfileNameInput.text = "";
        UpdateProfilePanel();

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
