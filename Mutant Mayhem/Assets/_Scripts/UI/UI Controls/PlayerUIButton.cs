using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum PlayerUIButtonType
{
    Shoot,
    Melee
}

public class PlayerUIButton : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public PlayerUIButtonType buttonType;

    [Header("Assigned at Runtime for Shoot and Melee")]
    public UnityEvent onPressed;
    public UnityEvent onReleased;

    bool isPressed;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isPressed) return;
        isPressed = true;
        onPressed?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Release();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // If they slide off the button while holding, stop the action.
        Release();
    }

    void Release()
    {
        if (!isPressed) return;
        isPressed = false;
        onReleased?.Invoke();
    }
}