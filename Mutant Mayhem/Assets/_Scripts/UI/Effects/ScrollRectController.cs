using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ScrollRectController : MonoBehaviour
{
    [SerializeField] float lerpTime;
    [SerializeField] RectTransform rectTrans;
    [SerializeField] List<RectTransform> rectTransforms;
    //[SerializeField] float scrollSpeed = 0.01f;

    Vector2 targetPosition;

    void Update()
    {
        foreach (RectTransform rectTrans in rectTransforms)
        {
            Vector2 yPos = new Vector2(rectTrans.anchoredPosition.x, targetPosition.y);
            rectTrans.anchoredPosition = Vector2.Lerp(rectTrans.anchoredPosition, yPos, lerpTime);           
        }
    }

    public void SnapTo(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();

        // Save targetPos since subsequent iterations below require it.
        Vector3 targetPos = target.position;

        float screenHeight = Screen.height;

        foreach (RectTransform rectTrans in rectTransforms)
        {
            // Calculate the new position.
            Vector2 newPos = 
                (Vector2)this.rectTrans.transform.InverseTransformPoint(rectTrans.position)
                - (Vector2)this.rectTrans.transform.InverseTransformPoint(targetPos - new Vector3(
                    0, rectTrans.rect.height / screenHeight , 0));

            targetPosition = new Vector2(rectTrans.anchoredPosition.x, newPos.y);
        }
    }
    /*
    public void OnScrollSecondaryInput(Vector2 input)
    {
        // Apply the scroll speed multiplier to scale down the input
        float scaledInput = input.y * scrollSpeed;
        targetPosition.y += scaledInput;

        // Ensure the targetPosition.y stays within the bounds of your content
        float minY = 0; // Define your min Y position
        float maxY = rectTrans.rect.height; // Define your max Y position
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
    }
    */
}
