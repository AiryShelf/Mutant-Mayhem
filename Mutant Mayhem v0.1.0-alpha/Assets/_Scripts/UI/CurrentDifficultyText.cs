using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurrentDifficultyText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI currentDifficultyText;
    [SerializeField] bool showLeadText = true;

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
        if (currentProfile != null && !string.IsNullOrEmpty(currentProfile.profileName))
        {
            // Set text
            if (showLeadText)
                currentDifficultyText.text = "Difficulty: " + currentProfile.difficultyLevel;
            else
                currentDifficultyText.text = currentProfile.difficultyLevel.ToString();

            // Set color
            switch (currentProfile.difficultyLevel)
            {
                case DifficultyLevel.Easy:
                currentDifficultyText.color = Color.green;
                break;

                case DifficultyLevel.Normal:
                currentDifficultyText.color = Color.cyan;
                break;

                case DifficultyLevel.Hard:
                currentDifficultyText.color = Color.red;
                break;
            }
        }
        else
        {
            currentDifficultyText.text = "Create a profile before playing!";
            currentDifficultyText.color = Color.red;
        }
    }
}