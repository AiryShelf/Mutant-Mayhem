using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Analytics;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;

public class OptionsPanel : MonoBehaviour
{
    public FadeCanvasGroupsWave fadeGroup;
    [SerializeField] TMP_Dropdown qualityDropdown;
    //[SerializeField] TMP_Dropdown resolutionDropdown;
    [SerializeField] Toggle vSyncToggle;
    [SerializeField] Toggle analyticsToggle;
    [SerializeField] Toggle virtualAimStickToggle;

    QualityManager qualityManager;

    void OnEnable()
    {
        // CHANGED: Use method groups matching UnityEvent signatures.
        qualityDropdown.onValueChanged.AddListener(QualityValueChanged);
        vSyncToggle.onValueChanged.AddListener(ToggleVSync);
        analyticsToggle.onValueChanged.AddListener(ToggleAnalytics);
        virtualAimStickToggle.onValueChanged.AddListener(DisableVirtualAimJoystick);
        //resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

        Analytics.OnConsentStatusChanged += HandleAnalyticsConsentChanged;

        //Initialize();
    }

    void OnDisable()
    {
        // CHANGED: Remove listeners using the same method groups.
        qualityDropdown.onValueChanged.RemoveListener(QualityValueChanged);
        vSyncToggle.onValueChanged.RemoveListener(ToggleVSync);
        analyticsToggle.onValueChanged.RemoveListener(ToggleAnalytics);
        virtualAimStickToggle.onValueChanged.RemoveListener(DisableVirtualAimJoystick);
        //resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);

        Analytics.OnConsentStatusChanged -= HandleAnalyticsConsentChanged;
    }

    void Start()
    {
        qualityManager = SettingsManager.Instance.GetComponent<QualityManager>();
        if (qualityManager == null)
            Debug.LogError("OptionsPanel: Could not find QualityManager on SettingsManager object");

        Initialize();
    }

    public void Initialize()
    {
        // Check if a current profile exists, then set option values
        int qualityLevel = QualitySettings.GetQualityLevel();

        PlayerProfile currentProfile = ProfileManager.Instance.currentProfile;
        if (currentProfile != null)
        {
            qualityLevel = currentProfile.qualityLevel;
            if (qualityLevel == -1)
                qualityLevel = QualitySettings.GetQualityLevel();

            virtualAimStickToggle.SetIsOnWithoutNotify(currentProfile.virtualAimJoystickDisabled);
        }
        else
        {
            virtualAimStickToggle.SetIsOnWithoutNotify(true);
        }

        qualityDropdown.SetValueWithoutNotify(qualityLevel);
        vSyncToggle.SetIsOnWithoutNotify(QualitySettings.vSyncCount > 0);
        analyticsToggle.SetIsOnWithoutNotify(Analytics.ConsentStatus == AnalyticsConsentStatus.Granted);
        
        if (analyticsToggle != null)
        {
            bool analyticsOn = (Analytics.ConsentStatus == AnalyticsConsentStatus.Granted) && Analytics.AnalyticsEnabled;
            analyticsToggle.SetIsOnWithoutNotify(analyticsOn);
        }
        
        //PopulateResolutionDropdown();
    }

    void DisableVirtualAimJoystick(bool disabled)
    {
        PlayerProfile currentProfile = ProfileManager.Instance.currentProfile;
        if (currentProfile != null)
        {
            currentProfile.virtualAimJoystickDisabled = disabled;
            ProfileManager.Instance.SaveCurrentProfile();
            SettingsManager.Instance.RefreshSettingsFromProfile(currentProfile);
        }
    }

    #region Graphics

    void QualityValueChanged(int qualityLevel)
    {
        qualityManager.SetGraphicsQuality(qualityLevel);
        if (ProfileManager.Instance.currentProfile != null)
        {
            ProfileManager.Instance.currentProfile.qualityLevel = qualityLevel;
            ProfileManager.Instance.SaveCurrentProfile();
        }
        Initialize();
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
            if (Analytics.Instance != null)
            {
                Analytics.Instance.OpenPermissionPanel(
                    Analytics.Instance.GrantConsent,
                    Analytics.Instance.DenyConsent
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
            if (Analytics.Instance != null)
            {
                Analytics.Instance.DenyConsent();
            }
            else
            {
                // Fallback if Analytics instance is missing.
                Analytics.ConsentStatus = AnalyticsConsentStatus.Denied;
                Analytics.AnalyticsEnabled = false;
                Analytics.StopDataCollection();
            }
        }
    }

    #endregion

    void HandleAnalyticsConsentChanged(AnalyticsConsentStatus status)
    {
        if (analyticsToggle == null)
            return;

        bool analyticsOn = (status == AnalyticsConsentStatus.Granted) && Analytics.AnalyticsEnabled;
        analyticsToggle.SetIsOnWithoutNotify(analyticsOn);

        MessageBanner.PulseMessage(
            $"Analytics consent changed to {status}. Analytics is now {(analyticsOn ? "enabled" : "disabled")}.",
            analyticsOn ? Color.green : Color.red
        );
    }
}