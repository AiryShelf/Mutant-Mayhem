using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlSettingsPanel : MonoBehaviour
{
    public FadeCanvasGroupsWave fadeGroup;
    [SerializeField] TMP_Dropdown movementTypeDropdown;
    [SerializeField] Toggle fastJoystickAimToggle;
    [SerializeField] Slider joystickCursorSpeedSlider;
    [SerializeField] Slider cursorAccelerationSlider;
    [SerializeField] Toggle spacebarToggle;

    void OnEnable()
    {
        // CHANGED: Use method groups matching UnityEvent signatures.
        movementTypeDropdown.onValueChanged.AddListener(MoveTypeValueChanged);
        fastJoystickAimToggle.onValueChanged.AddListener(FastJoystickAimToggle);
        joystickCursorSpeedSlider.onValueChanged.AddListener(JoystickCursorSpeedChanged);
        cursorAccelerationSlider.onValueChanged.AddListener(CursorAccelerationChanged);
        spacebarToggle.onValueChanged.AddListener(ToggleSpacebar);

        //Initialize();
    }

    void OnDisable()
    {
        // CHANGED: Remove listeners using the same method groups.
        movementTypeDropdown.onValueChanged.RemoveListener(MoveTypeValueChanged);
        fastJoystickAimToggle.onValueChanged.RemoveListener(FastJoystickAimToggle);
        joystickCursorSpeedSlider.onValueChanged.RemoveListener(JoystickCursorSpeedChanged);
        cursorAccelerationSlider.onValueChanged.RemoveListener(CursorAccelerationChanged);
        spacebarToggle.onValueChanged.RemoveListener(ToggleSpacebar);
    }

    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        // Check if a current profile exists, then set option values
        if (ProfileManager.Instance.currentProfile != null)
        {
            PlayerProfile profile = ProfileManager.Instance.currentProfile;

            movementTypeDropdown.SetValueWithoutNotify(profile.isStandardWASD ? 1 : 0);
            spacebarToggle.SetIsOnWithoutNotify(profile.isSpacebarEnabled);
            fastJoystickAimToggle.SetIsOnWithoutNotify(profile.isFastJoystickAimEnabled);

            float sliderValue = Mathf.InverseLerp(
                CursorManager.Instance.cursorSpeedMin,
                CursorManager.Instance.cursorSpeedMax,
                profile.joystickCursorSpeed
            );
            joystickCursorSpeedSlider.SetValueWithoutNotify(sliderValue);

            float accelSliderValue = Mathf.InverseLerp(
                CursorManager.Instance.cursorAccelMin,
                CursorManager.Instance.cursorAccelMax,
                profile.joystickAccelSpeed
            );
            cursorAccelerationSlider.SetValueWithoutNotify(accelSliderValue);
        }
        else
        {
            movementTypeDropdown.SetValueWithoutNotify(1); // Default to standard WASD
            spacebarToggle.SetIsOnWithoutNotify(true);
            fastJoystickAimToggle.SetIsOnWithoutNotify(false);
            joystickCursorSpeedSlider.SetValueWithoutNotify(0.5f);
            cursorAccelerationSlider.SetValueWithoutNotify(0.5f);
        }
    }

    #region Control Options

    public void ToggleSpacebar(bool isOn)
    {
        if (ProfileManager.Instance.currentProfile != null)
        {
            ProfileManager.Instance.currentProfile.isSpacebarEnabled = isOn;
            ProfileManager.Instance.SaveCurrentProfile();
        }

        SettingsManager.Instance.RefreshSettingsFromProfile(ProfileManager.Instance.currentProfile);
        Debug.Log("Spacebar throws grenades set to " + isOn);
    }

    void MoveTypeValueChanged(int value)
    {
        PlayerProfile profile = ProfileManager.Instance.currentProfile;
        switch (value)
        {
            case 0:
                profile.isStandardWASD = false;
                break;
            case 1:
                profile.isStandardWASD = true;
                break;
            default:
                Debug.LogError("Failed to change move type");
                break;
        }

        ProfileManager.Instance.SaveCurrentProfile();
        SettingsManager.Instance.RefreshSettingsFromProfile(profile);

        Debug.Log("useStandardWASD set to: " + value);
    }

    #endregion

    #region Joystick

    void JoystickCursorSpeedChanged(float sliderValue)
    {
        float value = CursorManager.Instance.joystickCursorSpeed;
        if (ProfileManager.Instance.currentProfile != null)
        {
            value = Mathf.Lerp(CursorManager.Instance.cursorSpeedMin, CursorManager.Instance.cursorSpeedMax, sliderValue);
            ProfileManager.Instance.currentProfile.joystickCursorSpeed = value;
            ProfileManager.Instance.SaveCurrentProfile();
        }

        SettingsManager.Instance.RefreshSettingsFromProfile(ProfileManager.Instance.currentProfile);
        Debug.Log("Joystick Cursor Speed set to " + value);
    }

    void CursorAccelerationChanged(float sliderValue)
    {
        float value = CursorManager.Instance.cursorAcceleration;
        if (ProfileManager.Instance.currentProfile != null)
        {
            value = Mathf.Lerp(CursorManager.Instance.cursorAccelMin, CursorManager.Instance.cursorAccelMax, sliderValue);
            ProfileManager.Instance.currentProfile.joystickAccelSpeed = value;
            ProfileManager.Instance.SaveCurrentProfile();
        }

        SettingsManager.Instance.RefreshSettingsFromProfile(ProfileManager.Instance.currentProfile);
        Debug.Log("Cursor Acceleration set to " + value);
    }

    void FastJoystickAimToggle(bool isOn)
    {
        //joystickCursorSpeedSlider.interactable = !isOn;
        if (ProfileManager.Instance.currentProfile != null)
        {
            ProfileManager.Instance.currentProfile.isFastJoystickAimEnabled = isOn;
            ProfileManager.Instance.SaveCurrentProfile();
        }

        SettingsManager.Instance.RefreshSettingsFromProfile(ProfileManager.Instance.currentProfile);
        Debug.Log("Fast Joystick Aim set to " + isOn);
    }

    #endregion
}