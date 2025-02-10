using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CursorRangeType { Radius, Bounds }

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    public float aimDistance = 20f;
    public float aimMinDistance = 5f;
    [SerializeField] Texture2D aimCursorTexture;
    [SerializeField] Sprite aimCursor;
    [SerializeField] Vector2 aimCursorHotspot = Vector2.zero;
    [SerializeField] Texture2D buildCursorTexture;
    [SerializeField] Sprite buildCursor;
    [SerializeField] Vector2 buildCursorHotspot = Vector2.zero;
    [SerializeField] Texture2D repairCursorTexture;
    [SerializeField] Sprite repairCursor;
    [SerializeField] Vector2 repairCursorHotspot = Vector2.zero;
    [SerializeField] Image customCursorImage;
    Player player;

    bool initialized;
    Transform customCursorTrans;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void Initialize()
    {
        initialized = true;
        player = FindObjectOfType<Player>();
        customCursorTrans = customCursorImage.transform;
        SetAimCursor();
        SetCustomCursorVisible(false);
    }

    public void MoveCustomCursorTo(Vector2 pos)
    {
        customCursorTrans.position = Camera.main.WorldToScreenPoint(pos);
    }

    public void MoveCustomCursorToUi(Vector2 pos, CursorRangeType rangeType, Vector2 circleCenter, float radius, Rect rect)
    {
        switch (rangeType)
        {
            case CursorRangeType.Radius:
                pos = ClampPositionWorldToScreenCircle(pos, circleCenter, radius, Camera.main);
                customCursorTrans.position = pos;
                break;
            case CursorRangeType.Bounds:
                pos = ClampPositionToUiBounds(pos, rect);
                break;
        }
        customCursorTrans.position = pos;
    }

    public Vector2 GetCustomCursorWorldPos()
    {
        return Camera.main.ScreenToWorldPoint(customCursorTrans.position);
    }

    public Vector2 GetCustomCursorUiPos()
    {
        return customCursorTrans.position;
    }

    Vector2 ClampPositionWorldToScreenCircle(Vector2 pos, Vector2 center, float radius, Camera cam)
    {
        // [CHANGE] Convert world position 'pos' to screen space.
        Vector3 screenPos = cam.WorldToScreenPoint(new Vector3(pos.x, pos.y, 0));
        center = cam.WorldToScreenPoint(center);

        // Calculate the offset in screen space.
        Vector2 offset = pos - center;
        
        // Check if the screen position is outside the screen-space circle.
        if (offset.sqrMagnitude > radius * radius)
        {
            // Clamp the offset to the screen radius.
            offset = offset.normalized * radius;
            // [CHANGE] Apply the clamped offset to the screen center.
            Vector2 clampedScreenPos = new Vector2(center.x, center.y) + offset;
            screenPos.x = clampedScreenPos.x;
            screenPos.y = clampedScreenPos.y;
        }

        // [CHANGE] Convert the clamped screen position back to world space.
        // Note: The Z value in screenPos (depth) is preserved from the WorldToScreenPoint conversion.
        return cam.WorldToScreenPoint(screenPos);

        /*
        // Calculate the offset from the center
        Vector2 offset = pos - center;
        // Check if outside the circle (using sqrMagnitude for efficiency)
        if (offset.sqrMagnitude > radius * radius)
        {
            // Clamp the offset to the radius and recompute the position
            offset = offset.normalized * radius;
            pos = center + offset;
        }
        return pos;
        */
    }

    Vector2 ClampPositionToUiBounds(Vector2 pos, Rect bounds)
    {
        pos.x = Mathf.Clamp(pos.x, bounds.xMin, bounds.xMax);
        pos.y = Mathf.Clamp(pos.y, bounds.yMin, bounds.yMax);
        return pos;
    }

    #region Cursor Images

    public void SetCursorVisible(bool visible)
    {
        Cursor.visible = visible;
    }

    public void SetCustomCursorVisible(bool visible)
    {
        customCursorImage.enabled = visible;
    }

    public void SetAimCursor()
    {
        if (!initialized)
            return;
            
        // Repair gun
        if (player.playerShooter.currentGunIndex == 4)
        {
            SetRepairCursor();
            customCursorImage.sprite = repairCursor;
        }
        else
        {
            // Aiming cursor
            Cursor.SetCursor(aimCursorTexture, aimCursorHotspot, CursorMode.Auto);
            customCursorImage.sprite = aimCursor;
        }
    }

    public void SetBuildCursor()
    {
        Cursor.SetCursor(buildCursorTexture, buildCursorHotspot, CursorMode.Auto);
        customCursorImage.sprite = buildCursor;
    }

    public void SetRepairCursor()
    {
        Cursor.SetCursor(repairCursorTexture, repairCursorHotspot, CursorMode.Auto);
        customCursorImage.sprite = repairCursor;
    }

    public void SetSystemCursor()
    {
        // Reset to system default
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    #endregion
}