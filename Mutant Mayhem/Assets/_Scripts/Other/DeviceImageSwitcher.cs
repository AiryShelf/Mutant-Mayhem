using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeviceImageSwitcher : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textToSwitch;
    [SerializeField] string keyboardText;
    [SerializeField] string touchscreenText;
    [SerializeField] string gamepadText;

    void OnEnable()
    {
        //InputController.LastUsedDeviceChanged += OnLastUsedDeviceChanged;
    }

    void OnDisable()
    {
        //InputController.LastUsedDeviceChanged -= OnLastUsedDeviceChanged;
        Debug.Log($"{gameObject} unsubscribed from LastUsedDeviceChanged text updates");
    }

    void OnLastUsedDeviceChanged(InputDevice device)
    {
        Debug.Log("DeviceTextSwitcher: Responding to device changed event...");

        if (InputController.LastUsedDevice == Keyboard.current)
            textToSwitch.text = keyboardText;
        else if (InputController.LastUsedDevice == Touchscreen.current)
            textToSwitch.text = touchscreenText;
        else if (InputController.LastUsedDevice == Gamepad.current)
            textToSwitch.text = gamepadText;
        /*
        switch (device)
        {
            case Keyboard keyboard:
            textToSwitch.text = keyboardText;
            break;

            case Touchscreen touchscreen:
            textToSwitch.text = touchscreenText;
            break;

            case Gamepad gamepad:
            textToSwitch.text = gamepadText;
            break;
        }
        */
    }
}
