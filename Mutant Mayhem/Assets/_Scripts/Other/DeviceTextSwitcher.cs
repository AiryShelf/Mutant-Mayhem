using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeviceTextSwitcher : MonoBehaviour
{
    [Header("Text Adjustments")]
    [SerializeField] TextMeshProUGUI textToSwitch;
    [SerializeField] string stringToReplace_BlankAll; // String to replace in text
    
    [Header("Replacement Text")]
    [SerializeField] string keyboardText;
    [SerializeField] string touchscreenText;
    [SerializeField] string gamepadText;

    InputController inputController;

    void OnEnable()
    {
        inputController = InputController.Instance;
        inputController.LastUsedDeviceChanged += OnLastUsedDeviceChanged;

        OnLastUsedDeviceChanged(InputController.LastUsedDevice);
    }

    void OnDisable()
    {
        Debug.Log($"{gameObject} unsubscribing from LastUsedDeviceChanged text updates");
        inputController.LastUsedDeviceChanged -= OnLastUsedDeviceChanged;
        
    }

    void OnLastUsedDeviceChanged(InputDevice device)
    {
        Debug.Log("DeviceTextSwitcher: Responding to device changed event...");
        
        string newText = GetDeviceText();

        if (textToSwitch != null)
        {
            if (string.IsNullOrEmpty(stringToReplace_BlankAll))
            {
                // Replace the entire text
                if (!string.IsNullOrEmpty(newText))
                    textToSwitch.text = newText;
            }
            else
            {
                // Replace only the specific string within the text
                textToSwitch.text = textToSwitch.text.Replace(stringToReplace_BlankAll, newText);
                stringToReplace_BlankAll = newText;
            }   
        }
    }

    string GetDeviceText()
    {
        if (InputController.LastUsedDevice == Keyboard.current)
            return keyboardText;
        else if (InputController.LastUsedDevice == Touchscreen.current)
            return touchscreenText;
        else if (InputController.LastUsedDevice == Gamepad.current)
            return gamepadText;

        return string.Empty;
    }
}
