using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.UI;

public class OptionsPanel : MonoBehaviour
{
    public FadeCanvasGroupsWave fadeGroup;
    [SerializeField] TMP_Dropdown qualityDropdown;
    [SerializeField] Slider zoomSlider;
    [SerializeField] Toggle alwaysLockToPlayerToggle;
    [SerializeField] Toggle vSyncToggle;
    [SerializeField] Toggle analyticsToggle;

    QualityManager qualityManager;

    void OnEnable()
    {
        qualityDropdown.onValueChanged.AddListener(QualityValueChanged);
        zoomSlider.onValueChanged.AddListener(ZoomBiasChanged);
        alwaysLockToPlayerToggle.onValueChanged.AddListener(AlwaysLockToPlayerChanged);
        vSyncToggle.onValueChanged.AddListener(ToggleVSync);
        analyticsToggle.onValueChanged.AddListener(ToggleAnalytics);

        AnalyticsManager.OnConsentStatusChanged += HandleAnalyticsConsentChanged;
    }

    void OnDisable()
    {
        // CHANGED: Remove listeners using the same method groups.
        qualityDropdown.onValueChanged.RemoveListener(QualityValueChanged);
        zoomSlider.onValueChanged.RemoveListener(ZoomBiasChanged);
        alwaysLockToPlayerToggle.onValueChanged.RemoveListener(AlwaysLockToPlayerChanged);
        vSyncToggle.onValueChanged.RemoveListener(ToggleVSync);
        analyticsToggle.onValueChanged.RemoveListener(ToggleAnalytics);

        AnalyticsManager.OnConsentStatusChanged -= HandleAnalyticsConsentChanged;
    }

    void Start()
    {
        qualityManager = SettingsManager.Instance.GetComponent<QualityManager>();
        if (qualityManager == null)
            Debug.LogError("OptionsPanel: Could not find QualityManager on SettingsManager object");

        InitializeUI();
    }

    public void InitializeUI()
    {
        // Check if a current profile exists, then set option values
        int qualityLevel = QualitySettings.GetQualityLevel();
        PlayerProfile currentProfile = ProfileManager.Instance.currentProfile;
        if (currentProfile != null)
        {
            qualityLevel = currentProfile.qualityLevel;
            if (qualityLevel == -1)
                qualityLevel = QualitySettings.GetQualityLevel();
        }

        // Populate UI elements
        qualityDropdown.SetValueWithoutNotify(qualityLevel);
        zoomSlider.SetValueWithoutNotify(currentProfile != null ? currentProfile.zoomBias : 0f);
        alwaysLockToPlayerToggle.SetIsOnWithoutNotify(currentProfile != null ? currentProfile.alwaysLockToPlayer : false);
        vSyncToggle.SetIsOnWithoutNotify(QualitySettings.vSyncCount > 0);
        analyticsToggle.SetIsOnWithoutNotify(AnalyticsManager.ConsentStatus == AnalyticsConsentStatus.Granted);
        
        if (analyticsToggle != null)
        {
            bool analyticsOn = (AnalyticsManager.ConsentStatus == AnalyticsConsentStatus.Granted) && AnalyticsManager.AnalyticsEnabled;
            analyticsToggle.SetIsOnWithoutNotify(analyticsOn);
        }
    }

    #region Input Callbacks

    void QualityValueChanged(int qualityLevel)
    {
        qualityManager.SetGraphicsQuality(qualityLevel);
        if (ProfileManager.Instance.currentProfile != null)
        {
            ProfileManager.Instance.currentProfile.qualityLevel = qualityLevel;
            ProfileManager.Instance.SaveCurrentProfile();
        }
        InitializeUI();
    }

    void ToggleVSync(bool isOn)
    {
        qualityManager.SetVSync(isOn);
    }

    void ToggleAnalytics(bool isOn)
    {
        // User is trying to enable analytics
        if (isOn)
        {
            // Otherwise, show the consent panel and let its callbacks handle consent.
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.OpenPermissionPanel(
                    AnalyticsManager.Instance.GrantConsent,
                    AnalyticsManager.Instance.DenyConsent
                );
            }
            else
            {
                Debug.LogWarning("OptionsPanel: Analytics.Instance is null, cannot request analytics consent.");
                analyticsToggle.SetIsOnWithoutNotify(false);
            }
        }
        else
        {
            // User explicitly turned analytics off from the options menu.
            if (AnalyticsManager.Instance != null)
            {
                AnalyticsManager.Instance.DenyConsent();
            }
            else
            {
                // Fallback if Analytics instance is missing.
                AnalyticsManager.ConsentStatus = AnalyticsConsentStatus.Denied;
                AnalyticsManager.AnalyticsEnabled = false;
                AnalyticsManager.StopDataCollection();
            }
        }
    }

    void ZoomBiasChanged(float sliderValue)
    {
        if (ProfileManager.Instance.currentProfile != null)
        {
            ProfileManager.Instance.currentProfile.zoomBias = sliderValue;
            ProfileManager.Instance.DelaySaveProfile();
        }
    }

    void AlwaysLockToPlayerChanged(bool isOn)
    {
        if (ProfileManager.Instance.currentProfile != null)
        {
            ProfileManager.Instance.currentProfile.alwaysLockToPlayer = isOn;
            ProfileManager.Instance.SaveCurrentProfile();

            CameraController.Instance.alwaysLockToPlayer = isOn;
        }
    }

    void HandleAnalyticsConsentChanged(AnalyticsConsentStatus status)
    {
        if (analyticsToggle == null)
            return;

        bool analyticsOn = (status == AnalyticsConsentStatus.Granted) && AnalyticsManager.AnalyticsEnabled;
        analyticsToggle.SetIsOnWithoutNotify(analyticsOn);

        MessageBanner.PulseMessage(
            $"Analytics consent changed to {status}. \nAnalytics is now {(analyticsOn ? "enabled" : "disabled")}.",
            analyticsOn ? Color.green : Color.red
        );
    }

    #endregion
}