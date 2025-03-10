using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.Intrinsics;
using System.Runtime.InteropServices.WindowsRuntime;

public class TouchManager : MonoBehaviour
{
    public static TouchManager Instance { get; private set; }
    
    public VirtualJoystick moveJoystick;
    public VirtualJoystick aimJoystick;
    public RectTransform upgradePanelRect;
    public PanelSwitcher upgradePanelSwitcher;
    public UIBuildMenuController buildMenuController;
    public RectTransform buildPanelRect;
    public Player player;
    

    private Dictionary<int, TouchData> activeTouches = new Dictionary<int, TouchData>();
    Rect screenBounds = new Rect(0,0,0,0);
    bool virtualAimJoystickVisible;

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

        RefreshScreenBounds();
    }

    void Update()
    {
        if (Touchscreen.current == null) return;

        HashSet<int> currentFrameFingerIds = new HashSet<int>();

        // Iterate through all possible touches
        foreach (var touchControl in Touchscreen.current.touches)
        {
            int fingerId = touchControl.touchId.ReadValue();
            if (fingerId < 0) continue;

            currentFrameFingerIds.Add(fingerId);

            if (!touchControl.press.isPressed) 
            {
                // If the finger was previously tracked but now is lifted
                if (activeTouches.ContainsKey(touchControl.touchId.ReadValue()))
                    EndTouch(touchControl.touchId.ReadValue());
                
                // Not tracked, skip
                continue;
            }

            // There's a finger pressed
            //int fingerId = touchControl.touchId.ReadValue();
            Vector2 touchPos = touchControl.position.ReadValue();
            var phase = touchControl.phase.ReadValue();

            if (!activeTouches.ContainsKey(fingerId))
            {
                BeginTouch(fingerId, touchPos, false);
            }

            if (phase == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                MoveTouch(fingerId, touchPos);
            }
            else if (phase == UnityEngine.InputSystem.TouchPhase.Ended || phase == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                EndTouch(fingerId);
            }

            switch (phase)
            {
                case UnityEngine.InputSystem.TouchPhase.Began:
                    BeginTouch(fingerId, touchPos, false);
                    break;
                case UnityEngine.InputSystem.TouchPhase.Moved:
                //case UnityEngine.InputSystem.TouchPhase.Stationary:
                    MoveTouch(fingerId, touchPos);
                    break;
                case UnityEngine.InputSystem.TouchPhase.Ended:
                case UnityEngine.InputSystem.TouchPhase.Canceled:
                    EndTouch(fingerId);
                    break;
            }
        }

        // Cleanup, for fast touch-releases being missed
        List<int> missingFingerIds = new List<int>();
        foreach (var kvp in activeTouches)
        {
            if (!currentFrameFingerIds.Contains(kvp.Key))
            {
                // We never saw this ID in this frame's loop, so end it.
                missingFingerIds.Add(kvp.Key);
            }
        }
        foreach (int id in missingFingerIds)
        {
            EndTouch(id);
        }
    }

    public void SetVirtualAimJoystickVisible(bool visible)
    {
        virtualAimJoystickVisible = visible;
    }

    public bool GetVirtualAimJoystickVisible()
    {
        return virtualAimJoystickVisible;
    }

    public void SetVirtualJoysticksActive(bool active)
    {
        if (InputManager.LastUsedDevice == Touchscreen.current && !CursorManager.Instance.inMenu)
        {
            moveJoystick.ActivateJoystick(active);
            if (virtualAimJoystickVisible)
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

        RefreshScreenBounds();
    }

    //public void SetAimJoystickActive()

    public bool GetVirtualJoysticksActive()
    {
        return moveJoystick.isActiveAndEnabled;
    }

    public void RefreshScreenBounds()
    {
        screenBounds = new Rect(0, 0, Screen.width, Screen.height);
    }

    #region Tap/Move/Release

    private void BeginTouch(int fingerId, Vector2 position, bool wasExisting)
    {
        //Debug.Log($"TouchManager: Screen Resolution: {Screen.width}x{Screen.height}");
        //Debug.Log($"TouchManager: Native Resolution: {Screen.currentResolution.width}x{Screen.currentResolution.height}");
        //Debug.Log($"TouchManager: Render Scale: {ScalableBufferManager.widthScaleFactor}x{ScalableBufferManager.heightScaleFactor}");
        //Debug.Log($"TouchManager: Tap Position: {position}");

        // Raycast to see if it's over UI
        if (IsPointerOverUI(position, out GameObject hitUIObject))
        {
            //Debug.Log($"Touch hit {hitUIObject.name}");

            // Joysticks
            if (IsInRegion(position, moveJoystick.transform as RectTransform))
            {
                activeTouches[fingerId] = new TouchData(fingerId, TouchPurpose.Joystick, position);
            }
            else if (IsInRegion(position, aimJoystick.transform as RectTransform))
            {
                activeTouches[fingerId] = new TouchData(fingerId, TouchPurpose.Joystick, position);
            }
            else if (IsInRegion(position, upgradePanelRect))
            {
                // Upgrade Panel
                activeTouches[fingerId] = new TouchData(fingerId, TouchPurpose.UpgradePanel, position);
            }
            else if (player != null && IsInRegion(position, upgradePanelRect))   
            {
                // Build menu
                activeTouches[fingerId] = new TouchData(fingerId, TouchPurpose.BuildMenu, position);
                buildMenuController.isTouchScrolling = true;
                //CursorManager.Instance.MoveCustomCursorTo(position, CursorRangeType.Bounds, Vector2.zero, 0, screenBounds);
            }
            else
            {
                // This prevents accidental taps over UI Elements in the heat of battle (like trying to tap the 3rd finger down)
                bool isLooking = false;
                foreach (var kvp in activeTouches)
                {
                    if (kvp.Value.purpose == TouchPurpose.Look)
                    {
                        isLooking = true;
                        break;
                    }
                }

                // other UI
                if (isLooking)
                    AddShootTouch(wasExisting, fingerId, position);
                else
                    activeTouches[fingerId] = new TouchData(fingerId, TouchPurpose.UI, position);
            }
            
        }
        else
        {
            AddShootTouch(wasExisting, fingerId, position);
        }
    }

    void AddShootTouch(bool wasExisting, int fingerId, Vector2 position)
    {
        if (player != null)
            {
                if (!wasExisting)
                    activeTouches[fingerId] = new TouchData(fingerId, TouchPurpose.Shoot, position);

                List<int> existingLookFingerIds = new List<int>();
                foreach (var kvp in activeTouches)
                {
                    if (kvp.Value.purpose == TouchPurpose.Look)
                        existingLookFingerIds.Add(kvp.Value.fingerId);
                }

                // Check if there is already a finger down shooting
                List<int> existingShootFingerIds = new List<int>();
                foreach (var kvp in activeTouches)
                {
                    if (kvp.Value.purpose == TouchPurpose.Shoot || 
                        kvp.Value.purpose == TouchPurpose.Melee ||
                        kvp.Value.purpose == TouchPurpose.Look)
                    {
                        existingShootFingerIds.Add(kvp.Value.fingerId);
                    }
                }

                if (existingShootFingerIds.Count > 2)
                {
                    // Melee
                    //Debug.Log("Two-Finger Tap Detected! Trigger Melee instead!");
                    foreach(var id in existingShootFingerIds)
                    {
                        if (activeTouches[id].purpose == TouchPurpose.Shoot)
                            activeTouches[id].purpose = TouchPurpose.Melee;
                    }

                    activeTouches[fingerId].purpose = TouchPurpose.Melee;

                    player.animControllerPlayer.FireInput_Cancelled(new InputAction.CallbackContext());
                    player.animControllerPlayer.MeleeInput_Performed(new InputAction.CallbackContext()); 
                }
                else if (existingShootFingerIds.Count == 2)
                {
                    // Shoot
                    //Debug.Log("Single-Finger Tap Detected!  Trigger Shoot");
                    foreach(var id in existingShootFingerIds)
                    {
                        if (activeTouches[id].purpose == TouchPurpose.Melee)
                            activeTouches[id].purpose = TouchPurpose.Shoot;
                    }
                        
                    player.animControllerPlayer.FireInput_Performed(new InputAction.CallbackContext());
                }
                else 
                {
                    // Only one finger down, Look
                    activeTouches[fingerId].purpose = TouchPurpose.Look;
                    CursorManager.Instance.MoveCustomCursorTo(position, CursorRangeType.Bounds, Vector2.zero, 0, screenBounds);
                    player.lastAimDir = Camera.main.ScreenToWorldPoint(position) - player.transform.position;
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
            case TouchPurpose.Look:
                CursorManager.Instance.MoveCustomCursorTo(position, CursorRangeType.Bounds, Vector2.zero, 0, screenBounds);
                if (player != null)
                    player.lastAimDir = Camera.main.ScreenToWorldPoint(position) - player.transform.position;
                break;
            case TouchPurpose.Shoot:
                // Update aim or firing logic if you want continuous movement
                // e.g., playerShooter.OnFireTouchMove(fingerId, position);
                //CursorManager.Instance.MoveCustomCursorTo(position, CursorRangeType.Bounds, Vector2.zero, 0, screenBounds);
                //if (player != null)
                    //player.lastAimDir = Camera.main.ScreenToWorldPoint(position) - player.transform.position;
                break;
            case TouchPurpose.Melee:
                //CursorManager.Instance.MoveCustomCursorTo(position, CursorRangeType.Bounds, Vector2.zero, 0, screenBounds);
                //if (player != null)
                    //player.lastAimDir = Camera.main.ScreenToWorldPoint(position) - player.transform.position;
                break;
            case TouchPurpose.UI:
                break;
            case TouchPurpose.UpgradePanel:
                float dragDeltaX = position.x - data.lastScrollCheckPos.x;
                float dragThreshold = 60f;

                if (Mathf.Abs(dragDeltaX) > dragThreshold)
                {
                    // If positive dragDeltaY => user is dragging finger up => "scroll down" in menu
                    if (dragDeltaX > 0)
                        upgradePanelSwitcher.SwipeRight();
                    else
                        upgradePanelSwitcher.SwipeLeft();

                    // Reset lastScrollCheckPosY so we can do another incremental step
                    data.lastScrollCheckPos = position;
                }
                break;
            case TouchPurpose.BuildMenu:
                float dragDeltaY = position.y - data.lastScrollCheckPos.y;
                dragThreshold = 120f;

                if (Mathf.Abs(dragDeltaY) > dragThreshold)
                {
                    // If positive dragDeltaY => user is dragging finger up => "scroll down" in menu
                    if (dragDeltaY > 0)
                        buildMenuController.ScrollDown();
                    else
                        buildMenuController.ScrollUp();

                    // Reset lastScrollCheckPosY so we can do another incremental step
                    data.lastScrollCheckPos = position;
                }
                break;
        }

        activeTouches[fingerId] = data;
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
            case TouchPurpose.Look:
                // Assign a new Look touch if shooting/meleeing
                foreach (var kvp in activeTouches)
                {
                    if (kvp.Value.purpose == TouchPurpose.Shoot)
                    {
                        kvp.Value.purpose = TouchPurpose.Look;
                        player.animControllerPlayer.FireInput_Cancelled(new InputAction.CallbackContext());
                        BeginTouch(kvp.Value.fingerId, kvp.Value.currentPosition, true);
                        break;
                    }
                    else if (kvp.Value.purpose == TouchPurpose.Melee)
                    {
                        kvp.Value.purpose = TouchPurpose.Look;
                        player.animControllerPlayer.MeleeInput_Cancelled(new InputAction.CallbackContext());
                        BeginTouch(kvp.Value.fingerId, kvp.Value.currentPosition, true);
                        break;
                    }
                }
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
            case TouchPurpose.BuildMenu:
                buildMenuController.isTouchScrolling = false;
                break;
        };
    }

    #endregion

    #region Checks

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

        if (count == 0)
            player.animControllerPlayer.MeleeInput_Cancelled(new InputAction.CallbackContext());
        else if (count < 3)
        {
            touchData.purpose = TouchPurpose.Shoot;
            Debug.Log("One melee tap remaining, change to Shoot");
            player.animControllerPlayer.MeleeInput_Cancelled(new InputAction.CallbackContext());
            player.animControllerPlayer.FireInput_Performed(new InputAction.CallbackContext());
            
            CursorManager.Instance.MoveCustomCursorTo(touchData.currentPosition, CursorRangeType.Bounds, Vector2.zero, 0, screenBounds);
            player.lastAimDir = Camera.main.ScreenToWorldPoint(touchData.currentPosition) - player.transform.position;
        }
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
    private bool IsInRegion(Vector2 screenPos, RectTransform rect)
    {
        if (rect == null) return false;

        // Convert the screen position to local rect transform space
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPos, null, out localPoint);

        // Check if the localPoint is within the rect bounds
        return rect.rect.Contains(localPoint);
    }

    #endregion
}
