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

    InputManager inputController;
    int subCount = 0;

    void OnEnable()
    {
        inputController = InputManager.Instance;
        if (inputController != null)
        {
            subCount++;
            inputController.LastUsedDeviceChanged += OnLastUsedDeviceChanged;
        }

        OnLastUsedDeviceChanged(InputManager.LastUsedDevice);
    }

    void OnDisable()
    {
        for (int i = 0; i < subCount; i++)
            inputController.LastUsedDeviceChanged -= OnLastUsedDeviceChanged;

        //Debug.Log($"{gameObject} unsubscribed from LastUsedDeviceChanged text updates");
    }

    void Start()
    {
        inputController = InputManager.Instance;
        if (inputController != null)
        {
            subCount++;
            inputController.LastUsedDeviceChanged += OnLastUsedDeviceChanged;
        }
        else
            Debug.LogError("DeviceImageSwitcher: Could not find InputController.Instance on Start");

        OnLastUsedDeviceChanged(InputManager.LastUsedDevice);
    }

    void OnLastUsedDeviceChanged(InputDevice device)
    {
        Debug.Log("DeviceImageSwitcher: Responding to device changed event...");

        if (InputManager.LastUsedDevice == Keyboard.current)
            imageToSwitch.sprite = keyboardSprite;
        else if (InputManager.LastUsedDevice == Touchscreen.current)
            imageToSwitch.sprite = touchscreenSprite;
        else if (InputManager.LastUsedDevice == Gamepad.current)
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
