using UnityEngine;
using UnityEngine.InputSystem;

public class VirtualJoystickManager : MonoBehaviour
{
    public static VirtualJoystickManager Instance;

    public VirtualJoystick leftJoystick;
    public VirtualJoystick rightJoystick;
    
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
    }
}
    /*
    public VirtualJoystick leftJoystick; // Reference your UI joystick script

    private VirtualJoystickDevice leftDevice;

    void Start()
    {
        leftDevice = InputSystem.AddDevice<VirtualJoystickDevice>();
        if(leftDevice == null)
        {
            Debug.LogError("Failed to add VirtualJoystickDevice");
        }
        else
        {
            Debug.Log("VirtualJoystickDevice added successfully.");
        }
    }

    void Update()
    {
        // NEW: Update the device with the current value from your UI joystick.
        Vector2 currentInput = leftJoystick.InputVector;
        var state = new VirtualJoystickState { stick = currentInput };

        // NEW: Queue the state event so that the Input System processes it.
        InputSystem.QueueStateEvent(leftDevice, state);

        // NEW: Optionally call InputSystem.Update() if needed (usually the system auto-updates each frame).
        // InputSystem.Update();
    }
}

*/

