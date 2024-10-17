using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurrentDifficultyText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI currentDifficultyText;

    void OnEnable()
    {
        ProfileManager.OnProfileIsSet += UpdateDifficultyText;
    }

    void OnDisable()
    {      
        ProfileManager.OnProfileIsSet -= UpdateDifficultyText;
    }

    void Start()
    {
        UpdateDifficultyText(ProfileManager.Instance.currentProfile);
    }

    public void UpdateDifficultyText(PlayerProfile currentProfile)
    {
        // Update current difficulty text
        if (currentProfile != null && !string.IsNullOrEmpty(currentProfile.profileName))
        {
            currentDifficultyText.text = "Difficulty: " + currentProfile.difficultyLevel;
            currentDifficultyText.color = Color.cyan;
        }
        else
        {
            currentDifficultyText.text = "Create a profile before playing!";
            currentDifficultyText.color = Color.red;
        }
    }
}