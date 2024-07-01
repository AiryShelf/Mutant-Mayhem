using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OffScreenIndicator : MonoBehaviour
{
    public GameObject target;
    public Image arrow;
    public Camera mainCamera;

    private void FixedUpdate()
    {
        Vector3 targetPosition = mainCamera.WorldToViewportPoint(target.transform.position);
        //Debug.Log("offscreen targetPosition: " + targetPosition);
        if (IsTargetOffScreen(targetPosition))
        {
            arrow.enabled = true;
            Vector2 screenPosition = CalculateScreenPosition(targetPosition);
            arrow.rectTransform.position = screenPosition;
            arrow.rectTransform.rotation = CalculateArrowRotation(screenPosition, target.transform.position);
        }
        else
        {
            arrow.enabled = false;
        }
    }

    private bool IsTargetOffScreen(Vector3 targetPosition)
    {
        return targetPosition.x < 0 || targetPosition.x > 1 || targetPosition.y < 0 || targetPosition.y > 1;
    }

    private Vector2 CalculateScreenPosition(Vector3 targetPosition)
    {
        targetPosition.x = Mathf.Clamp(targetPosition.x, 0.05f, 0.95f);
        targetPosition.y = Mathf.Clamp(targetPosition.y, 0.05f, 0.95f);
        return mainCamera.ViewportToScreenPoint(targetPosition);
    }

    private Quaternion CalculateArrowRotation(Vector2 screenPosition, Vector3 targetWorldPosition)
    { 
        Vector3 direction = Camera.main.ScreenToWorldPoint(arrow.rectTransform.position) - targetWorldPosition;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(new Vector3(0, 0, angle + 90));
    }
}
