using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DeviceImageSwitcher : MonoBehaviour
{
    [SerializeField] Image imageToSwitch;
    [SerializeField] Sprite keyboardSprite;
    [SerializeField] Sprite touchscreenSprite;
    [SerializeField] Sprite gamepadSprite;

    InputController inputController;

    void OnEnable()
    {
        inputController = InputController.Instance;
        inputController.LastUsedDeviceChanged += OnLastUsedDeviceChanged;

        OnLastUsedDeviceChanged(InputController.LastUsedDevice);
    }

    void OnDisable()
    {
        inputController.LastUsedDeviceChanged -= OnLastUsedDeviceChanged;
        //Debug.Log($"{gameObject} unsubscribed from LastUsedDeviceChanged text updates");
    }

    void Start()
    {
        inputController = InputController.Instance;
        if (inputController != null)
            inputController.LastUsedDeviceChanged += OnLastUsedDeviceChanged;
        else
            Debug.LogError("DeviceImageSwitcher: Could not find InputController.Instance on Start");

        OnLastUsedDeviceChanged(InputController.LastUsedDevice);
    }

    void OnLastUsedDeviceChanged(InputDevice device)
    {
        Debug.Log("DeviceImageSwitcher: Responding to device changed event...");

        if (InputController.LastUsedDevice == Keyboard.current)
            imageToSwitch.sprite = keyboardSprite;
        else if (InputController.LastUsedDevice == Touchscreen.current)
            imageToSwitch.sprite = touchscreenSprite;
        else if (InputController.LastUsedDevice == Gamepad.current)
            imageToSwitch.sprite = gamepadSprite;
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
