using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurrentProfileText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI currentProfileText;
    [SerializeField] bool showLeadText = true;

    void OnEnable()
    {
        ProfileManager.OnProfileIsSet += UpdateProfileText;
        if (ProfileManager.Instance != null)
            UpdateProfileText(ProfileManager.Instance.currentProfile);
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
        if (currentProfile != null && !string.IsNullOrEmpty(currentProfile.profileName))
        {
            if (showLeadText)
                currentProfileText.text = "Current Profile: " + currentProfile.profileName;
            else
                currentProfileText.text = currentProfile.profileName;
            currentProfileText.color = Color.green;
        }
        else
        {
            currentProfileText.text = "Create a profile before playing!";
            currentProfileText.color = Color.red;
        }
    }
}
