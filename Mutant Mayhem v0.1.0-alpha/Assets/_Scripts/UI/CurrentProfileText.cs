using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurrentProfileText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI currentProfileText;

    void OnEnable()
    {
        ProfileManager.OnProfileIsSet += UpdateProfileText;
    }

    void OnDisable()
    {      
        ProfileManager.OnProfileIsSet -= UpdateProfileText;
    }

    void Start()
    {
        UpdateProfileText(ProfileManager.Instance.currentProfile);
    }

    public void UpdateProfileText(PlayerProfile currentProfile)
    {
        // Update current profile text
        if (currentProfile != null && !string.IsNullOrEmpty(currentProfile.profileName))
        {
            currentProfileText.text = "Current Profile: " + currentProfile.profileName;
            currentProfileText.color = Color.green;
        }
        else
        {
            currentProfileText.text = "Create a profile before playing!";
            currentProfileText.color = Color.red;
        }
    }
}
