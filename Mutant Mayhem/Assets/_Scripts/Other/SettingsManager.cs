using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum DifficultyLevel
{
    Easy,
    Normal,
    Hard,
}

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Difficulty Settings")]
    public int startingDifficulty = 1;
    private DifficultyLevel difficultyLevel; 

    [Header("Movement Settings")]
    public int startingMovement = 1;
    public int useStandardWASD = 1;

    [Header("Difficulty Multipliers")]
    public float WaveDifficultyMult = 1;
    public int WavePerBaseAdjust = 0; // down harder, hard enemies faster
    public float WaveListFactor = 0; // up harder, more waves added over time
    public float BatchTimeMult = 1;
    public float BatchMult = 1;
    public float CreditsMult = 1;

    [Header("Controls Settings")]
    public int spacebarEnabled = 1;

    WaveControllerRandom waveController;  
    Player player;

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

        InitializeSettings();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void InitializeSettings()
    {
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

        // Controls
        if (PlayerPrefs.HasKey("SpacebarEnabled"))
        {
            spacebarEnabled = PlayerPrefs.GetInt("SpacebarEnabled");
        }
        else
        {
            spacebarEnabled = 1;
            PlayerPrefs.SetInt("SpacebarEnabled", 1);
        }

        ApplyDifficultySettings();
        ApplyMovementSettings();
        ApplyControlSettings();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene Loaded: " + scene.name);
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
        waveController = FindObjectOfType<WaveControllerRandom>();
        if (waveController == null)
            Debug.LogError("Wave Controller not found on scene load!");

        switch (difficultyLevel)
        {
            case DifficultyLevel.Easy:
                waveController.timeBetweenWaves = 
                                waveController.timeBetweenWavesBase + 60;
                WaveDifficultyMult = 0.7f;
                WavePerBaseAdjust = 1;
                WaveListFactor = 0.8f;
                BatchTimeMult = 1.2f;
                BatchMult = 0.7f;
                CreditsMult = 1.5f;
                break;

            case DifficultyLevel.Normal:
            waveController.timeBetweenWaves = 
                            waveController.timeBetweenWavesBase;
                WaveDifficultyMult = 1;
                WavePerBaseAdjust = 0;
                WaveListFactor = 1f;
                BatchTimeMult = 1;
                BatchMult = 1;
                CreditsMult = 1;
                break;

            case DifficultyLevel.Hard:
                waveController.timeBetweenWaves = 
                                waveController.timeBetweenWavesBase - 30;
                WaveDifficultyMult = 1.3f;
                WavePerBaseAdjust = -1;
                WaveListFactor = 1.2f;
                BatchTimeMult = 0.8f;
                BatchMult = 1.3f;
                CreditsMult = 1;
                break;
        }
        //Debug.Log("Difficulty updated");
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
        Player player = FindObjectOfType<Player>();
        if (player)
        {
            player.movementType = useStandardWASD;
            //Debug.Log("Movement Type updated");
        }
        else
        {
            //Debug.Log("Player not found by SettingsManager");
        }
    }

    #endregion

    #region Controls

    public void ApplyControlSettings()
    {
        player = FindObjectOfType<Player>();
        InputAction throwAction = player.inputAsset.FindActionMap("Player").FindAction("Throw");

        if (spacebarEnabled == 0)
        {
            // Disable the spacebar
            for (int i = 0; i < throwAction.bindings.Count; i++)
            {
                if (throwAction.bindings[i].effectivePath == "<Keyboard>/space")
                {
                    throwAction.ApplyBindingOverride(i, new InputBinding { overridePath = "" });
                }
            }
        }
        else
        {
            // Enable the spacebar
            for (int i = 0; i < throwAction.bindings.Count; i++)
            {
                if (throwAction.bindings[i].effectivePath == "<Keyboard>/space")
                {
                    throwAction.RemoveBindingOverride(i);
                }
            }
        }
    }

    #endregion
}