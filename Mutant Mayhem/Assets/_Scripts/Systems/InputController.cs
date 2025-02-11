using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputController : MonoBehaviour
{
    public static InputDevice LastUsedDevice { get; private set; }
    static bool joystickAsMouse = false;
    
    Vector2 lastMousePos;

    void Start()
    {
        if (Keyboard.current != null)
            LastUsedDevice = Keyboard.current;
        else if (Gamepad.current != null)
            LastUsedDevice = Gamepad.current;
        else if (Touchscreen.current != null)
            LastUsedDevice = Touchscreen.current;
    }

    void Update()
    {
        EventSystem.current.sendNavigationEvents = !joystickAsMouse;

        CheckCurrentInputDevice();

        CursorManager.Instance.CustomCursorControl();
        CursorManager.Instance.CustomCursorHover();
    }

    public static void SetJoystickMouseControl(bool active)
    {
        if (LastUsedDevice == Gamepad.current)
        {
            joystickAsMouse = active;
            Debug.Log("joystickAsMouse set to " + active);
            return;
        }
        else
            joystickAsMouse = false;

        Debug.Log("joystickAsMouse set to false");
        
    }

    public static bool GetJoystickAsMouseState()
    {
        return joystickAsMouse;
    }

    #region CheckInputDevice

    void CheckCurrentInputDevice()
    {
        Vector2 mousePos = Input.mousePosition;
        if (Keyboard.current.anyKey.wasPressedThisFrame || mousePos != lastMousePos)
        {
            lastMousePos = mousePos;
            if (LastUsedDevice != Keyboard.current)
            {
                LastUsedDevice = Keyboard.current;
                SetJoystickMouseControl(false);
                CursorManager.Instance.SetCursorVisible(true);
                CursorManager.Instance.SetUsingCustomCursor(false);
                CursorManager.Instance.SetCustomCursorVisible(false);
            }
        }
        if (Gamepad.current != null && Gamepad.current.allControls.Any(control => control.IsPressed()))
        {
            if (LastUsedDevice != Gamepad.current)
            {
                LastUsedDevice = Gamepad.current;
                SetJoystickMouseControl(true);
                CursorManager.Instance.SetCursorVisible(false);
                CursorManager.Instance.SetUsingCustomCursor(true);
                CursorManager.Instance.SetCustomCursorVisible(true);
            }
        }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            if (LastUsedDevice != Touchscreen.current)
            {
                LastUsedDevice = Touchscreen.current;
                SetJoystickMouseControl(false);
                CursorManager.Instance.SetCursorVisible(false);
                CursorManager.Instance.SetUsingCustomCursor(true);
                CursorManager.Instance.SetCustomCursorVisible(true);
            }
        }

        //Debug.Log(LastUsedDevice);
    }

    #endregion
}
