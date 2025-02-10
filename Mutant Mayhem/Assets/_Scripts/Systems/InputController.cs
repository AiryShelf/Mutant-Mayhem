using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    public static InputDevice LastUsedDevice;

    [SerializeField] float cursorSpeedFactor = 30f;
    Vector2 lastMousePos;
    static bool joystickAsMouse = false;
    Player player;

    void Start()
    {
        if (Keyboard.current != null)
            LastUsedDevice = Keyboard.current;
        else if (Gamepad.current != null)
            LastUsedDevice = Gamepad.current;
        else if (Touchscreen.current != null)
            LastUsedDevice = Touchscreen.current;

        player = FindObjectOfType<Player>();
    }

    void Update()
    {
        CheckCurrentInputDevice();
        if (joystickAsMouse)
        {
            Debug.Log("Joystick as mouse is running");
            float joystickX = Input.GetAxis("RightStickHorizontal");
            float joystickY = Input.GetAxis("RightStickVertical");
            Vector2 joystickInput = new Vector2(joystickX, joystickY);

            Vector2 lastAimDir = joystickInput * cursorSpeedFactor * Time.deltaTime;
            Vector2 newCursorPos = CursorManager.Instance.GetCustomCursorUiPos() + lastAimDir;

            if (player.stats.playerShooter.isBuilding)
            {
                CursorManager.Instance.MoveCustomCursorToUi(newCursorPos, CursorRangeType.Radius, player.transform.position, 6f, new Rect());
            }
            else 
            {
                Rect screenBounds = new Rect(0, 0, Screen.width, Screen.height);
                CursorManager.Instance.MoveCustomCursorToUi(newCursorPos, CursorRangeType.Bounds, Vector2.zero, 0f, screenBounds);
            }
        }
    }

    public static void SetJoystickMouseControl(bool active)
    {
        joystickAsMouse = active;
        Debug.Log("joystickAsMouse set to " + active);
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
            LastUsedDevice = Keyboard.current;
            CursorManager.Instance.SetCursorVisible(true);
            CursorManager.Instance.SetCustomCursorVisible(false);
        }
        else if (Gamepad.current != null &&
                (Gamepad.current.buttonSouth.wasPressedThisFrame ||  // A / X button
                 Gamepad.current.buttonNorth.wasPressedThisFrame ||  // Y / Triangle
                 Gamepad.current.buttonEast.wasPressedThisFrame ||   // B / Circle
                 Gamepad.current.buttonWest.wasPressedThisFrame ||   // X / Square
                 Gamepad.current.leftStick.ReadValue() != Vector2.zero ||
                 Gamepad.current.rightStick.ReadValue() != Vector2.zero))
        {
            LastUsedDevice = Gamepad.current;
            CursorManager.Instance.SetCursorVisible(false);
            CursorManager.Instance.SetCustomCursorVisible(true);
        }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            LastUsedDevice = Touchscreen.current;
            CursorManager.Instance.SetCursorVisible(false);
            CursorManager.Instance.SetCustomCursorVisible(true);
        }

        //Debug.Log(LastUsedDevice);
    }

    #endregion
}
