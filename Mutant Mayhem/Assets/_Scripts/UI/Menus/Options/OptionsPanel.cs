using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsPanel : MonoBehaviour
{
    public FadeCanvasGroupsWave fadeGroup;
    [SerializeField] TMP_Dropdown qualityDropdown;
    [SerializeField] Toggle vSyncToggle;
    [SerializeField] Toggle tutorialToggle;

    QualityManager qualityManager;

    void OnEnable()
    {
        // CHANGED: Use method groups matching UnityEvent signatures.
        tutorialToggle.onValueChanged.AddListener(ToggleTutorial);
        qualityDropdown.onValueChanged.AddListener(QualityValueChanged);
        vSyncToggle.onValueChanged.AddListener(ToggleVSync);

        //Initialize();
    }

    void OnDisable()
    {
        // CHANGED: Remove listeners using the same method groups.
        tutorialToggle.onValueChanged.RemoveListener(ToggleTutorial);
        qualityDropdown.onValueChanged.RemoveListener(QualityValueChanged);
        vSyncToggle.onValueChanged.RemoveListener(ToggleVSync);
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
        if (ProfileManager.Instance.currentProfile != null)
        {
            PlayerProfile profile = ProfileManager.Instance.currentProfile;

            tutorialToggle.SetIsOnWithoutNotify(profile.isTutorialEnabled);
        }
        else
        {
            tutorialToggle.SetIsOnWithoutNotify(true);
        }

        qualityDropdown.SetValueWithoutNotify(QualitySettings.GetQualityLevel());
        vSyncToggle.SetIsOnWithoutNotify(QualitySettings.vSyncCount > 0);
    }

    public void ToggleTutorial(bool isOn)
    {
        TutorialManager.SetTutorialState(isOn);
    }

    #region Graphics

    void QualityValueChanged(int qualityLevel)
    {
        qualityManager.SetGraphicsQuality(qualityLevel);
        Initialize();
    }

    void ToggleVSync(bool isOn)
    {
        qualityManager.SetVSync(isOn);
    }

    #endregion
}