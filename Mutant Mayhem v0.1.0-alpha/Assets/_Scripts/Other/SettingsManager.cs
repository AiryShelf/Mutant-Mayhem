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
    public DifficultyLevel difficultyLevel; 

    [Header("Movement Settings")]
    public int useStandardWASD = 1;

    [Header("Difficulty Multipliers")]
    public float WaveDifficultyMult = 1;
    public int WavesTillAddWaveBase = 0; // down harder, hard enemies faster
    public float SubwaveListGrowthFactor = 0; // up harder, more waves added over time
    public float SubwaveDelayMult = 1;
    public float BatchSpawnMult = 1;
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
        // For debug
        // PlayerPrefs.DeleteAll();
        // PlayerPrefs.Save();

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
            useStandardWASD = 1;
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

        PlayerPrefs.Save();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene Loaded: " + scene.name);
        if (scene.name == "Level 1")
        {
            ApplyDifficultySettings();
            ApplyMovementSettings();
            ApplyControlSettings();
        }
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
        {
            Debug.LogError("Wave Controller not found on scene load!");
            return;
        }

        switch (difficultyLevel)
        {
            case DifficultyLevel.Easy:
                waveController._timeBetweenWaves = 
                                waveController.timeBetweenWavesBase + 60;
                WaveDifficultyMult = 0.7f;
                WavesTillAddWaveBase = 1;
                SubwaveListGrowthFactor = 0.8f;
                SubwaveDelayMult = 1.2f;
                BatchSpawnMult = 0.7f;
                CreditsMult = 1.5f;
                break;

            case DifficultyLevel.Normal:
            waveController._timeBetweenWaves = 
                            waveController.timeBetweenWavesBase;
                WaveDifficultyMult = 1;
                WavesTillAddWaveBase = 0;
                SubwaveListGrowthFactor = 1f;
                SubwaveDelayMult = 1;
                BatchSpawnMult = 1;
                CreditsMult = 1;
                break;

            case DifficultyLevel.Hard:
                waveController._timeBetweenWaves = 
                                waveController.timeBetweenWavesBase - 30;
                WaveDifficultyMult = 1.3f;
                WavesTillAddWaveBase = -1;
                SubwaveListGrowthFactor = 1.2f;
                SubwaveDelayMult = 0.8f;
                BatchSpawnMult = 1.3f;
                CreditsMult = 1;
                break;
        }
        //Debug.Log("Difficulty updated");
    }

    #endregion

    #region Movement

    public void SetMovementType(int useStandardWASD)
    {
        this.useStandardWASD = useStandardWASD;
        PlayerPrefs.SetInt("StandardWASD", useStandardWASD);
        ApplyMovementSettings();
    }

    public void ApplyMovementSettings()
    {
        Player player = FindObjectOfType<Player>();
        if (player)
        {
            player.movementType = useStandardWASD;
            Debug.Log("Movement Type updated: " + useStandardWASD);
        }
        else
        {
            Debug.Log("Player not found by SettingsManager");
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