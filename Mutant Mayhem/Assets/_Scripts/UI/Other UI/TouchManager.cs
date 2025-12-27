using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class TouchManager : MonoBehaviour
{
    public static TouchManager Instance { get; private set; }
    
    public VirtualJoystick moveJoystick;
    public Button shootButtonLeft;
    public Button meleeButtonLeft;
    public RectTransform moveJoystickDeadzoneRect;
    public VirtualJoystick aimJoystick;
    public Button shootButtonRight;
    public Button meleeButtonRight;
    public RectTransform aimJoystickDeadzoneRect;
    public RectTransform rightSideButtonsDeadzoneRect;
    public UIBuildMenuController buildMenuController;
    public RectTransform buildPanelRect;
    public Player player;
    [SerializeField] float upgradePanelSwipeDistance = 200;
    [SerializeField] float buildPanelSwipeDistance = 80;
    

    private Dictionary<int, TouchData> activeTouches = new Dictionary<int, TouchData>();
    // Fingers that are allowed to control (or be promoted to) Look/aim.
    // Interactive UI controls (Buttons, etc.) should NEVER be aim-eligible.
    private HashSet<int> aimEligibleFingerIds = new HashSet<int>();
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

    public void ShowVirtualAimJoysticks(bool show)
    {
        if (InputManager.LastUsedDevice == Touchscreen.current && !CursorManager.Instance.inMenu)
        {
            moveJoystick.ActivateJoystick(show);
            ShowLeftSideAttackButtons(show);

            if (virtualAimJoystickVisible)
            {
                aimJoystick.ActivateJoystick(show);
                aimJoystickDeadzoneRect.gameObject.SetActive(show);

                shootButtonRight.gameObject.SetActive(false);
                meleeButtonRight.gameObject.SetActive(false);
                rightSideButtonsDeadzoneRect.gameObject.SetActive(false);
            }
            else
            {
                // Show right-side attack buttons when aim joystick is hidden
                aimJoystick.ActivateJoystick(false);
                aimJoystickDeadzoneRect.gameObject.SetActive(false);

                rightSideButtonsDeadzoneRect.gameObject.SetActive(show);
                shootButtonRight.gameObject.SetActive(show);
                meleeButtonRight.gameObject.SetActive(show);
            }
            
            moveJoystick.ResetJoystick();
            aimJoystick.ResetJoystick();
        }
        else 
        {
            moveJoystick.ActivateJoystick(false);
            moveJoystickDeadzoneRect.gameObject.SetActive(false);
            aimJoystick.ActivateJoystick(false);
            aimJoystickDeadzoneRect.gameObject.SetActive(false);
            shootButtonRight.gameObject.SetActive(false);
            meleeButtonRight.gameObject.SetActive(false);
            rightSideButtonsDeadzoneRect.gameObject.SetActive(false);
            shootButtonLeft.gameObject.SetActive(false);
            meleeButtonLeft.gameObject.SetActive(false);

            moveJoystick.ResetJoystick();
            aimJoystick.ResetJoystick();
        }

        RefreshScreenBounds();
    }
    
    public void ShowLeftSideAttackButtons(bool show)
    {
        if (InputManager.LastUsedDevice == Touchscreen.current)
        {
            moveJoystickDeadzoneRect.gameObject.SetActive(show);
            shootButtonLeft.gameObject.SetActive(show);
            meleeButtonLeft.gameObject.SetActive(show);
        }
        else
        {
            moveJoystickDeadzoneRect.gameObject.SetActive(false);
            shootButtonLeft.gameObject.SetActive(false);
            meleeButtonLeft.gameObject.SetActive(false);
        }
    }

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
        // IMPORTANT: Interactive UI (Buttons, etc.) should NEVER be aim-eligible.
        if (IsPointerOverInteractiveUI(position, out GameObject hitInteractiveUIObject))
        {
            // Ensure this finger can never become Look/aim.
            aimEligibleFingerIds.Remove(fingerId);

            // Still allow joysticks/build menu to be handled as before.
            if (IsInRegion(position, moveJoystick.transform as RectTransform))
            {
                activeTouches[fingerId] = new TouchData(fingerId, TouchPurpose.Joystick, position);
            }
            else if (IsInRegion(position, aimJoystick.transform as RectTransform))
            {
                activeTouches[fingerId] = new TouchData(fingerId, TouchPurpose.Joystick, position);
            }
            else if (player != null && IsInRegion(position, buildPanelRect))
            {
                activeTouches[fingerId] = new TouchData(fingerId, TouchPurpose.BuildMenu, position);
                buildMenuController.isTouchScrolling = true;
            }
            else
            {
                activeTouches[fingerId] = new TouchData(fingerId, TouchPurpose.UI, position);
            }

            return;
        }

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
            else if (player != null && IsInRegion(position, buildPanelRect))   
            {
                // Build menu
                activeTouches[fingerId] = new TouchData(fingerId, TouchPurpose.BuildMenu, position);
                buildMenuController.isTouchScrolling = true;
            }
            else if (IsInRegion(position, moveJoystickDeadzoneRect) || 
                     IsInRegion(position, aimJoystickDeadzoneRect) || 
                     IsInRegion(position, rightSideButtonsDeadzoneRect))
            {
                // Deadzone area for joysticks
                activeTouches[fingerId] = new TouchData(fingerId, TouchPurpose.UI, position);
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
                {
                    aimEligibleFingerIds.Add(fingerId);
                    AddShootTouch(wasExisting, fingerId, position);
                }
                else
                {
                    activeTouches[fingerId] = new TouchData(fingerId, TouchPurpose.UI, position);
                }
            }
        }
        else
        {
            aimEligibleFingerIds.Add(fingerId);
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
        float dragThreshold = upgradePanelSwipeDistance;

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
            case TouchPurpose.BuildMenu:
                float dragDeltaY = position.y - data.lastScrollCheckPos.y;
                dragThreshold = buildPanelSwipeDistance;
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
        aimEligibleFingerIds.Remove(fingerId);

        switch (data.purpose)
        {
            case TouchPurpose.Joystick:
                // End joystick usage
                //moveJoystick.OnTouchEnd(fingerId);
                //aimJoystick.OnTouchEnd(fingerId);
                break;
            case TouchPurpose.Look:
                // Assign a new Look touch if shooting/meleeing, but ONLY if that touch is aim-eligible.
                // This prevents button touches (interactive UI) from stealing the cursor when the aim finger lifts.
                foreach (var kvp in activeTouches)
                {
                    int otherId = kvp.Key;
                    TouchData otherTouch = kvp.Value;

                    // Only allow promotion if this finger was marked aim-eligible at touch begin.
                    if (!aimEligibleFingerIds.Contains(otherId))
                        continue;

                    if (otherTouch.purpose == TouchPurpose.Shoot)
                    {
                        otherTouch.purpose = TouchPurpose.Look;
                        activeTouches[otherId] = otherTouch;

                        player.animControllerPlayer.FireInput_Cancelled(new InputAction.CallbackContext());
                        BeginTouch(otherId, otherTouch.currentPosition, true);
                        break;
                    }
                    else if (otherTouch.purpose == TouchPurpose.Melee)
                    {
                        otherTouch.purpose = TouchPurpose.Look;
                        activeTouches[otherId] = otherTouch;

                        player.animControllerPlayer.MeleeInput_Cancelled(new InputAction.CallbackContext());
                        BeginTouch(otherId, otherTouch.currentPosition, true);
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
                //EventSystem.current.SetSelectedGameObject(null);
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
            
            if (aimEligibleFingerIds.Contains(touchData.fingerId))
            {
                CursorManager.Instance.MoveCustomCursorTo(touchData.currentPosition, CursorRangeType.Bounds, Vector2.zero, 0, screenBounds);
                player.lastAimDir = Camera.main.ScreenToWorldPoint(touchData.currentPosition) - player.transform.position;
            }
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

    private bool IsInRegion(Vector2 screenPos, RectTransform rect)
    {
        if (rect == null) return false;

        // Convert the screen position to local rect transform space
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPos, null, out localPoint);

        // Check if the localPoint is within the rect bounds
        return rect.rect.Contains(localPoint);
    }

    private bool IsPointerOverInteractiveUI(Vector2 screenPosition, out GameObject hitUIObject)
    {
        // UI Raycast
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        // Consider any Selectable (Button/Toggle/Slider/etc.) or ScrollRect as interactive.
        // If any such control is in the raycast stack, treat this touch as NOT aim-eligible.
        foreach (var r in results)
        {
            if (r.gameObject == null) continue;

            if (r.gameObject.GetComponentInParent<Selectable>() != null ||
                r.gameObject.GetComponentInParent<ScrollRect>() != null)
            {
                hitUIObject = r.gameObject;
                return true;
            }
        }

        hitUIObject = null;
        return false;
    }

    #endregion
}

    