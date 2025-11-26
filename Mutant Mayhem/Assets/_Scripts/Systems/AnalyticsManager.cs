using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Analytics;
using System.Text;
using System;

public enum AnalyticsConsentStatus
{
    Unknown,
    Granted,
    Denied
}

public class AnalyticsManager : MonoBehaviour
{
    public static AnalyticsManager Instance { get; private set; }
    public static AnalyticsConsentStatus ConsentStatus = AnalyticsConsentStatus.Unknown;
    public static string ConsentVersion = "0";
    public static DateTime ConsentTimestamp;
    public static Action<AnalyticsConsentStatus> OnConsentStatusChanged;
    public static bool AnalyticsEnabled = false;
    public static bool CollectionInitialized = false;

    const string InstallIdKey = "InstallId";
    public static string GetInstallId()
    {
        if (PlayerPrefs.HasKey(InstallIdKey))
        {
            return PlayerPrefs.GetString(InstallIdKey);
        }

        string newId = System.Guid.NewGuid().ToString("N");
        PlayerPrefs.SetString(InstallIdKey, newId);
        PlayerPrefs.Save();
        return newId;
    }

    public static int PrevTotalPlayTime = 0;
    public static int PrevDamageToPlayer = 0;
    public static int PrevDamageToCube = 0;
    public static int PrevDamageToStructures = 0;
    public static int PrevAmountRepairedByPlayer = 0;
    public static int PrevAmountRepairedByDrones = 0;
    public static int PrevMeleeAttacksByPlayer = 0;
    public static int PrevShotsFiredPlayerBullets = 0;
    public static int PrevShotsFiredPlayerLasers = 0;
    public static int PrevGrenadesThrownByPlayer = 0;
    public static int PrevEnemiesKilledByPlayer = 0;
    public static int PrevEnemiesKilledByTurrets = 0;
    public static int PrevEnemiesKilledByDrones = 0;
    public static int PrevStructuresBuilt = 0;
    public static int PrevWallsBuilt = 0;
    public static int PrevTurretsBuilt = 0;
    public static int PrevStructuresLost = 0;

    // Optional snapshot strings to be populated by other systems (e.g., UpgradeManager, AugManager)
    public static string UpgradeSnapshot = string.Empty;
    public static string AugmentationSnapshot = string.Empty;

    [SerializeField] ConsentPanel consentPanel; 

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

