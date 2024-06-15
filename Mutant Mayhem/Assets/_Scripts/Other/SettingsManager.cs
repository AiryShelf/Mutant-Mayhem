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

    [Header("Difficulty Setting")]
    private DifficultyLevel difficultyLevel = DifficultyLevel.Easy; 

    [Header("Movement Setting")]
    public bool useStandardWASD = false;

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
    }

    void Start()
    {
        SetDifficulty(DifficultyLevel.Normal);
    }

    #region Difficulty

    public void SetDifficulty(DifficultyLevel level)
    {
        difficultyLevel = level;
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

    public void SetMovementType(bool useStandardWASD)
    {
        if (useStandardWASD)
            this.useStandardWASD = true;
        else
            // Move to mouse
            this.useStandardWASD = false;
        Debug.Log("Movement Type updated");
    }

    #endregion
}