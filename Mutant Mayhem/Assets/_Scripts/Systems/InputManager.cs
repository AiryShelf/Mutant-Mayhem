using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public static InputDevice LastUsedDevice { get; private set; }
    bool joystickAsMouse = false;
    public event Action<InputDevice> LastUsedDeviceChanged;
    
    Vector2 lastMousePos = Vector2.zero;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (Application.platform == RuntimePlatform.Android ||
            Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Debug.Log("Forcing Touchscreen on mobile in Awake.");
            LastUsedDevice = Touchscreen.current; 
        }
        else
        {
            // If not on mobile, fall back to keyboard/gamepad checks
            if (Keyboard.current != null)
                LastUsedDevice = Keyboard.current;
            else if (Gamepad.current != null)
                LastUsedDevice = Gamepad.current;
            else if (Touchscreen.current != null)
                LastUsedDevice = Touchscreen.current;
        }
        
        //LogPlatform();
    }

    void Start()
    {
        SetUpCurrentDevice();
        LastUsedDeviceChanged?.Invoke(LastUsedDevice);
    }

    void Update()
    {
        //EventSystem.current.sendNavigationEvents = !joystickAsMouse;

        CheckCurrentInputDevice();
    }

    void LogPlatform()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
                Debug.Log("Running on Windows build");
                LastUsedDevice = Keyboard.current;
                break;
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.OSXEditor:
                Debug.Log("Running on macOS build");
                LastUsedDevice = Keyboard.current;
                break;
            case RuntimePlatform.LinuxPlayer:
                Debug.Log("Running on Linux build");
                LastUsedDevice = Keyboard.current;
                break;
            case RuntimePlatform.Android:
                Debug.Log("Running on Android build");
                LastUsedDevice = Touchscreen.current;
                break;
            case RuntimePlatform.IPhonePlayer:
                Debug.Log("Running on iOS build");
                LastUsedDevice = Touchscreen.current;
                break;
            case RuntimePlatform.XboxOne:
                Debug.Log("Running on Xbox One build");
                LastUsedDevice = Gamepad.current;
                break;
            // Add additional platforms as needed
            default:
                Debug.Log("Running on an unrecognized platform: " + Application.platform);
                break;
        }
    }

    public static void SetLastUsedDevice(InputDevice device)
    {
        LastUsedDevice = device;
    }

    public static void SetJoystickMouseControl(bool active)
    {
        Instance.joystickAsMouse = active;
        //Debug.Log("joystickAsMouse set to " + active);
    }

    public static bool GetJoystickAsMouseState()
    {
        return Instance.joystickAsMouse;
    }

    #region CheckInputDevice

    void CheckCurrentInputDevice()
    {
        Vector2 mousePos = Vector2.zero;
        if (Mouse.current != null)
            mousePos = Mouse.current.position.ReadValue();
        //Debug.Log($"Mouse Pos: {mousePos}");

        if (Keyboard.current != null && (Keyboard.current.anyKey.wasPressedThisFrame || mousePos != lastMousePos))
        {
            lastMousePos = mousePos;
            if (LastUsedDevice != Keyboard.current)
            {
                LastUsedDevice = Keyboard.current;
                SetUpCurrentDevice();
                //SetJoystickMouseControl(false);
            }
        }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            if (LastUsedDevice != Touchscreen.current)
            {
                LastUsedDevice = Touchscreen.current;
                SetUpCurrentDevice();
                //SetJoystickMouseControl(false);
            }
        }
        else if (Gamepad.current != null && Gamepad.current.allControls.Any(control => control.IsPressed()))
        {
            if (LastUsedDevice != Gamepad.current)
            {
                LastUsedDevice = Gamepad.current;
                SetUpCurrentDevice();
                //SetJoystickMouseControl(true);
            }
        }
    }

    void SetUpCurrentDevice()
    {
        switch (LastUsedDevice)
        {
            case Keyboard device:
                CursorManager.Instance.SetCursorVisible(true);
                CursorManager.Instance.SetUsingCustomCursor(false);
                CursorManager.Instance.SetCustomCursorVisible(false);
                TouchManager.Instance.SetVirtualJoysticksActive(false);
                LastUsedDeviceChanged?.Invoke(LastUsedDevice);
                break;
            case Touchscreen device:
                CursorManager.Instance.SetCursorVisible(false);
                CursorManager.Instance.SetUsingCustomCursor(true);
                CursorManager.Instance.SetCustomCursorVisible(true);
                TouchManager.Instance.SetVirtualJoysticksActive(true);
                LastUsedDeviceChanged?.Invoke(LastUsedDevice);
                break;
            case Gamepad device:
                CursorManager.Instance.SetCursorVisible(false);
                CursorManager.Instance.SetUsingCustomCursor(true);
                CursorManager.Instance.SetCustomCursorVisible(true);
                TouchManager.Instance.SetVirtualJoysticksActive(false);
                LastUsedDeviceChanged?.Invoke(LastUsedDevice);
                break;
        }

        Debug.Log($"LastUsedDevice set to: {LastUsedDevice}");
    }

    public static bool IsMobile()
    {
        return SystemInfo.deviceType == DeviceType.Handheld ||
            Application.platform == RuntimePlatform.Android ||
            Application.platform == RuntimePlatform.IPhonePlayer;
    }

    #endregion
}
