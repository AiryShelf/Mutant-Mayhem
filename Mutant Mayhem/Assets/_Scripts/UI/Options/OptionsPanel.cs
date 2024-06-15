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
    }

    void MoveTypeValueChanged(TMP_Dropdown change)
    {
        switch (change.value)
        {
            case 0:
                SettingsManager.Instance.SetMovementType(false);
                break;
            case 1:
                SettingsManager.Instance.SetMovementType(true);
                break;
            default:
                Debug.LogError("Failed to change move type");
                break;
        }
    }
}
