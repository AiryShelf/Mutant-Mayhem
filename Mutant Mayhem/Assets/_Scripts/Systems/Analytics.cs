using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Analytics;

public class Analytics : MonoBehaviour
{
    public static Analytics Instance { get; private set; }

    private bool _analyticsInitialized = false;

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

    async void Start()
    {
        // Initialise Unity Services
        await UnityServices.InitializeAsync();
        Debug.Log("UnityServices initialised.");

        // Optional: if you have user consent flow, ensure consent is granted before enabling analytics collection
        // Example: EndUserConsent.SetConsentState(AnalyticsIntent.Analytics, ConsentStatus.Granted);

        _analyticsInitialized = true;
    }

    public void TrackWaveReached(int wave)
    {
        if (!_analyticsInitialized)
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