using UnityEngine;

public class SafeAreaSetter : MonoBehaviour
{
    [Header("Safe Area Settings")]
    [SerializeField] RectTransform[] panelsToResize;
    [SerializeField] Camera cameraToResize;

    [Header("Safe Zone Black Overlays")]
    [SerializeField] bool enableSafeZoneOverlays = true; 
    [SerializeField] RectTransform topOverlay;           
    [SerializeField] RectTransform bottomOverlay;        
    [SerializeField] RectTransform leftOverlay;          
    [SerializeField] RectTransform rightOverlay;         

    Rect lastSafeArea;

    void Awake()
    {
        ApplySafeArea();
    }

    void Start()
    {
        if (Screen.safeArea != lastSafeArea)
            ApplySafeArea();
    }

    void ApplySafeArea()
    {
        Rect safe = Screen.safeArea;

        // Always use full screen height
        safe.y = 0;
        safe.height = Screen.height;

        lastSafeArea = safe;

        // Convert from absolute pixels to normalized 0–1 viewport coords
        Rect normalized = new Rect
        (
            safe.x / Screen.width,
            safe.y / Screen.height,
            safe.width / Screen.width,
            safe.height / Screen.height
        );

        if (cameraToResize != null)
            cameraToResize.rect = normalized;

        foreach (RectTransform panel in panelsToResize)
        {
            if (panel != null)
            {
                panel.anchorMin = new Vector2(normalized.xMin, normalized.yMin);
                panel.anchorMax = new Vector2(normalized.xMax, normalized.yMax);
                panel.offsetMin = Vector2.zero;
                panel.offsetMax = Vector2.zero;
            }
        }

        if (enableSafeZoneOverlays)
            ApplySafeZoneOverlays(); // <-- NEW
    }

    void ApplySafeZoneOverlays()
    {
        Rect safe = Screen.safeArea;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        float topInset = screenHeight - (safe.y + safe.height);
        float bottomInset = safe.y;
        float leftInset = safe.x;
        float rightInset = screenWidth - (safe.x + safe.width);

        float topAnchor = (screenHeight - topInset) / screenHeight;
        float bottomAnchor = bottomInset / screenHeight;
        float leftAnchor = leftInset / screenWidth;
        float rightAnchor = (screenWidth - rightInset) / screenWidth;

        if (topOverlay)
        {
            topOverlay.gameObject.SetActive(topInset > 0);
            topOverlay.anchorMin = new Vector2(0, topAnchor);
            topOverlay.anchorMax = new Vector2(1, 1);
            topOverlay.offsetMin = Vector2.zero;
            topOverlay.offsetMax = Vector2.zero;
        }

        if (bottomOverlay)
        {
            bottomOverlay.gameObject.SetActive(bottomInset > 0);
            bottomOverlay.anchorMin = new Vector2(0, 0);
            bottomOverlay.anchorMax = new Vector2(1, bottomAnchor);
            bottomOverlay.offsetMin = Vector2.zero;
            bottomOverlay.offsetMax = Vector2.zero;
        }

        if (leftOverlay)
        {
            leftOverlay.gameObject.SetActive(leftInset > 0);
            leftOverlay.anchorMin = new Vector2(0, 0);
            leftOverlay.anchorMax = new Vector2(leftAnchor, 1);
            leftOverlay.offsetMin = Vector2.zero;
            leftOverlay.offsetMax = Vector2.zero;
        }

        if (rightOverlay)
        {
            rightOverlay.gameObject.SetActive(rightInset > 0);
            rightOverlay.anchorMin = new Vector2(rightAnchor, 0);
            rightOverlay.anchorMax = new Vector2(1, 1);
            rightOverlay.offsetMin = Vector2.zero;
            rightOverlay.offsetMax = Vector2.zero;
        }
    }
}
