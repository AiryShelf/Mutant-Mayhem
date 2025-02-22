using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeviceObjectActivator : MonoBehaviour
{
    [SerializeField] GameObject objectToToggle;
    [SerializeField] bool showOnKeyboard;
    [SerializeField] bool showOnController;
    [SerializeField] bool showOnTouchscreen;


    void Start()
    {
        InputManager.Instance.LastUsedDeviceChanged += OnLastUsedDeviceChanged;

        ResetAndUpdate();
    }

    void OnDestroy()
    {
        InputManager.Instance.LastUsedDeviceChanged -= OnLastUsedDeviceChanged;
    }

    void OnLastUsedDeviceChanged(InputDevice device)
    {
        if (device is Keyboard)
        {
            objectToToggle.SetActive(showOnKeyboard); // Show or hide based on showOnKeyboard
        }
        else if (device is Gamepad)
        {
            objectToToggle.SetActive(showOnController); // Show or hide based on showOnController
        }
        else if (device is Touchscreen)
        {
            objectToToggle.SetActive(showOnTouchscreen); // Show or hide based on showOnTouchscreen
        }
        else
        {
            objectToToggle.SetActive(false); // Default: hide if device type is not recognized
        }
    }

    public void ResetAndUpdate()
    {
        OnLastUsedDeviceChanged(InputManager.LastUsedDevice);
    }
}
