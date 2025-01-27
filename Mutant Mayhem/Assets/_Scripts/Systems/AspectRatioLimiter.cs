using UnityEngine;

public class AspectRatioLimiter : MonoBehaviour
{
    [SerializeField] float targetAspect = 16.0f / 9.0f;

    void Start()
    {
        UpdateCameraViewport();
    }

    void Update()
    {
        UpdateCameraViewport();
    }

    private void UpdateCameraViewport()
    {
        float currentAspect = (float)Screen.width / Screen.height;

        if (currentAspect < targetAspect)
        {
            float scaleHeight = currentAspect / targetAspect;
            Camera.main.rect = new Rect(0, (1.0f - scaleHeight) / 2.0f, 1.0f, scaleHeight);
        }
        else
        {
            Camera.main.rect = new Rect(0, 0, 1, 1);
        }
    }
}
