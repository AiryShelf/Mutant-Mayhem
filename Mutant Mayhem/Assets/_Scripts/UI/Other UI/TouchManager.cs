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
    
    [Header("Tap to Shoot Settings")]
    [SerializeField] float shootTapRadiusPixels = 90f;

    [Header("Debounce Settings")]
    private float _lastBeganTime = -999f;
    private Vector2 _lastBeganPos;
    private const float BeganDebounceSeconds = 0.05f;     // 50ms
    private const float BeganDebouncePixels = 8f;         // small radius

    private Dictionary<int, TouchData> activeTouches = new Dictionary<int, TouchData>();
    // Fingers that are allowed to control (or be promoted to) Look/aim.
    // Interactive UI controls (Buttons, etc.) should NEVER be aim-eligible.
    private HashSet<int> aimEligibleFingerIds = new HashSet<int>();
    Rect screenBounds = new Rect(0,0,0,0);
    bool virtualAimJoystickVisible;
    Vector2 lastCursorScreenPos;

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

            if (!touchControl.press.isPressed)
            {
                if (activeTouches.ContainsKey(fingerId))
                    EndTouch(fingerId);
                continue;
            }

            // Only pressed touches make it into this set
            currentFrameFingerIds.Add(fingerId);

            Vector2 touchPos = touchControl.position.ReadValue();
            var phase = touchControl.phase.ReadValue();

            if (!activeTouches.ContainsKey(fingerId) && phase != UnityEngine.InputSystem.TouchPhase.Began)
            {
                BeginTouch(fingerId, touchPos, false);
                Debug.LogError("TouchManager: Missed touch in activeTouches. Forcing tracking.");
            }

            switch (phase)
            {
                case UnityEngine.InputSystem.TouchPhase.Began:
                    BeginTouch(fingerId, touchPos, false);
                    break;
                case UnityEngine.InputSystem.TouchPhase.Moved:
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
        lastCursorScreenPos = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }

    #region Tap/Move/Release

    private void BeginTouch(int fingerId, Vector2 position, bool wasExisting)
    {
        // If this finger is already tracked, don't re-begin it (prevents double begin reclassification)
        if (activeTouches.ContainsKey(fingerId))
            return;

        // Snapshot the cursor position BEFORE any logic might move it this begin.
        // This prevents Android double-began from making distToCursor suddenly become 0.
        Vector2 cursorPosAtBegin = lastCursorScreenPos;

        // Raycast to see if it's over INTERACTIVE UI
        // IMPORTANT: Interactive UI (Buttons, etc.) should NEVER be aim-eligible.
        if (IsPointerOverInteractiveUI(position, out GameObject hitInteractiveUIObject))
        {
            aimEligibleFingerIds.Remove(fingerId);

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
            else if (IsInRegion(position, moveJoystickDeadzoneRect) ||
                    IsInRegion(position, aimJoystickDeadzoneRect) ||
                    IsInRegion(position, rightSideButtonsDeadzoneRect))
            {
                activeTouches[fingerId] = new TouchData(fingerId, TouchPurpose.UI, position);
            }
            else
            {
                bool isLooking = false;
                foreach (var kvp in activeTouches)
                {
                    if (kvp.Value.purpose == TouchPurpose.Look)
                    {
                        isLooking = true;
                        break;
                    }
                }

                if (isLooking)
                {
                    aimEligibleFingerIds.Add(fingerId);
                    AddCombatTouch(wasExisting, fingerId, position, cursorPosAtBegin);
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
            AddCombatTouch(wasExisting, fingerId, position, cursorPosAtBegin);
        }
    }


    void AddCombatTouch(bool wasExisting, int fingerId, Vector2 position, Vector2 cursorPosAtBegin)

    {
        if (player == null)
            return;

        if (!wasExisting)
            activeTouches[fingerId] = new TouchData(fingerId, TouchPurpose.UI, position);

        int existingCombatCount = CountCombatTouchesExcluding(fingerId);

        // 2nd+ finger: melee (do NOT cancel fire)
        if (existingCombatCount >= 1)
        {
            activeTouches[fingerId].purpose = TouchPurpose.Melee;
            player.animControllerPlayer.MeleeInput_Performed(new InputAction.CallbackContext());
            return;
        }

        Debug.Log($"tap={position} lastCursor={lastCursorScreenPos} realCursor={CursorManager.Instance.GetCustomCursorScreenPos()} "
        + $"distLast={Vector2.Distance(position,lastCursorScreenPos)} distReal={Vector2.Distance(position,CursorManager.Instance.GetCustomCursorScreenPos())}");

        // 1st finger: tap near cursor => start shooting immediately (do NOT move cursor)
        float distToCursor = Vector2.Distance(position, cursorPosAtBegin);

        if (distToCursor <= shootTapRadiusPixels)
        {
            activeTouches[fingerId].purpose = TouchPurpose.Shoot;
            player.animControllerPlayer.FireInput_Performed(new InputAction.CallbackContext());
            Debug.Log("TouchManager: Tap to Shoot activated.");
            return;
        }

        // Otherwise, aim/Look
        activeTouches[fingerId].purpose = TouchPurpose.Look;
        lastCursorScreenPos = position;
        CursorManager.Instance.MoveCustomCursorTo(position, CursorRangeType.Bounds, Vector2.zero, 0, screenBounds);
        player.lastAimDir = Camera.main.ScreenToWorldPoint(position) - player.transform.position;
    }


    int CountCombatTouchesExcluding(int excludeFingerId)
    {
        int count = 0;
        foreach (var kvp in activeTouches)
        {
            if (kvp.Key == excludeFingerId) continue;

            switch (kvp.Value.purpose)
            {
                case TouchPurpose.Look:
                case TouchPurpose.Shoot:
                case TouchPurpose.Melee:
                    count++;
                    break;
            }
        }
        return count;
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
                lastCursorScreenPos = position;
                CursorManager.Instance.MoveCustomCursorTo(position, CursorRangeType.Bounds, Vector2.zero, 0, screenBounds);
                if (player != null)
                    player.lastAimDir = Camera.main.ScreenToWorldPoint(position) - player.transform.position;
                break;
            case TouchPurpose.Shoot:
                // While holding shoot, dragging moves aim AND fire continues (auto weapons)
                lastCursorScreenPos = position;
                CursorManager.Instance.MoveCustomCursorTo(position, CursorRangeType.Bounds, Vector2.zero, 0, screenBounds);
                if (player != null)
                    player.lastAimDir = Camera.main.ScreenToWorldPoint(position) - player.transform.position;
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
                    CancelMeleeIfNoneRemaining();
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

    void CancelMeleeIfNoneRemaining()
    {
        foreach (var kvp in activeTouches)
        {
            if (kvp.Value.purpose == TouchPurpose.Melee)
                return;
        }

        player.animControllerPlayer.MeleeInput_Cancelled(new InputAction.CallbackContext());
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

    