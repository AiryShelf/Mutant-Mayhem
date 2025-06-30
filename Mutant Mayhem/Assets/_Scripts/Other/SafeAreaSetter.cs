/*  SafeAreaCamera.cs
 *  Place this on the *main* camera (or every camera you want clipped). 
 */
using UnityEngine;

public class SafeAreaSetter : MonoBehaviour
{
    [SerializeField] RectTransform[] panelsToResize;
    [SerializeField] Camera cameraToResize;
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

        // Convert from absolute pixels to normalised 0–1 viewport coords
        Rect normalized = new Rect
        (
            safe.x        / Screen.width,
            safe.y        / Screen.height,
            safe.width    / Screen.width,
            safe.height   / Screen.height
        );

        if (cameraToResize != null)
            cameraToResize.rect = normalized;

        // Resize panels to fit the safe area
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
    }
}