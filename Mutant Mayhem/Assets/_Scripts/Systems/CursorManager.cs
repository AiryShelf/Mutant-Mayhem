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

    public void MoveCustomCursorToUi(Vector2 uiPos, CursorRangeType rangeType, Vector2 circleCenter, float radius, Rect rect)
    {
        switch (rangeType)
        {
            case CursorRangeType.Radius:
                uiPos = ClampScreenPositionToWorldCircle(uiPos, circleCenter, radius, Camera.main);
                customCursorTrans.position = uiPos;
                break;
            case CursorRangeType.Bounds:
                uiPos = ClampUiPositionToUiBounds(uiPos, rect);
                break;
        }
        customCursorTrans.position = uiPos;
    }

    public Vector2 GetCustomCursorWorldPos()
    {
        return Camera.main.ScreenToWorldPoint(customCursorTrans.position);
    }

    public Vector2 GetCustomCursorUiPos()
    {
        return customCursorTrans.position;
    }

    Vector2 ClampScreenPositionToWorldCircle(Vector2 screenPos, Vector2 worldCenter, float worldRadius, Camera cam)
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        Vector2 worldOffset = worldPos - worldCenter;
        
        // Check if the screen position is outside the screen-space circle.
        if (worldOffset.sqrMagnitude > worldRadius * worldRadius)
        {
            // Clamp the offset to the screen radius.
            worldOffset = worldOffset.normalized * worldRadius;
            Vector2 clampedWorldPos = worldCenter + worldOffset;
            screenPos = cam.WorldToScreenPoint(clampedWorldPos);
        }

        return screenPos;
    }

    Vector2 ClampUiPositionToUiBounds(Vector2 uiPos, Rect bounds)
    {
        uiPos.x = Mathf.Clamp(uiPos.x, bounds.xMin, bounds.xMax);
        uiPos.y = Mathf.Clamp(uiPos.y, bounds.yMin, bounds.yMax);
        return uiPos;
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