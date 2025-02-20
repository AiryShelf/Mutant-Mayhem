using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.Intrinsics;

public class TouchManager : MonoBehaviour
{
    public static TouchManager Instance { get; private set; }
    
    public VirtualJoystick moveJoystick;
    public VirtualJoystick aimJoystick;
    public Player player;

    private Dictionary<int, TouchData> activeTouches = new Dictionary<int, TouchData>();
    Rect screenBounds = new Rect(0,0,0,0);

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

        screenBounds = new Rect(0, 0, Screen.width, Screen.height);
    }

    void Update()
    {
        if (Touchscreen.current == null) return;

        // Iterate through all possible touches
        foreach (var touchControl in Touchscreen.current.touches)
        {
            if (!touchControl.press.isPressed) 
            {
                // If the finger was previously tracked but now is lifted
                if (activeTouches.ContainsKey(touchControl.touchId.ReadValue()))
                    EndTouch(touchControl.touchId.ReadValue());
                
                // Not pressed, skip
                continue;
            }

            // There's a finger pressed
            int fingerId = touchControl.touchId.ReadValue();
            Vector2 touchPos = touchControl.position.ReadValue();
            var phase = touchControl.phase.ReadValue();

            switch (phase)
            {
                case UnityEngine.InputSystem.TouchPhase.Began:
                    BeginTouch(fingerId, touchPos);
                    break;
                case UnityEngine.InputSystem.TouchPhase.Moved:
                //case UnityEngine.InputSystem.TouchPhase.Stationary:
                    MoveTouch(fingerId, touchPos);
                    break;
                // Ended/Canceled handled after loop checks isPressed = false
                // But in some devices, you might see Ended within the loop
                // We'll do a quick check to handle that if needed:
                case UnityEngine.InputSystem.TouchPhase.Ended:
                case UnityEngine.InputSystem.TouchPhase.Canceled:
                    EndTouch(fingerId);
                    break;
            }
        }
    }

    public void SetVirtualJoysticksActive(bool active)
    {
        if (InputManager.LastUsedDevice == Touchscreen.current && !CursorManager.Instance.inMenu)
        {
            moveJoystick.ActivateJoystick(active);
            aimJoystick.ActivateJoystick(active);
            moveJoystick.ResetJoystick();
            aimJoystick.ResetJoystick();
        }
        else 
        {
            moveJoystick.ActivateJoystick(false);
            aimJoystick.ActivateJoystick(false);
            moveJoystick.ResetJoystick();
            aimJoystick.ResetJoystick();
        }

        screenBounds = new Rect(0, 0, Screen.width, Screen.height);
    }

    public bool GetVirtualJoysticksActive()
    {
        return moveJoystick.isActiveAndEnabled;
    }

    private void BeginTouch(int fingerId, Vector2 position)
    {
        // Raycast to see if it's over UI
        if (IsPointerOverUI(position, out GameObject hitUIObject))
        {
            Debug.Log($"Touch hit {hitUIObject.name}");

            if (IsInJoystickRegion(position, moveJoystick))
            {
                activeTouches[fingerId] = new TouchData(fingerId, TouchPurpose.Joystick, position);
            }
            else if (IsInJoystickRegion(position, aimJoystick))
            {
                activeTouches[fingerId] = new TouchData(fingerId, TouchPurpose.Joystick, position);
            }
            else
            {
                // It's a UI button or other UI
                activeTouches[fingerId] = new TouchData(fingerId, TouchPurpose.UI, position);
            }
        }
        else
        {
            if (player != null)
            {
                activeTouches[fingerId] = new TouchData(fingerId, TouchPurpose.Shoot, position);

                // Check if there is already a finger down shooting
                List<int> existingShootFingerIds = new List<int>();
                foreach (var kvp in activeTouches)
                {
                    if (kvp.Value.purpose == TouchPurpose.Shoot || kvp.Value.purpose == TouchPurpose.Melee)
                    {
                        existingShootFingerIds.Add(kvp.Value.fingerId);
                    }
                }

                if (existingShootFingerIds.Count > 1)
                {
                    Debug.Log("Two-Finger Tap Detected! Trigger Melee instead!");
                    foreach(var id in existingShootFingerIds)
                        activeTouches[id].purpose = TouchPurpose.Melee;

                    activeTouches[fingerId].purpose = TouchPurpose.Melee;

                    player.animControllerPlayer.FireInput_Cancelled(new InputAction.CallbackContext());
                    player.animControllerPlayer.MeleeInput_Performed(new InputAction.CallbackContext()); 
                }
                else
                {
                    Debug.Log("Single-Finger Tap Detected!  Trigger Shoot");
                    player.animControllerPlayer.FireInput_Performed(new InputAction.CallbackContext());
                    
                    CursorManager.Instance.MoveCustomCursorTo(position, CursorRangeType.Bounds, Vector2.zero, 0, screenBounds);
                    player.lastAimDir = Camera.main.ScreenToWorldPoint(position) - player.transform.position;
                }
                // [CHANGE END] -----------------------------------------------
            }
            
        }
    }

    private void MoveTouch(int fingerId, Vector2 position)
    {
        if (!activeTouches.ContainsKey(fingerId)) return;

        TouchData data = activeTouches[fingerId];
        data.currentPosition = position;

        switch (data.purpose)
        {
            case TouchPurpose.Joystick:
                break;
            case TouchPurpose.Shoot:
                // Update aim or firing logic if you want continuous movement
                // e.g., playerShooter.OnFireTouchMove(fingerId, position);
                CursorManager.Instance.MoveCustomCursorTo(position, CursorRangeType.Bounds, Vector2.zero, 0, screenBounds);
                if (player != null)
                    player.lastAimDir = Camera.main.ScreenToWorldPoint(position) - player.transform.position;
                break;
            case TouchPurpose.Melee:
                CursorManager.Instance.MoveCustomCursorTo(position, CursorRangeType.Bounds, Vector2.zero, 0, screenBounds);
                if (player != null)
                    player.lastAimDir = Camera.main.ScreenToWorldPoint(position) - player.transform.position;
                break;
            case TouchPurpose.UI:
                break;
        }
    }

    private void EndTouch(int fingerId)
    {
        if (!activeTouches.ContainsKey(fingerId)) return;

        TouchData data = activeTouches[fingerId];
        activeTouches.Remove(fingerId);

        switch (data.purpose)
        {
            case TouchPurpose.Joystick:
                // End joystick usage
                //moveJoystick.OnTouchEnd(fingerId);
                //aimJoystick.OnTouchEnd(fingerId);
                break;
            case TouchPurpose.Shoot:
                // Stop firing if this was controlling firing
                if (player != null)
                    player.animControllerPlayer.FireInput_Cancelled(new InputAction.CallbackContext());
                break;
            case TouchPurpose.Melee:
                if (player != null)
                    CheckForLastMeleeTouch();
                break;
            case TouchPurpose.UI:
                // Release UI press
                // If it was a button press, you can finalize the click here
                break;
        };
    }

    void CheckForLastMeleeTouch()
    {
        int count = 0;
        TouchData touchData = new TouchData(0, TouchPurpose.None, Vector2.zero);
        foreach (var kvp in activeTouches)
        {
            if (kvp.Value.purpose == TouchPurpose.Melee)
            {
                count++;
                touchData = kvp.Value;
            }
        }

        if (count == 1)
        {
            touchData.purpose = TouchPurpose.Shoot;
            Debug.Log("One melee tap remaining, change to Shoot");
            player.animControllerPlayer.MeleeInput_Cancelled(new InputAction.CallbackContext());
            player.animControllerPlayer.FireInput_Performed(new InputAction.CallbackContext());
            
            CursorManager.Instance.MoveCustomCursorTo(touchData.currentPosition, CursorRangeType.Bounds, Vector2.zero, 0, screenBounds);
            player.lastAimDir = Camera.main.ScreenToWorldPoint(touchData.currentPosition) - player.transform.position;
        }
        else if (count == 0)
            player.animControllerPlayer.MeleeInput_Cancelled(new InputAction.CallbackContext());
    }

    private bool IsPointerOverUI(Vector2 screenPosition, out GameObject hitUIObject)
    {
        // Standard UI Raycast
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        if (results.Count > 0)
        {
            // Return the topmost UI object
            hitUIObject = results[0].gameObject;
            return true;
        }

        hitUIObject = null;
        return false;
    }

    // Example method to check if the position is in the joystick region
    private bool IsInJoystickRegion(Vector2 screenPos, VirtualJoystick joystick)
    {
        if (joystick == null) return false;
        RectTransform rect = joystick.GetComponent<RectTransform>();
        if (rect == null) return false;

        // Convert the screen position to local rect transform space
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPos, null, out localPoint);

        // Check if the localPoint is within the rect bounds
        return rect.rect.Contains(localPoint);
    }
}
