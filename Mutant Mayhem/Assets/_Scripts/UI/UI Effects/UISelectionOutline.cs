using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UISelectionOutline : MonoBehaviour
{
    [SerializeField] RectTransform outlineRectTransform;

    GameObject lastSelectedObject;

    void Update()
    {
        if (outlineRectTransform == null) return;

        // Check if the last used device was a Gamepad
        bool isGamepadActive = InputManager.LastUsedDevice == Gamepad.current;
        outlineRectTransform.gameObject.SetActive(isGamepadActive);

        if (!isGamepadActive) return;

        // Get the currently selected UI element
        GameObject selectedObject = CursorManager.Instance.currentHoveredObject;

        if (selectedObject == null || selectedObject == lastSelectedObject)
            return;

        lastSelectedObject = selectedObject;

        // Get RectTransform of selected UI element
        RectTransform selectedRect = selectedObject.GetComponent<RectTransform>();
        if (selectedRect == null) return;

        // Adjust outline position and size
        UpdateOutlinePosition(selectedRect);
    }

    void UpdateOutlinePosition(RectTransform targetRect)
    {
        outlineRectTransform.position = targetRect.position;
        outlineRectTransform.sizeDelta = targetRect.sizeDelta;
    }
}
