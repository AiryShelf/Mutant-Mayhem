using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Analytics;
public enum AnalyticsConsentStatus
{
    Unknown,
    Granted,
    Denied
}

public class Analytics : MonoBehaviour
{
    public static Analytics Instance { get; private set; }
    public static AnalyticsConsentStatus ConsentStatus = AnalyticsConsentStatus.Unknown;
    public static string ConsentVersion = "0";
    public static System.DateTime ConsentTimestamp;
    public static bool AnalyticsEnabled = false;
    public static bool AnalyticsInitialized = false;

    [SerializeField] PermissionPanel analyticsPermissionPanel;

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

        CheckConsentStatus();
        //if (ConsentStatus == AnalyticsConsentStatus.Unknown)
            //OpenPermissionPanel();
    }

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
            Initialize();
            AnalyticsEnabled = true;
        }

        Debug.Log($"Loaded analytics consent status: {ConsentStatus}, version: {ConsentVersion}, timestamp: {ConsentTimestamp}");
    }

    public void ShowAnalyticsPermission(
        UnityEngine.Events.UnityAction onYes,   // <-- These accept normal methods
        UnityEngine.Events.UnityAction onNo     
        )
    {
        analyticsPermissionPanel.gameObject.SetActive(true);

        analyticsPermissionPanel.yesButton.onClick.RemoveAllListeners();
        analyticsPermissionPanel.noButton.onClick.RemoveAllListeners();

        // Adds the passed-in methods directly
        analyticsPermissionPanel.yesButton.onClick.AddListener(onYes);
        analyticsPermissionPanel.noButton.onClick.AddListener(onNo);

        // auto-close panel
        analyticsPermissionPanel.yesButton.onClick.AddListener(ClosePermissionPanel);
        analyticsPermissionPanel.noButton.onClick.AddListener(ClosePermissionPanel);
    }
    
    public void OpenPermissionPanel(
        UnityEngine.Events.UnityAction onYes,   // <-- These accept normal methods
        UnityEngine.Events.UnityAction onNo 
        )
    {
        // Activate the permission panel (Modified)
        if (analyticsPermissionPanel != null)
        {
            ShowAnalyticsPermission(onYes, onNo);
        }
    }

    void ClosePermissionPanel()
    {
        // Deactivate the permission panel (Modified)
        if (analyticsPermissionPanel != null)
        {
            analyticsPermissionPanel.gameObject.SetActive(false);
        }
    }

    void SetConsentStatus(AnalyticsConsentStatus status)
    {
        ConsentStatus = status;
        if (status == AnalyticsConsentStatus.Granted)
        {
            Initialize();
            AnalyticsEnabled = true;
        }
        else if (status == AnalyticsConsentStatus.Denied)
        {
            AnalyticsEnabled = false;
        }

        // Save consent status, version, and timestamp to PlayerPrefs (Modified)
        PlayerPrefs.SetString("AnalyticsConsentStatus", status.ToString());
        PlayerPrefs.SetString("AnalyticsConsentVersion", ConsentVersion);
        ConsentTimestamp = System.DateTime.UtcNow;
        PlayerPrefs.SetString("AnalyticsConsentTimestamp", ConsentTimestamp.Ticks.ToString());
        PlayerPrefs.Save();
    }

    public void GrantConsent()
    {
        // Grant consent and hide panel (Modified)
        SetConsentStatus(AnalyticsConsentStatus.Granted);
        if (analyticsPermissionPanel != null)
        {
            analyticsPermissionPanel.gameObject.SetActive(false);
        }
    }

    public void DenyConsent()
    {
        // Deny consent and hide panel (Modified)
        SetConsentStatus(AnalyticsConsentStatus.Denied);
        if (analyticsPermissionPanel != null)
        {
            analyticsPermissionPanel.gameObject.SetActive(false);
        }
    }

    async void Initialize()
    {
        await UnityServices.InitializeAsync();
        Debug.Log("UnityServices initialised.");
        AnalyticsInitialized = true;
    }

    public void TrackWaveCompleted(int wave)
    {
        if (!AnalyticsInitialized)
        {
            //Debug.LogWarning($"Attempted to track wave {wave} before analytics initialised.");
            return;
        }

        // Keep events under 10 parameters for best performance
        // Keep parameter names under 40 characters
        // Keep string parameter values under 100 characters
        // Avoid using personally identifiable information (PII) in event names or parameters
        // Avoid using special characters in event names or parameter names
        // Avoid using reserved event names like "session_start" or "app_crash"
        // Avoid sending events too frequently (e.g., more than once per second)
        // Test your events in the Unity Analytics Debugger before deploying to production
        // See https://docs.unity.com/analytics/BestPractices.html for more info
        // See https://docs.unity.com/analytics/ImplementingAnalyticsEvents.html for more info

        var evt = new CustomEvent("wave_completed");
        evt.Add("version", Application.version);
        evt.Add("platform", Application.platform.ToString());

        evt.Add("planet_id", SystemInfo.deviceUniqueIdentifier);
        evt.Add("wave_number", (long)wave);
        evt.Add("time_playing_seconds", (double)StatsCounterPlayer.TotalPlayTime);
        evt.Add("player_bullet_shots", (long)StatsCounterPlayer.ShotsFiredPlayerBullets);
        evt.Add("player_laser_shots", (long)StatsCounterPlayer.ShotsFiredPlayerLasers);
        evt.Add("enemies_killed_by_player", (long)StatsCounterPlayer.EnemiesKilledByPlayer);
        evt.Add("enemies_killed_by_turrets", (long)StatsCounterPlayer.EnemiesKilledByTurrets);
        evt.Add("enemies_killed_by_drones", (long)StatsCounterPlayer.EnemiesKilledByDrones);
        evt.Add("non_wall_structures_built", (long)StatsCounterPlayer.StructuresBuilt - StatsCounterPlayer.WallsBuilt);
        evt.Add("walls_built", (long)StatsCounterPlayer.WallsBuilt);

        AnalyticsService.Instance.RecordEvent(evt);
        Debug.Log($"Tracked wave_completed event: wave {wave}");
    }

    public void TrackUpgradePurchased(string upgradeID, int cost)
    {
        if (!AnalyticsInitialized)
        {
            //Debug.LogWarning($"Attempted to track upgrade purchase before analytics initialised.");
            return;
        }

        var evt = new CustomEvent("upgrade_purchased");
        evt.Add("version", Application.version);
        evt.Add("platform", Application.platform.ToString());

        evt.Add("planet_id", SystemInfo.deviceUniqueIdentifier);
        evt.Add("wave_number", (long)WaveControllerRandom.Instance.currentWaveIndex);
        evt.Add("upgrade_id", upgradeID);
        evt.Add("cost", (long)cost);
        evt.Add("time_playing_seconds", (double)StatsCounterPlayer.TotalPlayTime);

        AnalyticsService.Instance.RecordEvent(evt);
        Debug.Log($"Tracked upgrade_purchased event: upgrade {upgradeID}, cost {cost}");
    }
}