using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OptionsPanel : MonoBehaviour
{
    [SerializeField] TMP_Dropdown difficultyDropdown;
    [SerializeField] TMP_Dropdown movementTypeDropdown;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        // Difficulty
        if (PlayerPrefs.HasKey("DifficultyLevel"))
        {
            difficultyDropdown.value = PlayerPrefs.GetInt("DifficultyLevel");
        }
        else
        {
            difficultyDropdown.value = SettingsManager.startingDifficulty;
        }

        // Movement Type
        if (PlayerPrefs.HasKey("StandardWASD"))
        {
            movementTypeDropdown.value = PlayerPrefs.GetInt("StandardWASD");
        }
        else
        {
            movementTypeDropdown.value = SettingsManager.startingMovement;
        }

        difficultyDropdown.onValueChanged.AddListener(delegate { 
                                          DifficultyValueChanged(difficultyDropdown); });
        movementTypeDropdown.onValueChanged.AddListener(delegate { 
                                            MoveTypeValueChanged(movementTypeDropdown); });
    }

    void DifficultyValueChanged(TMP_Dropdown change)
    {
        switch (change.value)
        {
            case 0:
                SettingsManager.Instance.SetDifficulty(DifficultyLevel.Easy);
                break;
            case 1:
                SettingsManager.Instance.SetDifficulty(DifficultyLevel.Normal);
                break;
            case 2:
                SettingsManager.Instance.SetDifficulty(DifficultyLevel.Hard);
                break;
            default:
                Debug.LogError("Failed to change difficulty");
                break;
        }

        Debug.Log("Difficulty changed via Dropdown");
    }

    void MoveTypeValueChanged(TMP_Dropdown change)
    {
        switch (change.value)
        {
            case 0:
                SettingsManager.Instance.SetMovementType(0);
                break;
            case 1:
                SettingsManager.Instance.SetMovementType(1);
                break;
            default:
                Debug.LogError("Failed to change move type");
                break;
        }
    }
}
