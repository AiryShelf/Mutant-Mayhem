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
    public float handleRange = 100f;
    public bool isFixed = true;
    public Vector2 JoystickOutput { get; private set; }

    // *** CHANGED *** Reference to your player input/animation script
    [Header("Player References")]
    public AnimationControllerPlayer animControllerPlayer; 
    // (Rename to match your actual script that has FireInput_Performed, etc.)

    [Header("Attack Settings")]
    [Tooltip("Check this if you want to detect taps (single or double) within the joystick area.")]
    public bool enableTapAttack = true;
    [Tooltip("How fast two taps must occur to register as a double tap.")]
    public float doubleTapThreshold = 0.3f;
    [Tooltip("Set this to decide if the 'left side' is for shooting and 'right side' for melee, etc.")]
    public bool divideHorizontally = true;
    [Tooltip("Radius of the dead zone in the center of the joystick where no attack is triggered.")]
    public float deadZoneRadius = 20f;

    // *** CHANGED *** For double-tap / hold logic
    private float lastTapTime = 0f;
    private bool isDoubleTap = false;
    private bool isHoldingAttack = false;
    
    private enum AttackType { None, Shoot, Melee }
    private AttackType currentAttackType = AttackType.None;

    // Joystick internal variables
    private Vector2 _startPos;
    private bool _isDragging = false;
    private int _pointerId = -1;

    private void Awake()
    {
        if (background == null)
            background = GetComponent<RectTransform>();

        _startPos = background.anchoredPosition;
        Debug.Log("Virtual Joystick Start Pos: " + _startPos);
    }

    public void ActivateJoystick(bool active)
    {
        background.gameObject.SetActive(active);
        handle.gameObject.SetActive(active);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Virtual Joystick: OnPointerDown");
        if (!isFixed)
        {
            background.anchoredPosition = ScreenPointToAnchorPosition(eventData.position);
        }

        _isDragging = true;
        _pointerId = eventData.pointerId;

        // *** CHANGED *** Tap detection & Attack
        if (enableTapAttack)
        {
            DetectTapAndAttack(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging || eventData.pointerId != _pointerId)
            return;

        Debug.Log("Virtual Joystick: OnDrag");
        Vector2 pos = ScreenPointToAnchorPosition(eventData.position);
        Vector2 offset = pos - (Vector2)background.anchoredPosition;
        float magnitude = Mathf.Clamp(offset.magnitude, 0f, handleRange);
        Vector2 direction = offset.normalized * magnitude;

        handle.anchoredPosition = direction;
        JoystickOutput = direction / handleRange;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Virtual Joystick: OnPointerUp");
        if (eventData.pointerId == _pointerId)
        {
            _isDragging = false;
            _pointerId = -1;

            handle.anchoredPosition = Vector2.zero;
            JoystickOutput = Vector2.zero;

            if (!isFixed)
            {
                background.anchoredPosition = _startPos;
            }

            // *** CHANGED *** If we're "holding" an attack, cancel it now
            if (isHoldingAttack)
            {
                CancelAttack(currentAttackType);
            }

            isHoldingAttack = false;
            currentAttackType = AttackType.None;
        }
    }

    private Vector2 ScreenPointToAnchorPosition(Vector2 screenPos)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)background.parent, 
            screenPos,
            null,
            out localPoint
        );
        Debug.Log($"Virtual Joystick: LocalPoint: {localPoint}, from screenPos: {screenPos}");
        return localPoint;
    }

    // *** CHANGED *** 
    // Detect whether this pointer down is a single or double tap, 
    // and call the appropriate method.
    private void DetectTapAndAttack(PointerEventData eventData)
    {
        Debug.Log("Virtual Joystick: DetectTapToAttack");
        float timeSinceLastTap = Time.time - lastTapTime;
        lastTapTime = Time.time;

        // Determine which attack type (shoot or melee)
        currentAttackType = DetermineAttackType(eventData);

        if (timeSinceLastTap <= doubleTapThreshold)
        {
            // double tap => hold for continuous
            isDoubleTap = true;
            isHoldingAttack = true;

            // *** CHANGED *** Start continuous attack
            PerformAttack(currentAttackType, continuous: true);
        }
        else
        {
            // single tap => quick single attack
            isDoubleTap = false;
            PerformAttack(currentAttackType, continuous: false);

            // For single-tap, we do an immediate "perform" and then "cancel"
            // so it only fires/melees once.
            StartCoroutine(DelayCancelAttack(currentAttackType));
        }
    }

    // *** CHANGED *** Check which side we're on.
    private AttackType DetermineAttackType(PointerEventData eventData)
    {
        Vector2 localPos = ScreenPointToAnchorPosition(eventData.position);
        Vector2 offsetFromCenter = localPos - (Vector2)background.anchoredPosition;

        if (offsetFromCenter.magnitude < deadZoneRadius)
        {
            return AttackType.None;
        }

        if (divideHorizontally)
        {
            // Left half => shoot, right half => melee
            return offsetFromCenter.x < 0 ? AttackType.Shoot : AttackType.Melee;
        }
        else
        {
            // Bottom half => shoot, top half => melee
            return offsetFromCenter.y < 0 ? AttackType.Shoot : AttackType.Melee;
        }
    }

    // *** CHANGED *** Perform the "Performed" input
    private void PerformAttack(AttackType attackType, bool continuous)
    {
        if (animControllerPlayer == null) return;

        Debug.Log($"Virtual Joystick: Perform attack type: {attackType}, continuous: {continuous}");
        // We’ll supply an empty InputAction.CallbackContext since we 
        // only need the animations to trigger.
        var emptyContext = new InputAction.CallbackContext();

        switch (attackType)
        {
            case AttackType.Shoot:
                // Simulate Fire Input Performed
                animControllerPlayer.FireInput_Performed(emptyContext);
                if (continuous)
                {
                    // We hold the input until pointer up
                    Debug.Log("[Joystick] Continuous Fire START");
                }
                else
                {
                    // Single tap shot
                    Debug.Log("[Joystick] Single Fire Triggered");
                }
                break;

            case AttackType.Melee:
                // Simulate Melee Input Performed
                animControllerPlayer.MeleeInput_Performed(emptyContext);
                if (continuous)
                {
                    Debug.Log("[Joystick] Continuous Melee START");
                }
                else
                {
                    Debug.Log("[Joystick] Single Melee Triggered");
                }
                break;

            default:
                break;
        }
    }

    IEnumerator DelayCancelAttack(AttackType attackType)
    {
        yield return new WaitForFixedUpdate();
        CancelAttack(attackType);
    }

    // *** CHANGED *** Perform the "Cancelled" input
    private void CancelAttack(AttackType attackType)
    {
        Debug.Log("Virtual Joystick: CancelAttack");
        if (animControllerPlayer == null) return;

        var emptyContext = new InputAction.CallbackContext();

        switch (attackType)
        {
            case AttackType.Shoot:
                animControllerPlayer.FireInput_Cancelled(emptyContext);
                Debug.Log("[Joystick] Fire CANCELLED");
                break;

            case AttackType.Melee:
                animControllerPlayer.MeleeInput_Cancelled(emptyContext);
                Debug.Log("[Joystick] Melee CANCELLED");
                break;

            default:
                break;
        }
    }
}


