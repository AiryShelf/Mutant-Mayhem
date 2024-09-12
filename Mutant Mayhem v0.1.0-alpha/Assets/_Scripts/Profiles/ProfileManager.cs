using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance;
    private string savePath => Path.Combine(Application.persistentDataPath, "profiles.json");
    public List<PlayerProfile> profiles = new List<PlayerProfile>();
    public PlayerProfile currentProfile;

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

        Initialize();
    }

    void Initialize()
    {
        LoadProfiles();

        if (PlayerPrefs.HasKey(LastUsedProfileKey))
        {
            string lastUsedProfile = PlayerPrefs.GetString(LastUsedProfileKey);
            Debug.Log($"Last used profile '{lastUsedProfile}' found in PlayerPrefs.");

            SetCurrentProfile(lastUsedProfile);
            
        }
        else
        {
            Debug.Log("No last used profile found.");
        }
    }

    // Loads all profiles from JSON
    public void LoadProfiles()
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
            Debug.Log("No profile file found, initializing an empty list.");
        }
    }

    // Saves all profiles to JSON
    public void SaveProfiles()
    {
        ProfileDataWrapper wrapper = new ProfileDataWrapper(profiles);
        string json = JsonUtility.ToJson(wrapper, true);
        
        Debug.Log($"Saving Profiles: {json}"); // Log the serialized data

        File.WriteAllText(savePath, json);
        Debug.Log($"Profiles saved to {savePath}");
    }

    // Create a new profile
    public void CreateProfile(string profileName)
    {
        if (profiles == null)
        {
            Debug.LogError("Profiles list is null during profile creation!");
            profiles = new List<PlayerProfile>(); // Ensure it's initialized
        }

        PlayerProfile newProfile = new PlayerProfile(profileName);
        profiles.Add(newProfile);
        SaveProfiles();
        SetCurrentProfile(newProfile.profileName);
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

        if (profileToRemove != null)
        {
            profiles.Remove(profileToRemove);
            SaveProfiles(); // Save the updated profiles list after removal
            PlayerPrefs.DeleteKey(profileToRemove.profileName);
            Debug.Log($"Profile '{profileName}' has been removed successfully.");
        }
        else
        {
            Debug.LogWarning($"Profile '{profileName}' not found.");
        }
    }

    // Set the current profile by profile name
    public void SetCurrentProfile(string profileName)
    {
        foreach (PlayerProfile profile in profiles)
        {
            if (profile.profileName == profileName)
            {
                currentProfile = profile;

                // Store the profile name in PlayerPrefs
                PlayerPrefs.SetString(LastUsedProfileKey, profileName);
                PlayerPrefs.Save(); // Make sure to save PlayerPrefs

                Debug.Log($"Profile '{profileName}' set as the current profile.");
                break;
            }
        }

        Debug.Log($"Current profile set to '{profileName}'");
    }
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
