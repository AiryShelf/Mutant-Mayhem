using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEngine.UIElements;

[ExecuteInEditMode()]
public class Tooltip : MonoBehaviour
{
    public TextMeshProUGUI headerField;
    public TextMeshProUGUI contentField;
    public LayoutElement layoutElement;
    public int characterWrapLimit;
    public RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    public void SetText(string content, string header = "")
    {
        if (string.IsNullOrEmpty(header))
        {
            headerField.gameObject.SetActive(false);
        }
        else
        {
            headerField.gameObject.SetActive(true);
            headerField.text = header;
        }

        contentField.text = content;

        int headerLength = headerField.text.Length;
        int contentLength = contentField.text.Length;

        layoutElement.enabled = (headerLength > characterWrapLimit || contentLength > characterWrapLimit) ? true : false;
    }

    void Update()
    {
        // Ensure we have our rect transform reference
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        // Allows updating tooltip size while running editor
        if (Application.isEditor)
        {
            if (layoutElement != null && headerField != null && contentField != null)
            {
                int headerLength = headerField.text.Length;
                int contentLength = contentField.text.Length;
                layoutElement.enabled = (headerLength > characterWrapLimit || contentLength > characterWrapLimit);
            }
        }

        // Determine pointer position safely across platforms (mouse on desktop, touch on mobile)
        Vector2 pos;

        // Prefer the unified Pointer device when available
        if (Pointer.current != null)
        {
            pos = Pointer.current.position.ReadValue();
        }
        // Fallback to touchscreen (e.g., iPhone) only when there is an active touch
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            pos = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else
        {
            // No pointer device or active touch; nothing to position
            return;
        }

        var normalizedPosition = new Vector2(pos.x / Screen.width, pos.y / Screen.height);
        var pivot = CalculatePivot(normalizedPosition);

        if (rectTransform != null)
            rectTransform.pivot = pivot;

        transform.position = pos;
    }

    private Vector2 CalculatePivot(Vector2 normalizedPosition)
    {
        // Set position for each quadrant
        var pivotTopLeft = new Vector2(-0.1f, 1.1f);
        var pivotTopRight = new Vector2(1.1f, 1.1f);
        var pivotBottomLeft = new Vector2(-0.1f, -0.1f);
        var pivotBottomRight = new Vector2(1.1f, -0.1f);

        // Check quadrants
        if (normalizedPosition.x < 0.5f && normalizedPosition.y >= 0.5f)
        {
            return pivotTopLeft;
        }
        else if (normalizedPosition.x > 0.5f && normalizedPosition.y >= 0.5f)
        {
            return pivotTopRight;
        }
        else if (normalizedPosition.x <= 0.5f && normalizedPosition.y < 0.5f)
        {
            return pivotBottomLeft;
        }
        else
        {
            return pivotBottomRight;
        }
    }
}