        //UnityServices.InitializeAsync();
        CheckConsentStatus();
        //if (ConsentStatus == AnalyticsConsentStatus.Unknown)
            //OpenPermissionPanel();
    }

    #region Consent

    void CheckConsentStatus()
    {
        // Load consent status from PlayerPrefs (Modified)
        string statusString = PlayerPrefs.GetString("AnalyticsConsentStatus", "Unknown");
        if (System.Enum.TryParse(statusString, out AnalyticsConsentStatus savedStatus))
        {
            ConsentStatus = savedStatus;
        }
        else
        {
            ConsentStatus = AnalyticsConsentStatus.Unknown;
        }

        ConsentVersion = PlayerPrefs.GetString("AnalyticsConsentVersion", "0");

        long timestampTicks = PlayerPrefs.GetString("AnalyticsConsentTimestamp", "0") != "0" ? System.Convert.ToInt64(PlayerPrefs.GetString("AnalyticsConsentTimestamp", "0")) : 0;
        if (timestampTicks > 0)
        {
            ConsentTimestamp = new System.DateTime(timestampTicks);
        }
        else
        {
            ConsentTimestamp = System.DateTime.UtcNow;
        }

        if (ConsentStatus == AnalyticsConsentStatus.Granted)
        {
            StartCollection();
            AnalyticsEnabled = true;
        }

        Debug.Log($"Loaded analytics consent status: {ConsentStatus}, version: {ConsentVersion}, timestamp: {ConsentTimestamp}");
    }

    public void ShowAnalyticsPermission(
        UnityEngine.Events.UnityAction onYes,   // <-- These accept normal methods
        UnityEngine.Events.UnityAction onNo)
    {
        consentPanel.gameObject.SetActive(true);

        consentPanel.yesButton.onClick.RemoveAllListeners();
        consentPanel.noButton.onClick.RemoveAllListeners();

        // Adds the passed-in methods directly
        consentPanel.yesButton.onClick.AddListener(onYes);
        consentPanel.noButton.onClick.AddListener(onNo);

        // auto-close panel
        consentPanel.yesButton.onClick.AddListener(ClosePermissionPanel);
        consentPanel.noButton.onClick.AddListener(ClosePermissionPanel);
    }
    
    public void OpenPermissionPanel(
        UnityEngine.Events.UnityAction onYes,   // <-- These accept normal methods
        UnityEngine.Events.UnityAction onNo)
    {
        // Activate the permission panel (Modified)
        if (consentPanel != null)
        {
            ShowAnalyticsPermission(onYes, onNo);
            return;
        }
        else
            consentPanel = FindObjectOfType<ConsentPanel>();

        if (consentPanel != null)
        {
            ShowAnalyticsPermission(onYes, onNo);
        }
        else
        {
            Debug.LogError("Analytics: ConsentPanel not found in scene to open permission panel.");
        }
    }

    void ClosePermissionPanel()
    {
        // Deactivate the permission panel (Modified)
        if (consentPanel != null)
        {
            consentPanel.gameObject.SetActive(false);
        }
    }

    void SetConsentStatus(AnalyticsConsentStatus status)
    {
        ConsentStatus = status;
        if (status == AnalyticsConsentStatus.Granted)
        {
            StartCollection();
            AnalyticsEnabled = true;
        }
        else if (status == AnalyticsConsentStatus.Denied)
        {
            StopDataCollection();
        }

        // Save consent status, version, and timestamp to PlayerPrefs (Modified)
        PlayerPrefs.SetString("AnalyticsConsentStatus", status.ToString());
        PlayerPrefs.SetString("AnalyticsConsentVersion", ConsentVersion);
        ConsentTimestamp = System.DateTime.UtcNow;
        PlayerPrefs.SetString("AnalyticsConsentTimestamp", ConsentTimestamp.Ticks.ToString());
        PlayerPrefs.Save();

        OnConsentStatusChanged?.Invoke(status);
        Debug.Log($"Analytics consent status set to: {status}, version: {ConsentVersion}, timestamp: {ConsentTimestamp}");
    }

    public void GrantConsent()
    {
        // Grant consent and hide panel (Modified)
        SetConsentStatus(AnalyticsConsentStatus.Granted);
        if (consentPanel != null)
        {
            consentPanel.gameObject.SetActive(false);
        }
    }

    public void DenyConsent()
    {
        // Deny consent and hide panel (Modified)
        SetConsentStatus(AnalyticsConsentStatus.Denied);
        if (consentPanel != null)
        {
            consentPanel.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Tracking

    async void StartCollection()
    {
        try
        {
            await UnityServices.InitializeAsync();
            AnalyticsService.Instance.StartDataCollection();

            Debug.Log("UnityServices + Analytics initialised, data collection started.");
            CollectionInitialized = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize Unity Services/Analytics: {e}");
            CollectionInitialized = false;
        }
    }

    public static void StopDataCollection()
    {
        if (CollectionInitialized && AnalyticsService.Instance != null)
        {
            AnalyticsService.Instance.StopDataCollection();
            Debug.Log("Analytics data collection stopped.");
        }
        AnalyticsEnabled = false;
        CollectionInitialized = false;
    }

    public void TrackNightCompleted(int night)
    {
        if (!CollectionInitialized)
        {
            //Debug.LogWarning($"Attempted to track wave {wave} before analytics initialised.");
            return;
        }

        BuildAugmentationSnapshot();
        BuildUpgradeSnapshot();

        // Compute per-wave deltas from cumulative stats
        int damageToPlayerNow = (int)StatsCounterPlayer.DamageToPlayer;
        int waveDamageTaken = damageToPlayerNow - PrevDamageToPlayer;
        PrevDamageToPlayer = damageToPlayerNow;

        int damageToCubeNow = (int)StatsCounterPlayer.DamageToCube;
        int waveDamageToCube = damageToCubeNow - PrevDamageToCube;
        PrevDamageToCube = damageToCubeNow;

        int damageToStructuresNow = (int)StatsCounterPlayer.DamageToStructures;
        int waveDamageToStructures = damageToStructuresNow - PrevDamageToStructures;
        PrevDamageToStructures = damageToStructuresNow;

        int amountRepairedByPlayerNow = (int)StatsCounterPlayer.AmountRepairedByPlayer;
        int waveAmountRepairedByPlayer = amountRepairedByPlayerNow - PrevAmountRepairedByPlayer;
        PrevAmountRepairedByPlayer = amountRepairedByPlayerNow;

        int amountRepairedByDronesNow = (int)StatsCounterPlayer.AmountRepairedByDrones;
        int waveAmountRepairedByDrones = amountRepairedByDronesNow - PrevAmountRepairedByDrones;
        PrevAmountRepairedByDrones = amountRepairedByDronesNow;

        int meleeAttacksByPlayerNow = StatsCounterPlayer.MeleeAttacksByPlayer;
        int waveMeleeAttacksByPlayer = meleeAttacksByPlayerNow - PrevMeleeAttacksByPlayer;
        PrevMeleeAttacksByPlayer = meleeAttacksByPlayerNow;

        int shotsBulletsNow = StatsCounterPlayer.ShotsFiredPlayerBullets;
        int waveBulletShots = shotsBulletsNow - PrevShotsFiredPlayerBullets;
        PrevShotsFiredPlayerBullets = shotsBulletsNow;

        int shotsLasersNow = StatsCounterPlayer.ShotsFiredPlayerLasers;
        int waveLaserShots = shotsLasersNow - PrevShotsFiredPlayerLasers;
        PrevShotsFiredPlayerLasers = shotsLasersNow;

        int grenadesThrownNow = StatsCounterPlayer.GrenadesThrownByPlayer;
        int waveGrenadesThrown = grenadesThrownNow - PrevGrenadesThrownByPlayer;
        PrevGrenadesThrownByPlayer = grenadesThrownNow;

        int enemiesKilledPlayerNow = StatsCounterPlayer.EnemiesKilledByPlayer;
        int waveEnemiesKilledByPlayer = enemiesKilledPlayerNow - PrevEnemiesKilledByPlayer;
        PrevEnemiesKilledByPlayer = enemiesKilledPlayerNow;

        int enemiesKilledTurretsNow = StatsCounterPlayer.EnemiesKilledByTurrets;
        int waveEnemiesKilledByTurrets = enemiesKilledTurretsNow - PrevEnemiesKilledByTurrets;
        PrevEnemiesKilledByTurrets = enemiesKilledTurretsNow;

        int enemiesKilledDronesNow = StatsCounterPlayer.EnemiesKilledByDrones;
        int waveEnemiesKilledByDrones = enemiesKilledDronesNow - PrevEnemiesKilledByDrones;
        PrevEnemiesKilledByDrones = enemiesKilledDronesNow;

        int structuresBuiltNow = StatsCounterPlayer.StructuresBuilt;
        int waveStructuresBuilt = structuresBuiltNow - PrevStructuresBuilt;
        PrevStructuresBuilt = structuresBuiltNow;

        int wallsBuiltNow = StatsCounterPlayer.WallsBuilt;
        int waveWallsBuilt = wallsBuiltNow - PrevWallsBuilt;
        PrevWallsBuilt = wallsBuiltNow;

        int turretsBuiltNow = StatsCounterPlayer.TurretsBuilt;
        int waveTurretsBuilt = turretsBuiltNow - PrevTurretsBuilt;
        PrevTurretsBuilt = turretsBuiltNow;

        int structuresLostNow = StatsCounterPlayer.StructuresLost;
        int waveStructuresLost = structuresLostNow - PrevStructuresLost;
        PrevStructuresLost = structuresLostNow;

        var evt = new CustomEvent("wave_completed");
        evt.Add("install_id", GetInstallId());
        evt.Add("input_used", InputManager.Instance != null ? InputManager.LastUsedDevice.ToString() : "null");
        evt.Add("difficulty", SettingsManager.Instance != null ? SettingsManager.Instance.difficultyLevel.ToString() : "null");
        evt.Add("player_class", ClassManager.Instance != null ? ClassManager.Instance.selectedClass.ToString() : "null");
        evt.Add("augmentations", AugmentationSnapshot);
        evt.Add("upgrades", UpgradeSnapshot);
        evt.Add("player_credits", BuildingSystem.Instance != null ? (int)BuildingSystem.PlayerCredits : -1);
        evt.Add("research_points_total", ProfileManager.Instance != null ? ProfileManager.Instance.currentProfile.researchPoints : -1);
        evt.Add("research_points_unspent", AugManager.Instance != null ? AugManager.Instance.currentResearchPoints : -1);
        evt.Add("current_planet", PlanetManager.Instance != null ? PlanetManager.Instance.currentPlanet.bodyName : "null");
        evt.Add("session_seconds_in_app", (int)Time.realtimeSinceStartup);
        evt.Add("session_seconds", (int)StatsCounterPlayer.TotalPlayTime);
        evt.Add("night_number", night);
        evt.Add("wave_damage_taken_player", waveDamageTaken);
        evt.Add("wave_damage_taken_cube", waveDamageToCube);
        evt.Add("wave_damage_taken_structures", waveDamageToStructures);
        evt.Add("wave_amount_repaired_player", waveAmountRepairedByPlayer);
        evt.Add("wave_amount_repaired_drones", waveAmountRepairedByDrones);
        evt.Add("wave_melee_attacks_player", waveMeleeAttacksByPlayer);
        evt.Add("wave_shots_lasers", waveLaserShots);
        evt.Add("wave_shots_bullets", waveBulletShots);
        evt.Add("wave_grenades_thrown", waveGrenadesThrown);
        evt.Add("wave_kills_player", waveEnemiesKilledByPlayer);
        evt.Add("wave_kills_turrets", waveEnemiesKilledByTurrets);
        evt.Add("wave_kills_drones", waveEnemiesKilledByDrones);
        evt.Add("wave_builds_nonWall", waveStructuresBuilt - waveWallsBuilt);
        evt.Add("wave_builds_wall", waveWallsBuilt);
        evt.Add("wave_builds_turret", waveTurretsBuilt);
        evt.Add("wave_structures_lost", waveStructuresLost);
        evt.Add("drone_count_attack", DroneManager.Instance != null ? DroneManager.Instance.activeAttackDrones.Count : 0);
        evt.Add("drone_count_construction", DroneManager.Instance != null ? DroneManager.Instance.activeConstructionDrones.Count : 0);
        AnalyticsService.Instance.RecordEvent(evt);
        Debug.Log($"Tracked wave_completed event: wave {night}");
    }

    public void TrackPlayerDeath(string deathCause)
    {
        if (!CollectionInitialized)
        {
            //Debug.LogWarning($"Attempted to track player death before analytics initialised.");
            return;
        }

        BuildAugmentationSnapshot();
        BuildUpgradeSnapshot();

        var evt = new CustomEvent("player_death");
        AddEverythingToEvent(evt); 

        string safeDeathCause = string.IsNullOrEmpty(deathCause) ? "no data" : deathCause;
        evt.Add("death_cause", safeDeathCause);
        AnalyticsService.Instance.RecordEvent(evt);
        Debug.Log($"Tracked player_death event: cause {safeDeathCause}, wave {WaveController.Instance.currentWaveIndex}");
    }

    public void TrackCubeDestroyed(string destructionCause)
    {
        if (!CollectionInitialized)
        {
            //Debug.LogWarning($"Attempted to track player death before analytics initialised.");
            return;
        }

        BuildAugmentationSnapshot();
        BuildUpgradeSnapshot();

        var evt = new CustomEvent("cube_destroyed");
        AddEverythingToEvent(evt);

        string safeDeathCause = string.IsNullOrEmpty(destructionCause) ? "no data" : destructionCause;
        evt.Add("destruction_cause", safeDeathCause);
        AnalyticsService.Instance.RecordEvent(evt);
        Debug.Log($"Tracked cube_destroyed event: cause {safeDeathCause}, wave {WaveController.Instance.currentWaveIndex}");
    }

    public void TrackSessionhQuit()
    {
        if (!CollectionInitialized)
        {
            //Debug.LogWarning($"Attempted to track playthrough quit before analytics initialised.");
            return;
        }

        BuildAugmentationSnapshot();
        BuildUpgradeSnapshot();

        var evt = new CustomEvent("session_quit");
        AddEverythingToEvent(evt);
        AnalyticsService.Instance.RecordEvent(evt);
        Debug.Log($"Tracked session_quit event at wave {WaveController.Instance.currentWaveIndex}");
    }

    void AddEverythingToEvent(CustomEvent evt)
    {
        evt.Add("install_id", GetInstallId());
        evt.Add("input_used", InputManager.Instance != null ? InputManager.LastUsedDevice.ToString() : "null");
        evt.Add("difficulty", SettingsManager.Instance != null ? SettingsManager.Instance.difficultyLevel.ToString() : "null");
        evt.Add("player_class", ClassManager.Instance != null ? ClassManager.Instance.selectedClass.ToString() : "null");
        evt.Add("augmentations", AugmentationSnapshot);
        evt.Add("upgrades", UpgradeSnapshot);
        evt.Add("player_credits", BuildingSystem.Instance != null ? (int)BuildingSystem.PlayerCredits : -1);
        evt.Add("research_points_total", ProfileManager.Instance != null ? ProfileManager.Instance.currentProfile.researchPoints : -1);
        evt.Add("research_points_unspent", AugManager.Instance != null ? AugManager.Instance.currentResearchPoints : -1);
        evt.Add("current_planet", PlanetManager.Instance != null ? PlanetManager.Instance.currentPlanet.bodyName : "null");
        evt.Add("session_seconds_in_app", (int)Time.realtimeSinceStartup);
        evt.Add("session_seconds", (int)StatsCounterPlayer.TotalPlayTime);
        evt.Add("night_number", WaveController.Instance.currentWaveIndex);
        evt.Add("total_damage_taken_player", (int)StatsCounterPlayer.DamageToPlayer);
        evt.Add("total_damage_taken_cube", (int)StatsCounterPlayer.DamageToCube);
        evt.Add("total_damage_taken_structures", (int)StatsCounterPlayer.DamageToStructures);
        evt.Add("total_amount_repaired_player", (int)StatsCounterPlayer.AmountRepairedByPlayer);
        evt.Add("total_amount_repaired_drones", (int)StatsCounterPlayer.AmountRepairedByDrones);
        evt.Add("total_melee_attacks_player", StatsCounterPlayer.MeleeAttacksByPlayer);
        evt.Add("total_shots_lasers", StatsCounterPlayer.ShotsFiredPlayerLasers);
        evt.Add("total_shots_bullets", StatsCounterPlayer.ShotsFiredPlayerBullets);
        evt.Add("total_grenades_thrown", StatsCounterPlayer.GrenadesThrownByPlayer);
        evt.Add("total_kills_player", StatsCounterPlayer.EnemiesKilledByPlayer);
        evt.Add("total_kills_turrets", StatsCounterPlayer.EnemiesKilledByTurrets);
        evt.Add("total_kills_drones", StatsCounterPlayer.EnemiesKilledByDrones);
        evt.Add("total_builds_nonWall", StatsCounterPlayer.StructuresBuilt - StatsCounterPlayer.WallsBuilt);
        evt.Add("total_builds_wall", StatsCounterPlayer.WallsBuilt);
        evt.Add("total_builds_turret", StatsCounterPlayer.TurretsBuilt);
        evt.Add("total_structures_lost", StatsCounterPlayer.StructuresLost);
        evt.Add("drone_count_attack", DroneManager.Instance != null ? DroneManager.Instance.activeAttackDrones.Count : -1);
        evt.Add("drone_count_construction", DroneManager.Instance != null ? DroneManager.Instance.activeConstructionDrones.Count : -1);
    }

    public void TrackSessionStart()
    {
        if (!CollectionInitialized)
        {
            //Debug.LogWarning($"Attempted to track session start before analytics initialised.");
            return;
        }

        BuildAugmentationSnapshot();

        var evt = new CustomEvent("session_start");
        evt.Add("install_id", GetInstallId());
        evt.Add("input_used", InputManager.Instance != null ? InputManager.LastUsedDevice.ToString() : "null");
        evt.Add("difficulty", SettingsManager.Instance != null ? SettingsManager.Instance.difficultyLevel.ToString() : "null");
        evt.Add("player_class", ClassManager.Instance != null ? ClassManager.Instance.selectedClass.ToString() : "null");
        evt.Add("augmentations", AugManager.selectedAugsString);
        evt.Add("research_points_total", ProfileManager.Instance != null ? ProfileManager.Instance.currentProfile.researchPoints : -1);
        evt.Add("research_points_unspent", AugManager.Instance != null ? AugManager.Instance.currentResearchPoints : -1);
        evt.Add("current_planet", PlanetManager.Instance != null ? PlanetManager.Instance.currentPlanet.bodyName : "null");
        evt.Add("session_seconds_in_app", (int)Time.realtimeSinceStartup);
        evt.Add("session_seconds", (int)StatsCounterPlayer.TotalPlayTime);
        AnalyticsService.Instance.RecordEvent(evt);
        Debug.Log($"Tracked session_start event.");
    }

    #endregion

    #region Utility

    public static void BuildUpgradeSnapshot()
    {
        var mgr = UpgradeManager.Instance;
        if (mgr == null)
        {
            UpgradeSnapshot = string.Empty;
            return;
        }

        StringBuilder sb = new StringBuilder();

        void AppendDict<TKey>(string prefix, Dictionary<TKey, int> dict)
        {
            if (dict == null)
                return;

            foreach (var kvp in dict)
            {
                int level = kvp.Value;
                if (level <= 0)
                    continue; // omit level 0 upgrades

                if (sb.Length > 0)
                    sb.Append(';');

                sb.Append(prefix);
                sb.Append('.');
                sb.Append(kvp.Key);   // enum name, e.g. MoveSpeed, GunDamage
                sb.Append('=');
                sb.Append(level);
            }
        }

        // 1) PlayerStats
        AppendDict("Player", mgr.playerStatsUpgLevels);

        // 2) StructureStats
        AppendDict("Structure", mgr.structureStatsUpgLevels);

        // 3) Consumables
        AppendDict("Consumable", mgr.consumablesUpgLevels);

        // 4–6) Guns: Laser, SMG (bullet), RepairGun
        AppendDict("Laser", mgr.laserUpgLevels);
        AppendDict("SMG", mgr.bulletUpgLevels);
        AppendDict("RepairGun", mgr.repairGunUpgLevels);

        // 7) DroneStats
        AppendDict("Drone", mgr.droneStatsUpgLevels);

        UpgradeSnapshot = sb.ToString();
    }

    public static void BuildAugmentationSnapshot()
    {
        var mgr = AugManager.Instance;
        if (mgr == null || AugManager.selectedAugsWithLvls == null)
        {
            AugmentationSnapshot = string.Empty;
            return;
        }

        StringBuilder sb = new StringBuilder();

        foreach (var kvp in AugManager.selectedAugsWithLvls)
        {
            var augSO = kvp.Key;
            int lvl = kvp.Value;

            if (lvl == 0 || augSO == null)
                continue;

            if (sb.Length > 0)
                sb.Append(';');

            // The child class name of the augmentation
            string augName = augSO.GetType().Name;

            sb.Append(augName);
            sb.Append('=');
            sb.Append(lvl);
        }

        AugmentationSnapshot = sb.ToString();
    }

    #endregion
}