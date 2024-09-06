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
    [SerializeField] Toggle spacebarToggle;

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
            difficultyDropdown.value = SettingsManager.Instance.startingDifficulty;
        }

        // Movement Type
        if (PlayerPrefs.HasKey("StandardWASD"))
        {
            movementTypeDropdown.value = PlayerPrefs.GetInt("StandardWASD");
        }
        else
        {
            movementTypeDropdown.value = 1;
        }

        // Controls
        if (SettingsManager.Instance.spacebarEnabled == 1)
            spacebarToggle.isOn = true;
        else
            spacebarToggle.isOn = false;

        difficultyDropdown.onValueChanged.AddListener(delegate { 
                                            DifficultyValueChanged(difficultyDropdown); });
        movementTypeDropdown.onValueChanged.AddListener(delegate { 
                                            MoveTypeValueChanged(movementTypeDropdown); });
        spacebarToggle.onValueChanged.AddListener(delegate {
                                            ToggleSpacebar(spacebarToggle); });
    }

    public void ToggleSpacebar(Toggle change)
    {
        Debug.Log("Toggle Spacebar ran");
        Player player = FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogError("Player not found");
            return;
        }

        InputAction throwAction = player.inputAsset.FindActionMap("Player").FindAction("Throw");
        if (throwAction == null)
        {
            Debug.LogError("Throw action not found");
            return;
        }

        if (SettingsManager.Instance.spacebarEnabled == 1)
        { 
            Debug.Log("spacebar found enabled");
            // Disable the spacebar
            for (int i = 0; i < throwAction.bindings.Count; i++)
            {
                Debug.Log("entered for loop disable");
                if (throwAction.bindings[i].path == "<Keyboard>/space")
                {
                    Debug.Log("Disbaled Spacebar");
                    throwAction.ApplyBindingOverride(i, new InputBinding { overridePath = "" });
                    PlayerPrefs.SetInt("SpacebarEnabled", 0);
                    SettingsManager.Instance.spacebarEnabled = 0;
                }
            }
        }
        else
        {
            Debug.Log("spacebar found disabled");
            // Enable the spacebar
            for (int i = 0; i < throwAction.bindings.Count; i++)
            {
                Debug.Log("entered for loop enable");
                if (throwAction.bindings[i].path == "<Keyboard>/space")
                {
                    Debug.Log("Enabled Spacebar");
                    throwAction.RemoveBindingOverride(i);
                    PlayerPrefs.SetInt("SpacebarEnabled", 1);
                    SettingsManager.Instance.spacebarEnabled = 1;
                }
            }
        }
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
