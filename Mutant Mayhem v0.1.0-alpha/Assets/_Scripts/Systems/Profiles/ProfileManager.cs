using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance;
    private string savePath => Path.Combine(Application.persistentDataPath, "profiles.json");
    public List<PlayerProfile> profiles = new List<PlayerProfile>();
    public PlayerProfile currentProfile;
    public static event Action<PlayerProfile> OnProfileIsSet;

    const string LastUsedProfileKey = "LastUsedProfile";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Debug.Log("Persistent Data Path: " + Application.persistentDataPath);
        ClearAllPlayerPrefsForEditor();

        Initialize();
    }

    void ClearAllPlayerPrefsForEditor()
    {
        #if UNITY_EDITOR
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save(); // Ensure changes are written to disk
            Debug.Log("All PlayerPrefs have been cleared.");
        #endif
    }

    void Initialize()
    {
        LoadAllProfiles();

        // Check if there is a last used profile
        if (PlayerPrefs.HasKey(LastUsedProfileKey))
        {
            string lastUsedProfile = PlayerPrefs.GetString(LastUsedProfileKey);

            // Check if the last used profile exists in the loaded profiles
            bool profileExists = profiles.Any(profile => profile.profileName == lastUsedProfile);

            if (profileExists)
            {
                Debug.Log($"Last used profile '{lastUsedProfile}' found in PlayerPrefs and exists " +
                            "in loaded profiles.");
                SetCurrentProfile(lastUsedProfile);
            }
            else
            {
                Debug.LogWarning($"Last used profile '{lastUsedProfile}' does not exist in loaded " +
                                    "profiles. Removing from PlayerPrefs.");
                PlayerPrefs.DeleteKey(LastUsedProfileKey);
                PlayerPrefs.Save(); // Ensure the change is saved
            }
        }
        else
        {
            Debug.Log("No last used profile found.");
            if (profiles.Count > 0)
            {
                SetCurrentProfile(profiles[0].profileName);
                Debug.Log("Profile set to index 0");
            }
        }
    }

    #region Load, Set, Save

    // Loads all profiles from JSON
    public void LoadAllProfiles()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            ProfileDataWrapper wrapper = JsonUtility.FromJson<ProfileDataWrapper>(json);

            // Ensure profiles are not null, even if the file exists but is empty
            profiles = wrapper?.profiles ?? new List<PlayerProfile>();

            Debug.Log($"Loaded {profiles.Count} profiles from {savePath}");
        }
        else
        {
            profiles = new List<PlayerProfile>();
            Debug.Log("No profile file found, started a new list.");
        }
    }

    // Set the current profile by profile name
    public void SetCurrentProfile(string profileName)
    {
        Debug.Log($"Attempting to set profile: {profileName}");

        // Find profile in loaded list
        foreach (PlayerProfile profile in profiles)
        {
            if (profile == null)
            {
                Debug.LogError("Profile is null, remove and skip...");
                profiles.Remove(profile);
                continue;
            }

            if (profile.profileName == profileName)
            {
                currentProfile = profile;

                // Store the profile name in PlayerPrefs
                PlayerPrefs.SetString(LastUsedProfileKey, profileName);
                PlayerPrefs.Save(); // Make sure to save PlayerPrefs

                Debug.Log($"Profile '{profileName}' set as the current profile.");
                if (OnProfileIsSet != null)
                {
                    Debug.Log("Invoking OnProfileIsSet event.");
                    OnProfileIsSet.Invoke(currentProfile);
                }
                return;
            }
        }

        // Handle no profiles remaining
        Debug.LogWarning("No profile with name " + profileName + " found to set!");
    }

    // Saves all profiles to JSON
    void SaveAllProfiles()
    {
        ProfileDataWrapper wrapper = new ProfileDataWrapper(profiles);
        string json = JsonUtility.ToJson(wrapper, true);
        
        Debug.Log($"Saving Profiles: {json}"); // Log the serialized data

        File.WriteAllText(savePath, json);
        Debug.Log($"Profiles saved to {savePath}");
    }

    public void SaveCurrentProfile()
    {
        if (currentProfile != null)
        {
            // Ensure the profile is part of the profiles list
            int index = profiles.FindIndex(profile => profile.profileName == currentProfile.profileName);

            if (index != -1)
            {
                profiles[index] = currentProfile;
            }
            else
            {
                return;
                // If for some reason the current profile is not found, add it
                //profiles.Add(currentProfile);
                //Debug.LogError("Could not find current profile in list.  Added it to list.");
            }

            SaveAllProfiles(); // Save all profiles including the updated current profile
            Debug.Log($"Current profile '{currentProfile.profileName}' has been saved.");
        }
        else
        {
            Debug.LogError("No current profile set to save.");
        }
    }

    #endregion Load, Set, Save

    #region Add, Remove

    // Create a new profile
    public void CreateProfile(string profileName)
    {
        if (profiles == null)
        {
            Debug.LogError("Profiles list is null during profile creation!");
            profiles = new List<PlayerProfile>();
        }

        PlayerProfile newProfile = new PlayerProfile(profileName);

        profiles.Add(newProfile);
        SetCurrentProfile(newProfile.profileName);
        SaveCurrentProfile();

        ProfileSelectionUI profileSelectionUI = FindObjectOfType<ProfileSelectionUI>();
        if (profileSelectionUI != null)
            profileSelectionUI.UpdateProfilePanel();

        Debug.Log($"Created new profile with default settings: {profileName}");
    }

    public void RemoveProfile(string profileName)
    {
        // Check if profiles list is initialized
        if (profiles == null || profiles.Count == 0)
        {
            Debug.LogWarning("No profiles to remove.");
            return;
        }

        // Find the profile to remove
        PlayerProfile profileToRemove = profiles.Find(profile => profile.profileName.Equals(
                                        profileName, System.StringComparison.OrdinalIgnoreCase));

        // Remove the profile
        if (profileToRemove != null)
        {
            profiles.Remove(profileToRemove);
            SaveAllProfiles();
            Debug.Log($"Profile '{profileName}' has been removed successfully.");
        }
        else
        {
            Debug.LogWarning($"Profile '{profileName}' not found.");
        }

        // Check if any profiles remain
        if (profiles.Count >= 1)
        {
            // Set the first profile in the list as the new current profile
            SetCurrentProfile(profiles[0].profileName);
        }
        else
        {
            // No profiles left, clear currentProfile and prompt for a new profile
            currentProfile = null;
            Debug.LogWarning("No profiles left. Please create a new profile.");
            
            // Optionally, prompt the player to create a new profile
            // You could display a message or open the create profile panel
        }

        OnProfileIsSet.Invoke(currentProfile);
    }

    #endregion Add, Remove
}

// Wrapper class for serializing the list of PlayerProfile objects
[System.Serializable]
public class ProfileDataWrapper
{
    public List<PlayerProfile> profiles = new List<PlayerProfile>();

    public ProfileDataWrapper(List<PlayerProfile> profiles)
    {
        this.profiles = profiles;
    }
}
