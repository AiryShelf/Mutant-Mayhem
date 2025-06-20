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

    string _stringToReplace;

    void Start()
    {
        InputManager.Instance.LastUsedDeviceChanged += OnLastUsedDeviceChanged;

        ResetAndUpdate();
    }

    void OnDestroy()
    {
        //Debug.Log($"{gameObject} unsubscribing from LastUsedDeviceChanged text updates");
        InputManager.Instance.LastUsedDeviceChanged -= OnLastUsedDeviceChanged;
    }

    void OnLastUsedDeviceChanged(InputDevice device)
    {
        //Debug.Log("DeviceTextSwitcher: Responding to device changed event...");
        
        string newText = GetDeviceText();

        if (textToSwitch != null)
        {
            if (string.IsNullOrEmpty(_stringToReplace))
            {
                // Replace the entire text
                if (!string.IsNullOrEmpty(newText))
                    textToSwitch.text = newText;
            }
            else
            {
                // Replace only the specific string within the text
                textToSwitch.text = textToSwitch.text.Replace(_stringToReplace, newText);
                _stringToReplace = newText;
            }   
        }
    }

    public void ResetAndUpdate()
    {
        _stringToReplace = stringToReplace_BlankAll;
        OnLastUsedDeviceChanged(InputManager.LastUsedDevice);
    }

    string GetDeviceText()
    {
        if (InputManager.LastUsedDevice == Keyboard.current)
            return keyboardText;
        else if (InputManager.LastUsedDevice == Touchscreen.current)
            return touchscreenText;
        else if (InputManager.LastUsedDevice == Gamepad.current)
            return gamepadText;

        return _stringToReplace;
    }
}
