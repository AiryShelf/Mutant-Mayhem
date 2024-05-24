using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollRectController : MonoBehaviour
{
    [SerializeField] float lerpTime;
    [SerializeField] RectTransform rectTrans;
    [SerializeField] List<RectTransform> rectTransforms;

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
}