/*

using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("References")]
    // *** CHANGED *** Public references to the background & handle
    public RectTransform background; 
    public RectTransform handle;

    [Header("Settings")]
    // *** CHANGED *** Radius of how far the handle can move from center
    public float handleRange = 100f;

    // If you'd like a "floating" joystick that appears where the user touches,
    // set this to false. If you want a fixed joystick, set it to true.
    public bool isFixed = true; 

    // This public property is what you'll read from other scripts
    // to get the (x,y) input.
    public Vector2 JoystickOutput { get; private set; }

    // Internal variables
    private Vector2 _startPos;
    private bool _isDragging = false;
    private int _pointerId = -1; // Track pointer ID if needed for multi-touch

    private void Awake()
    {
        if (background == null)
            background = GetComponent<RectTransform>();
        
        _startPos = background.anchoredPosition;
        Debug.Log("Virtual Joystick Start Pos: " + _startPos);
    }

    public void ActivateJoystick(bool active)
    {
        background.gameObject.SetActive(active);
        handle.gameObject.SetActive(active);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isFixed)
        {
            background.anchoredPosition = ScreenPointToAnchorPosition(eventData.position);
        }

        _isDragging = true;
        _pointerId = eventData.pointerId;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Ensure we're only dragging if it's the same pointer/finger
        if (!_isDragging || eventData.pointerId != _pointerId)
            return;

        Vector2 pos = ScreenPointToAnchorPosition(eventData.position);

        Vector2 offset = pos - (Vector2)background.anchoredPosition;
        float magnitude = Mathf.Clamp(offset.magnitude, 0f, handleRange);
        Vector2 direction = offset.normalized * magnitude;

        handle.anchoredPosition = direction;

        JoystickOutput = direction / handleRange;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId == _pointerId)
        {
            _isDragging = false;
            _pointerId = -1;

            handle.anchoredPosition = Vector2.zero;
            JoystickOutput = Vector2.zero;

            if (!isFixed)
            {
                background.anchoredPosition = _startPos; 
            }
        }
    }

    private Vector2 ScreenPointToAnchorPosition(Vector2 screenPos)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)background.parent, 
            screenPos,
            null, // or use Canvas' worldCamera if in Camera render mode
            out localPoint
        );
        return localPoint;
    }
}

*/
