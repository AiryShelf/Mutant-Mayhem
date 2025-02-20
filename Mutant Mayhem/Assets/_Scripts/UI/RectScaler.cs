using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class JoystickScaler : MonoBehaviour
{
    public RectTransform rectToScale;
    float default16x9Scale = 1.0f;
    int lastWidth, lastHeight;

    void OnEnable()
    {
        AdjustJoystickScale();
        StartCoroutine(CheckScreenScale());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    void AdjustJoystickScale()
    {
        float aspectRatio = (float)Screen.width / Screen.height;

        float scaleFactor = default16x9Scale;

        if (aspectRatio < 1.6f) // Narrower than 16:10 (e.g., 4:3)
        {
            scaleFactor = 0.7f; // Reduce scale
        }
        else if (aspectRatio < 1.77f) // Between 16:10 and 16:9
        {
            scaleFactor = 0.85f;
        }
        else
        {
            scaleFactor = default16x9Scale; // Keep default scale
        }

        // Apply scaling
        rectToScale.localScale = Vector3.one * scaleFactor;
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
                AdjustJoystickScale();
            }
        }
    }
}
