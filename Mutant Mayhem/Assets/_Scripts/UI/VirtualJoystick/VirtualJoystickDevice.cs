using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

/*
[StructLayout(LayoutKind.Explicit, Size = 8)]
public struct VirtualJoystickState : IInputStateTypeInfo
{
    // Format identifier for custom device
    public FourCC format => new FourCC('V', 'J', 'S', 'T');

    [FieldOffset(0)]
    public Vector2 stick;
}

[InputControlLayout(stateType = typeof(VirtualJoystickState), displayName = "Virtual Joystick Device")]
public class VirtualJoystickDevice : InputDevice
{
    [InputControl(name = "stick", offset = 0, sizeInBits = 64, usage = "Left", layout = "Vector2")]
    public Vector2Control stick { get; private set; }

    protected override void FinishSetup()
    {
        base.FinishSetup();
        stick = GetChildControl<Vector2Control>("stick");
        if (stick == null)
            Debug.LogError("VirtualJoystickDevice: Could not find stick");
        else
            Debug.LogError($"Stick found: {stick}");

        
    }
}

*/
