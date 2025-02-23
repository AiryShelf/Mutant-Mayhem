using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RectScaler : MonoBehaviour
{
    public RectTransform rectToScale;
    [SerializeField] bool touchScreenOnly = true;
    
    [Header("Scaling Factors")]
    [SerializeField] float wideScale = 1.2f;
    [SerializeField] float midWideScale = 1.1f;
    [SerializeField] float default16x9Scale = 1.0f;
    [SerializeField] float midNarrowScale = 0.87f;
    [SerializeField] float narrowScale = 0.75f;
    int lastWidth, lastHeight;
    float scaleFactor;

    void OnEnable()
    {
        StartCoroutine(CheckScreenScale());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    void AdjustScale()
    {
        if (touchScreenOnly && InputManager.LastUsedDevice != Touchscreen.current) return;

        float aspectRatio = (float)Screen.width / Screen.height;
        scaleFactor = default16x9Scale;

        if (aspectRatio >= 2.0f)
        {
            scaleFactor = wideScale;
        }
        else if (aspectRatio > 1.85f)
        {
            scaleFactor = midWideScale;
        }
        else if (aspectRatio >= 1.77f)
        {
            scaleFactor = default16x9Scale;
        }
        else if (aspectRatio > 1.5f) 
        {
            scaleFactor = midNarrowScale;
        }
        else
        {
            scaleFactor = narrowScale;
        }

        // Apply scaling
        rectToScale.localScale = Vector3.one * scaleFactor;
    }

    IEnumerator CheckScreenScale()
    {
        yield return null;

        while (gameObject.activeInHierarchy)
        {
            if ((touchScreenOnly && InputManager.LastUsedDevice == Touchscreen.current) || !touchScreenOnly)
            {
                if (Screen.width != lastWidth || Screen.height != lastHeight)
                {
                    lastWidth = Screen.width;
                    lastHeight = Screen.height;
                    AdjustScale();
                }
            }
            
            yield return new WaitForSecondsRealtime(2);
        }
    }
}
