using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    public bool useInstantJoystickAim = true;

    [Header("Difficulty Multipliers")]
    public float WaveDifficultyMult = 1; // Multiplies wave difficulty
    public int WavesTillAddWaveBaseDifficultyAdjust = 0; // down harder, hard enemies appear sooner
    public float SubwaveListGrowthFactor = 0; // up harder, more waves added over time
    public float SubwaveDelayMult = 1; // Time between Subwaves
    public float BatchSpawnMult = 1; // Multiplies number of enemies per batch in each Subwave
    public float CreditsMult = 1;

    [Header("Dynamic, dont't set here")]
    // bool spacebarThrowsGrenades = true;
    public float zoomBias = 0f;
    public float zoomBiasTouchscreen = 0f;
    public float joystickCursorSpeed;
    public float joystickAccelSpeed;
    public bool isVirtualAimJoystickVisible = true;

    WaveController waveController;

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
    }

    void Start()
    {
        if (ProfileManager.Instance.currentProfile == null)
        {
            Debug.Log("SettingsManager: No current profile found in ProfileManager on Start.");
            return;
        }
        if (!string.IsNullOrEmpty(ProfileManager.Instance.currentProfile.profileName))
            RefreshSettingsFromProfile(ProfileManager.Instance.currentProfile);
    }

    void OnEnable()
    {
        ProfileManager.OnProfileIsSet += RefreshSettingsFromProfile;
    }

    void OnDisable()
    {
        ProfileManager.OnProfileIsSet -= RefreshSettingsFromProfile;
    }

    public void RefreshSettingsFromProfile(PlayerProfile currentProfile)
    {
        if (currentProfile == null)
        {
            Debug.LogError("No profile found to load settings.");
            difficultyLevel = startingDifficulty;
            useStandardWASD = true;
            //spacebarThrowsGrenades = true;
            useInstantJoystickAim = true;
            joystickCursorSpeed = CursorManager.Instance.cursorSpeedDefault;
            joystickAccelSpeed = CursorManager.Instance.cursorAccelSpeedDefault;
            isVirtualAimJoystickVisible = true;
            return;
        }
        Debug.Log($"Loading settings from profile: {currentProfile.profileName}");

        difficultyLevel = currentProfile.difficultyLevel;
        useStandardWASD = currentProfile.isStandardWASD;
        //spacebarThrowsGrenades = currentProfile.isSpacebarEnabled;
        useInstantJoystickAim = currentProfile.isFastJoystickAimEnabled;
        if (currentProfile.joystickCursorSpeed < CursorManager.Instance.cursorSpeedMin)
        {
            currentProfile.joystickCursorSpeed = CursorManager.Instance.cursorSpeedDefault;
            currentProfile.joystickAccelSpeed = CursorManager.Instance.cursorAccelSpeedDefault;
            Debug.LogWarning($"Profile: {currentProfile} had an abnormally slow cursor speed, resetting to default");
            ProfileManager.Instance.SaveCurrentProfile();
        }
        joystickCursorSpeed = currentProfile.joystickCursorSpeed;
        joystickAccelSpeed = currentProfile.joystickAccelSpeed;
        isVirtualAimJoystickVisible = !currentProfile.virtualAimJoystickDisabled;
        //Debug.Log($"Settings loaded: WASD = {useStandardWASD}, Difficulty = {difficultyLevel}, Spacebar = {spacebarThrowsGrenades}, VirtualAimJoystickVisible = {isVirtualAimJoystickVisible}");

        ApplyMovementSettings();
        ApplyControlSettings();
    }
        
    public void ApplyGameplaySettings()
    {
        ApplyDifficultySettings();
        ApplyMovementSettings();
        ApplyControlSettings();

        //Debug.Log("Settings Manager finished applying settings");
    }

    #region Difficulty

    void ApplyDifficultySettings()
    {
        waveController = FindObjectOfType<WaveController>();
        if (waveController == null)
        {
            Debug.LogWarning("Wave Controller not found when applying difficulty settings");
            return;
        };

        switch (difficultyLevel)
        {
            case DifficultyLevel.Easy:
                WaveDifficultyMult = 0.7f;
                WavesTillAddWaveBaseDifficultyAdjust = -1;
                SubwaveListGrowthFactor = 0.8f;
                SubwaveDelayMult = 1.3f;
                BatchSpawnMult = 0.7f;
                CreditsMult = 2f;
                BuildingSystem.PlayerCredits = 1000;
                MessageBanner.PulseMessage($"You received $1000 to help you through easy mode", Color.cyan);
                break;

            case DifficultyLevel.Normal:
                WaveDifficultyMult = 1;
                WavesTillAddWaveBaseDifficultyAdjust = 0;
                SubwaveListGrowthFactor = 1f;
                SubwaveDelayMult = 1;
                BatchSpawnMult = 1;
                CreditsMult = 1;
                BuildingSystem.PlayerCredits = 0;
                break;

            case DifficultyLevel.Hard:
                WaveDifficultyMult = 1.5f;
                WavesTillAddWaveBaseDifficultyAdjust = 1;
                SubwaveListGrowthFactor = 1.2f;
                SubwaveDelayMult = 0.8f;
                BatchSpawnMult = 1.2f;
                CreditsMult = 0.7f;
                BuildingSystem.PlayerCredits = 0;
                break;
        }

        switch (InputManager.LastUsedDevice)
        {
            case Touchscreen device:
                // On touchscreen, slow down enemy spawn rate.
                SubwaveDelayMult *= 1.3f;
                break;
            case Gamepad device:
                // On gamepad, slow down enemy spawn rate.
                SubwaveDelayMult *= 1.15f;
                break;
        }

        //DeathManager deathManager = FindObjectOfType<DeathManager>();

        Debug.Log("Difficulty settings applied for " + difficultyLevel + " by Settings Manager");
    }

    #endregion

    #region Movement

    void ApplyMovementSettings()
    {
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.useStandardWASD = useStandardWASD;
            player.useFastJoystickAim = useInstantJoystickAim;
            Debug.Log("Movement Type updated. Standard movement: " + useStandardWASD + ", Fast Joystick Aim: " + useInstantJoystickAim);
        }
        else
        {
            Debug.LogWarning("Player not found by SettingsManager");
        }
    }

    #endregion

    #region Controls

    public void ApplyZoomBias()
    {
        if (ProfileManager.Instance.currentProfile == null)
        {
            zoomBias = zoomBiasTouchscreen;
            return;
        }

        zoomBias = zoomBiasTouchscreen + ProfileManager.Instance.currentProfile.zoomBias;

        if (CameraController.Instance != null)
        {
            CameraController.Instance.mouseMixWeight = CameraController.Instance.mouseMixWeightStart - zoomBias * 0.05f;
            CameraController.Instance.alwaysLockToPlayer = ProfileManager.Instance.currentProfile.alwaysLockToPlayer;
        }
    }

    void ApplyControlSettings()
    {
        ApplyZoomBias();

        CursorManager.Instance.joystickCursorSpeed = joystickCursorSpeed;
        CursorManager.Instance.cursorAcceleration = joystickAccelSpeed;
        TouchManager.Instance.SetVirtualAimJoystickVisible(isVirtualAimJoystickVisible);
    }

    #endregion
}