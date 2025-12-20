using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class UIRaycastProbe : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool logWhenNotClicking = false;
    [SerializeField] private bool logResults = true;

#if UNITY_EDITOR
    void Update()
    {
        if (EventSystem.current == null)
        {
            if (logWhenNotClicking)
                Debug.LogWarning("UIRaycastProbe: No EventSystem.current in the scene.");
            return;
        }

        if (Mouse.current == null)
        {
            if (logWhenNotClicking)
                Debug.LogWarning("UIRaycastProbe: Mouse.current is null (no mouse device detected).");
            return;
        }

        if (!Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (logWhenNotClicking)
                Debug.Log("UIRaycastProbe: no click this frame");
            return;
        }

        var data = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        if (!logResults)
            return;

        Debug.Log($"---- UI Raycast Results (top = first hit) : {results.Count} ----");
        foreach (var r in results)
            Debug.Log($"{r.gameObject.name} | depth:{r.depth} | module:{r.module}");
    }
#endif
}