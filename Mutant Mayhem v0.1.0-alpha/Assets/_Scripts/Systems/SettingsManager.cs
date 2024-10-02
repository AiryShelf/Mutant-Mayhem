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
    public DifficultyLevel startingDifficulty = DifficultyLevel.Normal;
    public DifficultyLevel difficultyLevel = DifficultyLevel.Normal; 

    [Header("Movement Settings")]
    public bool useStandardWASD = true;

    [Header("Difficulty Multipliers")]
    public float WaveDifficultyMult = 1; // Multiplies enemy stats on spawning
    public int WavesTillAddWaveBase = 0; // down harder, hard enemies faster
    public float SubwaveListGrowthFactor = 0; // up harder, more waves added over time
    public float SubwaveDelayMult = 1; // Time between Subwaves
    public float BatchSpawnMult = 1; // Multiplies number of enemies per batch in each Subwave
    public float CreditsMult = 1;

    [Header("Controls Settings")]
    public bool isSpacebarEnabled = true;

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

        InitializeProfileSettings(ProfileManager.Instance.currentProfile);
    }

    void OnEnable()
    {
        ProfileManager.OnProfileIsSet += InitializeProfileSettings;
    }

    void OnDisable()
    {
        ProfileManager.OnProfileIsSet -= InitializeProfileSettings;
    }


    public void InitializeProfileSettings(PlayerProfile currentProfile)
    {
        if (currentProfile == null)
        {
            Debug.LogError("No profile found to load settings.");
            difficultyLevel = startingDifficulty;
            useStandardWASD = true;
            isSpacebarEnabled = true;
            return;
        }

        difficultyLevel = currentProfile.difficultyLevel;
        useStandardWASD = currentProfile.isStandardWASD;
        isSpacebarEnabled = currentProfile.isSpacebarEnabled;

        Debug.Log("Settings Manager loaded settings from PlayerProfile: " + currentProfile.profileName);
    }
        
    public void ApplyGameplaySettings()
    {
        ApplyDifficultySettings();
        ApplyMovementSettings();
        ApplyControlSettings();

        waveController.Initialize();

        Debug.Log("Settings Manager finished applying settings");
    }

    #region Difficulty

    public void SetDifficulty(DifficultyLevel level)
    {
        difficultyLevel = level;
        ProfileManager.Instance.currentProfile.difficultyLevel = level;
        ProfileManager.Instance.SaveCurrentProfile();
        ApplyDifficultySettings();
    }

    public void ApplyDifficultySettings()
    {
        waveController = FindObjectOfType<WaveControllerRandom>();
        if (waveController == null)
        {
            Debug.LogError("Wave Controller not found on scene load!");
            return;
        };

        switch (difficultyLevel)
        {
            case DifficultyLevel.Easy:
                waveController._timeBetweenWaves = waveController.timeBetweenWavesBase + 60;
                WaveDifficultyMult = 0.7f;
                WavesTillAddWaveBase = 1;
                SubwaveListGrowthFactor = 0.8f;
                SubwaveDelayMult = 1.2f;
                BatchSpawnMult = 0.7f;
                CreditsMult = 1.5f;
                BuildingSystem.PlayerCredits = 600;
                MessagePanel.PulseMessage("You recieved $600 to help you through easy mode", Color.cyan);
                break;

            case DifficultyLevel.Normal:
                waveController._timeBetweenWaves = waveController.timeBetweenWavesBase;
                WaveDifficultyMult = 1;
                WavesTillAddWaveBase = 0;
                SubwaveListGrowthFactor = 1f;
                SubwaveDelayMult = 1;
                BatchSpawnMult = 1;
                CreditsMult = 1;
                break;

            case DifficultyLevel.Hard:
                waveController._timeBetweenWaves = waveController.timeBetweenWavesBase - 30;
                WaveDifficultyMult = 1.3f;
                WavesTillAddWaveBase = -1;
                SubwaveListGrowthFactor = 1.2f;
                SubwaveDelayMult = 0.8f;
                BatchSpawnMult = 1.2f;
                CreditsMult = 1;
                break;
        }

        DeathManager deathManager = FindObjectOfType<DeathManager>();
        deathManager.ApplyDifficultyToRPGain(difficultyLevel);

        Debug.Log("Difficulty updated by Settings Manager");
    }

    #endregion

    #region Movement

    public void SetMovementType(bool useStandardWASD)
    {
        this.useStandardWASD = useStandardWASD;
        ApplyMovementSettings();
    }

    public void ApplyMovementSettings()
    {
        Player player = FindObjectOfType<Player>();
        if (player)
        {
            player.useStandardWASD = useStandardWASD;
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

        if (!isSpacebarEnabled)
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