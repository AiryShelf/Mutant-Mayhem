using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ClickableSprite : MonoBehaviour
{
    [SerializeField] Collider2D targetCollider;
    [SerializeField] UnityEvent onClick;
    [SerializeField] bool touchscreenOnly = false;

    void Update()
    {
        if (touchscreenOnly && Touchscreen.current == null)
            return;

        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            // Check for UI Elements
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(Pointer.current.deviceId))
                return;

            Vector2 screenPosition = Pointer.current.position.ReadValue();
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

            // First, check if the pointer is inside the collider bounds before raycasting
            if (targetCollider != null && targetCollider.bounds.Contains(worldPosition))
            {
                CheckRaycastHits(worldPosition);
            }
        }
    }

    void CheckRaycastHits(Vector2 worldPosition)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(worldPosition, Vector2.zero);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == targetCollider)  // Ensure the collider matches the assigned one
            {
                Debug.Log(targetCollider.gameObject.name + " clicked!");
                onClick?.Invoke();  // Trigger assigned event
                return;  // Stop checking after the first valid hit
            }
        }
    }
}
