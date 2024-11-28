using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [SerializeField] Texture2D aimCursor;
    [SerializeField] Vector2 aimCursorHotspot = Vector2.zero;
    [SerializeField] Texture2D buildCursor;
    [SerializeField] Vector2 buildCursorHotspot = Vector2.zero;
    [SerializeField] Texture2D repairCursor;
    [SerializeField] Vector2 repairCursorHotspot = Vector2.zero;
    Player player;

    bool initialized;

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
        SetAimCursor();
    }

    public void SetAimCursor()
    {
        if (!initialized)
            return;
            
        // Repair gun
        if (player.playerShooter.currentGunIndex == 9)
            SetRepairCursor();
        else
            // Aiming cursor
            Cursor.SetCursor(aimCursor, aimCursorHotspot, CursorMode.Auto);
    }

    public void SetBuildCursor()
    {
        Cursor.SetCursor(buildCursor, buildCursorHotspot, CursorMode.Auto);
    }

    public void SetRepairCursor()
    {
        Cursor.SetCursor(repairCursor, repairCursorHotspot, CursorMode.Auto);
    }

    public void SetSystemCursor()
    {
        // Reset to system default
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}