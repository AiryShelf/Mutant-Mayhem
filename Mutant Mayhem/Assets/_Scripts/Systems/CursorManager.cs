using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum CursorRangeType { Radius, Bounds }

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [SerializeField] InputActionAsset inputActionAsset;
    [SerializeField] List<GraphicRaycaster> graphicRaycasters = new List<GraphicRaycaster>();
    public Canvas canvas;
    public bool usingCustomCursor = false;
    [SerializeField] float cursorSpeedFactor = 400;
    public float aimDistance = 20f;
    public float aimMinDistance = 5f;
    [SerializeField] int rayDistance = 100;
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
    GameObject currentHoveredObject = null;
    Transform customCursorTrans;
    InputAction clickAction;
    List<RaycastResult> filteredResults = new List<RaycastResult>();

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

        Initialize();
    }

    public void Initialize()
    {
        initialized = true;
        player = FindObjectOfType<Player>();
        InputActionMap uiActionMap = inputActionAsset.FindActionMap("UI");
        clickAction = uiActionMap.FindAction("Click");
        clickAction.performed += CheckForSimulatedClick;
        customCursorTrans = customCursorImage.transform;
        SetAimCursor();
        SetCustomCursorVisible(false);
    }

    void OnEnable()
    {
        
    }

    void OnDisable()
    {
        clickAction.performed -= CheckForSimulatedClick;
    }

    #region Movement

    public void MoveCustomCursorWorldToUi(Vector2 worldPos)
    {
        customCursorTrans.position = (Vector2)Camera.main.WorldToScreenPoint(worldPos);
    }

    public void MoveCustomCursorTo(Vector2 uiPos, CursorRangeType rangeType, Vector2 worldCenter, float worldRadius, Rect rect)
    {
        switch (rangeType)
        {
            case CursorRangeType.Radius:
                uiPos = ClampUiPositionToScreenCircle(uiPos, worldCenter, worldRadius, Camera.main);
                customCursorTrans.position = uiPos;
                break;
            case CursorRangeType.Bounds:
                uiPos = ClampUiPositionToUiBounds(uiPos, rect);
                break;
        }
        customCursorTrans.position = uiPos;
    }

    public void CustomCursorControl()
    {
        if (!InputController.GetJoystickAsMouseState())
            return;

        //Debug.Log("Joystick as mouse is running");
        float joystickX = Input.GetAxis("RightStickHorizontal");
        float joystickY = Input.GetAxis("RightStickVertical");
        Vector2 joystickInput = new Vector2(joystickX, joystickY);

        Vector2 lastAimDir = joystickInput * cursorSpeedFactor * Time.deltaTime;
        Vector2 newCursorPos;

        if (player != null && player.stats.playerShooter.isBuilding)
        {
            newCursorPos = GetCustomCursorUiPos() + lastAimDir / 2;
            MoveCustomCursorTo(newCursorPos, CursorRangeType.Radius, player.transform.position, 6f, new Rect());
        }
        else 
        {
            newCursorPos = GetCustomCursorUiPos() + lastAimDir;
            Rect screenBounds = new Rect(0, 0, Screen.width, Screen.height);
            MoveCustomCursorTo(newCursorPos, CursorRangeType.Bounds, Vector2.zero, 0f, screenBounds);
        } 
        
    }

    #endregion

    #region Click / Hover

    public void CheckForSimulatedClick(InputAction.CallbackContext context)
    {
        if (!InputController.GetJoystickAsMouseState())
            return;

        Debug.Log("Simulated click started...");

        GameObject selected = currentHoveredObject;
        if (selected == null)
            return;

        // **NEW**: If the selected object has a Button component, simulate a click.
        Button button = selected.GetComponent<Button>();
        if (button != null)
        {
            Debug.Log($"BUTTON FOUND on {selected}! Invoking OnClick!");
            button.onClick.Invoke();
            return;
        }

        // **NEW**: If the selected object has a Dropdown component, simulate a click.
        // Note: Unity's Dropdown doesn't expose a public "Show" method.
        // Instead, we'll send a pointer click event to trigger its default behavior.
        Dropdown dropdown = selected.GetComponent<Dropdown>();
        if (dropdown != null)
        {
            Debug.Log("Dropdown found, clicking...");
            // Simulate a pointer click event.
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(selected, pointerData, ExecuteEvents.pointerClickHandler);
            return;
        }

        // **NEW**: If the selected object has a Toggle component, toggle its state.
        Toggle toggle = selected.GetComponent<Toggle>();
        if (toggle != null)
        {
            Debug.Log("Toggle found! Clicking...");
            // Toggle the value.
            toggle.isOn = !toggle.isOn;
            // Optionally, invoke the onValueChanged event.
            toggle.onValueChanged.Invoke(toggle.isOn);
            return;
        }

        /*
        Debug.Log("Simulating mouse click...");

        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        // This conversion assumes canvas is in Screen Space - Overlay or Camera.
        pointerData.position = customCursorTrans.position;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        foreach (GraphicRaycaster gr in graphicRaycasters)
        {
            gr.Raycast(pointerData, raycastResults);
        }

        // Clear any previous results.
        List<RaycastResult> filteredResults = FilterRaycastResults(raycastResults);

        GameObject clickedObject = filteredResults.Count > 0 ? filteredResults[0].gameObject : null;

        if (clickedObject != null)
        {
            Debug.Log("Raycast: found " + clickedObject + ", looking for button...");
            Button button = clickedObject.GetComponent<Button>();
            if (button != null)
            {
                Debug.Log("BUTTON FOUND!  Invoking...");
                button.onClick.Invoke();
            }
        }

        /*
        Debug.Log("Simulating mouse click...");
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = customCursorTrans.position;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        foreach(var caster in graphicRaycasters)
            caster.Raycast(pointerEventData, raycastResults);

        GameObject clickedObject = raycastResults.Count > 0 ? raycastResults[0].gameObject : null;

        if (clickedObject != null)
        {
            Debug.Log("Raycast: found something, looking for button...");
            Button button = clickedObject.GetComponent<Button>();
            if (button != null)
            {
                Debug.Log("BUTTON FOUND!  Invoking...");
                button.onClick.Invoke();
            }
        }
        */
        
    }

    public void CustomCursorHover()
    {
        if (!InputController.GetJoystickAsMouseState())
            return;

        Debug.Log("CursorHover Ran");
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        // This conversion assumes canvas is in Screen Space - Overlay or Camera.
        pointerEventData.position = customCursorTrans.position;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        foreach(var caster in graphicRaycasters)
            caster.Raycast(pointerEventData, raycastResults);

        List<RaycastResult> filteredResults = FilterRaycastResults(raycastResults);

        GameObject newHoveredObject = filteredResults.Count > 0 ? filteredResults[0].gameObject : null;

        if (newHoveredObject != currentHoveredObject)
        {
            // If there was a previously hovered object, send it a pointer exit event.
            if (currentHoveredObject != null)
            {
                Debug.Log("Simulating pointer exit event on " + currentHoveredObject);
                //EventSystem.current.SetSelectedGameObject(null);
                ExecuteEvents.Execute(currentHoveredObject, pointerEventData, ExecuteEvents.pointerExitHandler);
            }

            // If a new object is now hovered, send it a pointer enter event.
            if (newHoveredObject != null)
            {
                Debug.Log("Simulating pointer enter event on " + newHoveredObject);
                //EventSystem.current.SetSelectedGameObject(newHoveredObject);
                ExecuteEvents.Execute(newHoveredObject, pointerEventData, ExecuteEvents.pointerEnterHandler);
                
            }

            // Update the current hovered object.
            currentHoveredObject = newHoveredObject;
        }
    }

    List<RaycastResult> FilterRaycastResults(List<RaycastResult> raycastResults)
    {
        List<RaycastResult> filteredResults = new List<RaycastResult>();

        foreach (RaycastResult result in raycastResults)
        {
            // **CHANGED:** Check if the GameObject has a Graphic component
            Graphic graphic = result.gameObject.GetComponent<Graphic>();

            if (graphic != null)
            {
                // **CHANGED:** Skip elements that are not raycast targets or are invisible.
                if (!graphic.raycastTarget || graphic.color.a <= 0.01f)
                    continue;
            }

            // **CHANGED:** Only add results that have an interactable component.
            bool isInteractable = false;
            if (result.gameObject.GetComponent<Button>() != null)
                isInteractable = true;
            else if (result.gameObject.GetComponent<Dropdown>() != null)
                isInteractable = true;
            else if (result.gameObject.GetComponent<Toggle>() != null)
                isInteractable = true;
            else if (result.gameObject.GetComponent<Slider>() != null)
                isInteractable = true;

            if (!isInteractable)
                continue;

            // Add the valid result.
            filteredResults.Add(result);
        }

        return filteredResults;
    }


    #endregion

    #region Position / Clamp

    public Vector2 GetCustomCursorWorldPos()
    {
        return Camera.main.ScreenToWorldPoint(customCursorTrans.position);
    }

    public Vector2 GetCustomCursorUiPos()
    {
        return customCursorTrans.position;
    }

    Vector2 ClampUiPositionToScreenCircle(Vector2 screenPos, Vector2 worldCenter, float worldRadius, Camera cam)
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

    #endregion

    #region Cursor Images

    public void SetCursorVisible(bool visible)
    {
        Cursor.visible = visible;
    }

    public void SetCustomCursorVisible(bool visible)
    {
        if (usingCustomCursor)
            customCursorImage.enabled = visible;
        else 
            customCursorImage.enabled = false;
    }

    public void SetUsingCustomCursor(bool isUsing)
    {
        usingCustomCursor = isUsing;

        if (!isUsing)
            SetCustomCursorVisible(false);
    }

    public void SetAimCursor()
    {
        if (!initialized)
            return;
            
        // Repair gun
        if (player != null && player.playerShooter.currentGunIndex == 4)
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