using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class VirtualJoystick : MonoBehaviour, 
    IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("References")]
    public RectTransform background;
    public RectTransform handle;

    [Header("Settings")]
    public float handleRange = 75;
    public bool isFixed = true;
    public Vector2 JoystickOutput { get; private set; }

    [Header("Player References")]
    public Player player;
    public AnimationControllerPlayer animControllerPlayer; 

    [Header("Button Settings")]
    [Tooltip("Check this if you want to detect taps (single or double) within the joystick area for actions.  (This splits the joystick in 2 zones, vertically for Shoot and Melee)")]
    public bool enableTapAttack = false;
    [Tooltip("Check this if you want to detect taps (single or double) within the joystick area for sprinting.")]
    public bool enableTapSprint = false;
    [Tooltip("If true, double tap starts action. If false, a single tap starts action.")]
    public bool useDoubleTapHold = false;
    [Tooltip("How fast two taps must occur to register as a double tap.")]
    public float doubleTapThreshold = 0.3f;
    [Tooltip("Radius of the dead zone in the center of the joystick where no action is triggered.")]
    public float deadZoneRadius = 0f;

    private float lastTapTime = 0f;
    private bool isHoldingAction = false;
    
    private enum ActionType { None, Shoot, Melee, Sprint }
    private ActionType currentActionType = ActionType.None;

    Vector2 startPos;
    private Vector2 _startPos;
    private bool _isDragging = false;
    private int _pointerId = -1;
    InputAction.CallbackContext emptyContext = new InputAction.CallbackContext();
    Coroutine delayCancelCoroutine;
    Vector2 pointerDownPos;

    private void Awake()
    {
        if (background == null)
            background = GetComponent<RectTransform>();

        startPos = background.anchoredPosition;
        _startPos = startPos;
        //Debug.Log("Virtual Joystick Start Pos: " + _startPos);
    }

    public void ActivateJoystick(bool active)
    {
        background.gameObject.SetActive(active);
        handle.gameObject.SetActive(active);
    }

    #region Tap/Drag/Release

    public void OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log("Virtual Joystick: OnPointerDown");
        if (!isFixed)
        {
            _startPos = background.anchoredPosition;
            background.anchoredPosition = ScreenPointToAnchorPosition(eventData.position);
            handle.anchoredPosition = Vector2.zero;
        }

        pointerDownPos = ScreenPointToAnchorPosition(eventData.position);
        _isDragging = true;
        _pointerId = eventData.pointerId;

        DetectTapAndDoAction(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging || eventData.pointerId != _pointerId)
            return;

        //Debug.Log("Virtual Joystick: OnDrag");
        Vector2 pos = ScreenPointToAnchorPosition(eventData.position);
        Vector2 offset = pos - pointerDownPos;
        float magnitude = Mathf.Clamp(offset.magnitude, 0f, handleRange);
        Vector2 direction = offset.normalized * magnitude;

        handle.anchoredPosition = direction;
        JoystickOutput = direction / handleRange;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //Debug.Log("Virtual Joystick: OnPointerUp");
        if (eventData.pointerId == _pointerId)
        {
            ResetJoystick();
        }
    }

    public void ResetJoystick()
    {
        _isDragging = false;
        _pointerId = -1;

        handle.anchoredPosition = Vector2.zero;
        JoystickOutput = Vector2.zero;

        if (!isFixed)
        {
            background.anchoredPosition = startPos;
            handle.anchoredPosition = startPos;
        }

        if (isHoldingAction)
        {
            CancelAction(currentActionType);
        }

        isHoldingAction = false;
        currentActionType = ActionType.None;
    }

    #endregion

    #region Detection

    private Vector2 ScreenPointToAnchorPosition(Vector2 screenPos)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)background.parent, 
            screenPos,
            null,
            out localPoint
        );
        //Debug.Log($"Virtual Joystick: LocalPoint: {localPoint}, from screenPos: {screenPos}");
        return localPoint;
    }

    private void DetectTapAndDoAction(PointerEventData eventData)
    {
        //Debug.Log("Virtual Joystick: DetectTapToAction");
        float timeSinceLastTap = Time.time - lastTapTime;
        lastTapTime = Time.time;

        // Determine which action type (shoot or melee)
        currentActionType = DetermineActionType(eventData);

        if (useDoubleTapHold)
        {
            // Double tap starts a continuous action
            if (timeSinceLastTap <= doubleTapThreshold)
            {
                isHoldingAction = true;
                if (delayCancelCoroutine != null)
                    StopCoroutine(delayCancelCoroutine);
                PerformAction(currentActionType, continuous: true);
            }
        }
        else
        {
            isHoldingAction = true;
            PerformAction(currentActionType, continuous: true);
        }
    }

    private ActionType DetermineActionType(PointerEventData eventData)
    {
        Vector2 localPos = ScreenPointToAnchorPosition(eventData.position);
        Vector2 offsetFromCenter = localPos - _startPos;

        if (offsetFromCenter.magnitude < deadZoneRadius)
            return ActionType.None;

        if (enableTapAttack)
        {
            // Left half => shoot, right half => melee
            return offsetFromCenter.x < 0 ? ActionType.Shoot : ActionType.Melee;
        }
        else if (enableTapSprint)
            return ActionType.Sprint;
        
        return ActionType.None;
    }

    #endregion

    #region Perform Action

    private void PerformAction(ActionType actionType, bool continuous)
    {
        if (animControllerPlayer == null || player == null) return;

        //Debug.Log($"Virtual Joystick: Perform action type: {actionType}, continuous: {continuous}");

        switch (actionType)
        {
            case ActionType.Sprint:
                // Simulate Sprint Input Performed
                player.SprintInput_Performed(emptyContext);
                if (continuous)
                {
                    // We hold the input until pointer up
                    //Debug.Log("[Joystick] Continuous Sprint START");
                }
                else
                {
                    // Single tap 
                    //Debug.Log("[Joystick] Single Sprint Triggered");
                }
                break;
            case ActionType.Shoot:
                // Simulate Fire Input Performed
                animControllerPlayer.FireInput_Performed(emptyContext);
                if (continuous)
                {
                    // We hold the input until pointer up
                    //Debug.Log("[Joystick] Continuous Fire START");
                }
                else
                {
                    // Single tap shot
                    //Debug.Log("[Joystick] Single Fire Triggered");
                }
                break;

            case ActionType.Melee:
                // Simulate Melee Input Performed
                animControllerPlayer.MeleeInput_Performed(emptyContext);
                if (continuous)
                {
                    //Debug.Log("[Joystick] Continuous Melee START");
                }
                else
                {
                    //Debug.Log("[Joystick] Single Melee Triggered");
                }
                break;

            default:
                break;
        }
    }

    IEnumerator DelayCancelAction(ActionType actionType)
    {
        yield return new WaitForFixedUpdate();
        CancelAction(actionType);
    }

    #endregion

    #region Cancel Action

    private void CancelAction(ActionType actionType)
    {
        //Debug.Log("Virtual Joystick: CancelAction: " + actionType);
        if (animControllerPlayer == null || player == null) return;

        switch (actionType)
        {
            case ActionType.Sprint:
                player.SprintInput_Cancelled(emptyContext);
                break;
            case ActionType.Shoot:
                animControllerPlayer.FireInput_Cancelled(emptyContext);
                //Debug.Log("[Joystick] Fire CANCELLED");
                break;

            case ActionType.Melee:
                animControllerPlayer.MeleeInput_Cancelled(emptyContext);
                //Debug.Log("[Joystick] Melee CANCELLED");
                break;

            default:
                break;
        }
    }

    #endregion
}
