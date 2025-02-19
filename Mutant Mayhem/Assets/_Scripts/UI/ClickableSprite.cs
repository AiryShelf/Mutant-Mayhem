using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class ClickableSprite : MonoBehaviour
{
    [SerializeField] private Collider2D targetCollider;  // Assign a specific Collider2D
    [SerializeField] private UnityEvent onClick;  // UnityEvent to assign functions in the Inspector

    private void Update()
    {
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            Vector2 screenPosition = Pointer.current.position.ReadValue();
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

            // First, check if the pointer is inside the collider bounds before raycasting
            if (targetCollider != null && targetCollider.bounds.Contains(worldPosition))
            {
                CheckRaycastHits(worldPosition);
            }
        }
    }

    private void CheckRaycastHits(Vector2 worldPosition)
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
