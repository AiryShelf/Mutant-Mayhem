using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CanvasScalerCustom : MonoBehaviour
{
    // CHANGED: Instead of a single RectTransform, use an array to support multiple canvases.
    public RectTransform[] canvasesToScale;

    // CHANGED: Exposed scale factors as public fields for flexibility.
    public float default16x9Scale = 1.0f;
    public float midScale = 0.85f;     // For aspect ratios between 16:10 and 16:9
    public float narrowScale = 0.7f;   // For aspect ratios narrower than 16:10
    

    int lastWidth, lastHeight;

    void OnEnable()
    {
        AdjustCanvasScale();
        StartCoroutine(CheckScreenScale());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    void AdjustCanvasScale()
    {
        float aspectRatio = (float)Screen.width / Screen.height;
        float scaleFactor = default16x9Scale;

        if (aspectRatio < 1.6f) // Narrower than 16:10 (e.g., 4:3)
        {
            scaleFactor = narrowScale;
        }
        else if (aspectRatio < 1.77f) // Between 16:10 and 16:9
        {
            scaleFactor = midScale;
        }
        else
        {
            scaleFactor = default16x9Scale;
        }

        foreach (var canvas in canvasesToScale)
        {
            if (canvas != null)
            {
                if (scaleFactor == default16x9Scale)
                    canvas.GetComponent<CanvasScaler>().enabled = true;
                else
                    canvas.GetComponent<CanvasScaler>().enabled = false;

                canvas.localScale = Vector3.one * scaleFactor;
            }
        }
    }

    IEnumerator CheckScreenScale()
    {
        while (gameObject.activeInHierarchy)
        {
            yield return new WaitForSecondsRealtime(2);

            if (Screen.width != lastWidth || Screen.height != lastHeight)
            {
                lastWidth = Screen.width;
                lastHeight = Screen.height;
                AdjustCanvasScale();
            }
        }
    }
}
