using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeviceRectAdjuster : MonoBehaviour
{
    public RectTransform rectToScale;
    [SerializeField] bool touchScreenOnly = true;
    
    [Header("Scaling Factors")]
    [SerializeField] float wideScale = 1.2f;
    [SerializeField] float midWideScale = 1.1f;
    [SerializeField] float default16x9Scale = 1.0f;
    [SerializeField] float midNarrowScale = 0.87f;
    [SerializeField] float narrowScale = 0.75f;

    [Header("Custom Scaling Pivot")]
    [SerializeField] bool useCustomPivot = false;
    [SerializeField] Vector2 cutomScalePivot = new Vector2(0.5f, 0.5f);

    [Header("Positions Adjustments")]
    [SerializeField] Vector2 widePositionOffest = Vector2.zero;
    [SerializeField] Vector2 defaultWidePositionOffest = Vector2.zero;
    [SerializeField] Vector2 narrowPositionOffset = Vector2.zero;

    [Header("iOS Position Adjustment Addition")]
    [SerializeField] bool iOSPosOffsetAdd = false;
    [SerializeField] Vector2 iOSOffsetAddition = Vector2.zero;

    private void OnValidate()
    {
        cutomScalePivot.x = Mathf.Clamp(cutomScalePivot.x, 0f, 1f);
        cutomScalePivot.y = Mathf.Clamp(cutomScalePivot.y, 0f, 1f);
    }

    float startScaleFactor;
    Vector2 startPivot;
    Vector2 startAnchoredPos;
    int lastWidth, lastHeight;
    float scaleFactor;
    float aspectRatio;

    void Awake()
    {
        startScaleFactor = rectToScale.localScale.x;
        if (rectToScale.localScale.y != startScaleFactor)
            Debug.LogWarning("DeviceRectScaler: Detected object with unsymetrical scaling.  X scale does not equal Y scale!");
    }

    void OnDestroy()
    {
        ScreenScaleChecker.OnAspectRatioChanged -= AspectRatioChanged;
        InputManager.Instance.LastUsedDeviceChanged -= DeviceChanged;
    }

    void Start()
    {
        startAnchoredPos = rectToScale.anchoredPosition;
        ScreenScaleChecker.OnAspectRatioChanged += AspectRatioChanged;
        InputManager.Instance.LastUsedDeviceChanged += DeviceChanged;

        StartCoroutine(DelayStart());
    }

    IEnumerator DelayStart()
    {
        yield return new WaitForSeconds(0.5f);
        AspectRatioChanged(ScreenScaleChecker.CurrentAspectRatio);
    }

    void AdjustScale()
    {
        scaleFactor = startScaleFactor;
        ScaleRect(scaleFactor);

        if (touchScreenOnly && InputManager.LastUsedDevice != Touchscreen.current) return;

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

        ScaleRect(scaleFactor);
    }

    void ScaleRect(float scaleFactor)
    {
        if (useCustomPivot)
        {
            startPivot = rectToScale.pivot;
            rectToScale.pivot = cutomScalePivot;
        }

        // Apply scaling
        rectToScale.localScale = Vector3.one * scaleFactor;

        if (useCustomPivot)
            rectToScale.pivot = startPivot;
    }

    void AdjustPosition()
    {
        rectToScale.anchoredPosition = startAnchoredPos;

        if (touchScreenOnly && InputManager.LastUsedDevice != Touchscreen.current) return;

        if (aspectRatio >= 2.0f)
        {
            rectToScale.anchoredPosition = startAnchoredPos + widePositionOffest;
        }
        else if (aspectRatio > 1.85f)
        {
            rectToScale.anchoredPosition = startAnchoredPos + widePositionOffest;
        }
        else if (aspectRatio >= 1.77f)
        {
            rectToScale.anchoredPosition = startAnchoredPos + defaultWidePositionOffest;
        }
        else if (aspectRatio > 1.5f) 
        {
            rectToScale.anchoredPosition = startAnchoredPos + narrowPositionOffset;
        }
        else
        {
            rectToScale.anchoredPosition = startAnchoredPos + narrowPositionOffset;
        }

        if (iOSPosOffsetAdd && Application.platform == RuntimePlatform.IPhonePlayer)
        {
            rectToScale.anchoredPosition += iOSOffsetAddition;
        }
    }

    void AspectRatioChanged(float aspectRatio)
    {
        this.aspectRatio = aspectRatio;
        AdjustScale();
        AdjustPosition();
    }

    void DeviceChanged(InputDevice device)
    {
        AdjustScale();
        AdjustPosition();
    }
}
