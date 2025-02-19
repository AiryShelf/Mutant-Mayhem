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
