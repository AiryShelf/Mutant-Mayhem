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

    [SerializeField] GameObject analyticsPermissionPanel;

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
        if (ConsentStatus == AnalyticsConsentStatus.Unknown)
            OpenPermissionPanel();
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
            ConsentTimestamp = default(System.DateTime);
        }

        if (ConsentStatus == AnalyticsConsentStatus.Granted)
        {
            Initialize();
            AnalyticsEnabled = true;
        }
    }
    
    void OpenPermissionPanel()
    {
        // Activate the permission panel (Modified)
        if (analyticsPermissionPanel != null)
        {
            analyticsPermissionPanel.SetActive(true);
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
            analyticsPermissionPanel.SetActive(false);
        }
    }

    public void DenyConsent()
    {
        // Deny consent and hide panel (Modified)
        SetConsentStatus(AnalyticsConsentStatus.Denied);
        if (analyticsPermissionPanel != null)
        {
            analyticsPermissionPanel.SetActive(false);
        }
    }

    async void Initialize()
    {
        await UnityServices.InitializeAsync();
        Debug.Log("UnityServices initialised.");
        AnalyticsInitialized = true;
    }

    public void TrackWaveReached(int wave)
    {
        if (!AnalyticsInitialized)
        {
            Debug.LogWarning($"Attempted to track wave {wave} before analytics initialised.");
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
        var evt = new CustomEvent("wave_reached");
        evt.Add("wave_number", (long)wave);
        evt.Add("timestamp", System.DateTime.UtcNow.ToString("o"));
        evt.Add("time_in_game_seconds", (long)Time.time);

        AnalyticsService.Instance.RecordEvent(evt);
        Debug.Log($"Tracked wave_reached event: wave {wave}");
    }
}