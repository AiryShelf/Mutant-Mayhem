using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DifficultyLevel
{
    Easy,
    Normal,
    Hard,
}

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }
    
    public static bool TutorialDisabled = false;
    public static bool tutorialShowedBuild = false;
    public static bool tutorialShowedUpgrade = false;

    [Header("Difficulty Setting")]
    public static int startingDifficulty = 1;
    private DifficultyLevel difficultyLevel; 

    [Header("Movement Setting")]
    public static int startingMovement = 1;
    public int useStandardWASD = 1;

    [Header("Difficulty Multipliers")]
    public float WaveDifficultyMult = 1;
    public int WavePerBaseAdjust = 0; // down harder, hard enemies faster
    public float WaveListFactor = 0; // up harder, more waves added over time
    public float BatchTimeMult = 1;
    public float BatchMult = 1;
    public float CreditsMult = 1;    

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
        }

        InitializeSettings();
    }

    void InitializeSettings()
    {
        // Reset tutorial
        tutorialShowedBuild = false;
        tutorialShowedUpgrade = false;

        // Difficulty
        if (PlayerPrefs.HasKey("DifficultyLevel"))
        {
            difficultyLevel = (DifficultyLevel)PlayerPrefs.GetInt("DifficultyLevel");
        }
        else
        {
            difficultyLevel = (DifficultyLevel)startingDifficulty;
            PlayerPrefs.SetInt("DifficultyLevel", (int)difficultyLevel);
        }

        // Movement Type
        if (PlayerPrefs.HasKey("StandardWASD"))
        {
            useStandardWASD = PlayerPrefs.GetInt("StandardWASD");
        }
        else
        {
            useStandardWASD = startingMovement;
            PlayerPrefs.SetInt("StandardWASD", 1);
        }

        ApplyDifficultySettings();
        ApplyMovementSettings();
    }

    #region Difficulty

    public void SetDifficulty(DifficultyLevel level)
    {
        difficultyLevel = level;
        PlayerPrefs.SetInt("DifficultyLevel", (int)level);
        ApplyDifficultySettings();
    }

    private void ApplyDifficultySettings()
    {
        switch (difficultyLevel)
        {
            case DifficultyLevel.Easy:
                WaveDifficultyMult = 0.7f;
                WavePerBaseAdjust = 1;
                WaveListFactor = 0.8f;
                BatchTimeMult = 1.2f;
                BatchMult = 0.7f;
                CreditsMult = 2f;
                break;

            case DifficultyLevel.Normal:
                WaveDifficultyMult = 1;
                WavePerBaseAdjust = 0;
                WaveListFactor = 1f;
                BatchTimeMult = 1;
                BatchMult = 1;
                CreditsMult = 1;
                break;

            case DifficultyLevel.Hard:
                WaveDifficultyMult = 1.3f;
                WavePerBaseAdjust = -1;
                WaveListFactor = 1.2f;
                BatchTimeMult = 0.8f;
                BatchMult = 1.3f;
                CreditsMult = 1;
                break;
        }
        Debug.Log("Difficulty updated");
    }

    #endregion

    #region Movement

    public void SetMovementType(int useStandardWASD)
    {
        if (useStandardWASD == 1)
            this.useStandardWASD = 1;
        else
            // Move to mouse
            this.useStandardWASD = 0;

        PlayerPrefs.SetInt("StandardWASD", useStandardWASD);
        ApplyMovementSettings();
    }

    public void ApplyMovementSettings()
    {
        FindObjectOfType<Player>().movementType = useStandardWASD;
        Debug.Log("Movement Type updated");
    }

    #endregion
}