using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum CursorRangeType { Radius, Bounds }

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [SerializeField] InputActionAsset inputActionAsset;
    public Transform worldCursorTrans;
    
    [Header("Aim Cursor")]
    public float aimDistance = 20f;
    public float aimMinDistance = 5f;
    [SerializeField] Texture2D aimCursorTexture;
    [SerializeField] Sprite aimCursor;
    [SerializeField] Vector2 aimCursorHotspot = Vector2.zero;

    [Header("Build Cursor")]
    [SerializeField] Texture2D buildCursorTexture;
    [SerializeField] Sprite buildCursor;
    [SerializeField] Vector2 buildCursorHotspot = Vector2.zero;

    [Header("Repair Cursor")]
    [SerializeField] Texture2D repairCursorTexture;
    [SerializeField] Sprite repairCursor;
    [SerializeField] Vector2 repairCursorHotspot = Vector2.zero;

    [Header("Custom Cursor")]
    public bool usingCustomCursor = false;
    [SerializeField] float cursorSpeedFactor = 400;
    [SerializeField] Image customCursorImage;
    [SerializeField] List<GraphicRaycaster> graphicRaycasters = new List<GraphicRaycaster>();
    [SerializeField] GraphicRaycaster persistentCanvasGR;
    [SerializeField] int rayDistance = 100;

    Player player;

    bool initialized;
    bool isDropdownOpen = false;
    public GameObject currentHoveredObject = null;
    Transform customCursorTrans;
    InputAction clickAction;
    [SerializeField] List<GameObject> filteredResults = new List<GameObject>();
    Dictionary<int, List<GameObject>> sortedResults = new Dictionary<int, List<GameObject>>()
    {
        { 0, new List<GameObject>() }, // Toggles
        { 1, new List<GameObject>() }, // Buttons
        { 2, new List<GameObject>() }, // Dropdowns
        { 3, new List<GameObject>() }, // Sliders
        { 4, new List<GameObject>() }  // InputFields
    };

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

        SetCustomCursorVisible(false);
        Initialize();
    }

    public void Initialize()
    {
        initialized = true;
        player = FindObjectOfType<Player>();
        InputActionMap uiActionMap = inputActionAsset.FindActionMap("UI");
        clickAction = uiActionMap.FindAction("SimulatedClick");
        clickAction.started += CheckForSimulatedClick;
        customCursorTrans = customCursorImage.transform;
        SetAimCursor();
    }

    void OnEnable()
    {
        
    }

    void OnDisable()
    {
        clickAction.started -= CheckForSimulatedClick;
    }

    void Update()
    {
        CustomCursorControl();
        CustomCursorHover();
    }

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

    public void SetGraphicRaycasters(List<GraphicRaycaster> raycasters)
    {
        graphicRaycasters.Clear();
        graphicRaycasters = raycasters;
        graphicRaycasters.Add(persistentCanvasGR);
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

        Vector2 lastAimDir = joystickInput * cursorSpeedFactor * Time.unscaledDeltaTime;
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

    #region Click

    public void CheckForSimulatedClick(InputAction.CallbackContext context)
    {
        if (!InputController.GetJoystickAsMouseState() || !usingCustomCursor)
            return;

        Debug.Log("Simulated click started on currentHoveredObject: " + currentHoveredObject);

        //EventSystem.current.SetSelectedGameObject(null);

        GameObject selected = currentHoveredObject;
        if (selected == null)
            return;

        Button button = selected.GetComponent<Button>();
        if (button != null)
        {
            Debug.Log($"BUTTON FOUND on {selected}! Invoking OnClick!");
            button.onClick.Invoke();
            return;
        }

        TMP_Dropdown dropdown = selected.GetComponent<TMP_Dropdown>();
        if (dropdown != null)
        {
            Debug.Log($"Dropdown found on {selected}, clicking...");
            // Simulate a pointer click event.
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(selected, pointerData, ExecuteEvents.pointerClickHandler);
            return;
        }

        Toggle toggle = selected.GetComponent<Toggle>();
        if (toggle != null)
        {
            Debug.Log($"Toggle found on {selected}, clicking...");
            // Toggle the value.
            toggle.isOn = !toggle.isOn;
            // Optionally, invoke the onValueChanged event.
            toggle.onValueChanged.Invoke(toggle.isOn);
            return;
        }

        TMP_InputField inputField = selected.GetComponent<TMP_InputField>();
        if (inputField != null)
        {
            Debug.Log($"Input field found on {selected}, clicking...");
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(selected, pointerData, ExecuteEvents.pointerClickHandler);
            return;
        }

        Slider slider = selected.GetComponent<Slider>();
        if (slider != null)
        {
            Debug.Log($"Slider found on {selected}, clicking...");
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(selected, pointerData, ExecuteEvents.pointerClickHandler);

            RectTransform sliderRect = slider.GetComponent<RectTransform>();

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(sliderRect, customCursorTrans.position, null, out Vector2 localPoint))
            {
                // Normalize the click position to get a value between 0 and 1
                float normalizedValue = Mathf.InverseLerp(-sliderRect.rect.width / 2, sliderRect.rect.width / 2, localPoint.x);

                // Convert normalized value to slider value
                slider.value = Mathf.Lerp(slider.minValue, slider.maxValue, normalizedValue);
            }

            return;
        }

        // Click anyways
        PointerEventData pointerData2 = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute(selected, pointerData2, ExecuteEvents.pointerClickHandler);
        
    }

    #endregion

    #region Hover

    public void CustomCursorHover()
    {
        if (!InputController.GetJoystickAsMouseState() || !usingCustomCursor)
            return;

        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = customCursorTrans.position;

        // UI raycasting
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        foreach(var caster in graphicRaycasters)
            caster.Raycast(pointerEventData, raycastResults);

        // Handle Dropdown menus
        foreach(var caster in graphicRaycasters)
        {
            Transform dropdown = FindChildRecursive(caster.transform, "Dropdown List");
            if (dropdown != null)
            {
                GraphicRaycaster dropdownRaycaster = dropdown.GetComponent<GraphicRaycaster>();
                if (dropdownRaycaster != null)
                {
                    dropdownRaycaster.Raycast(pointerEventData, raycastResults);
                }
                break;
            }
        }
        /*
        GameObject dropdownList = GameObject.Find("Dropdown List"); // TMP_Dropdown generates this name
        if (dropdownList != null)
        {
            GraphicRaycaster dropdownRaycaster = dropdownList.GetComponentInParent<GraphicRaycaster>();
            if (dropdownRaycaster != null)
            {
                dropdownRaycaster.Raycast(pointerEventData, raycastResults);
            }
        }
        */

        FilterRaycastResults(raycastResults);
        GameObject newHoveredUIObject = filteredResults.Count > 0 ? filteredResults[0] : null;

        // 3D raycasting
        Ray ray = Camera.main.ScreenPointToRay(customCursorTrans.position);
        RaycastHit hit;
        GameObject newHovered3DObject = null;
        if (Physics.Raycast(ray, out hit))
        {
            newHovered3DObject = hit.collider.gameObject;
        }

        // Determine the object to be hovered (UI takes priority over 3D if both are found)
        GameObject newHoveredObject = newHoveredUIObject != null ? newHoveredUIObject : newHovered3DObject;
        if (newHoveredObject != currentHoveredObject)
        {
            if (currentHoveredObject != null)
            {
                Debug.Log("Simulating pointer exit event on " + currentHoveredObject);
                ExecuteEvents.Execute(currentHoveredObject, pointerEventData, ExecuteEvents.pointerExitHandler);
                EventSystem.current.SetSelectedGameObject(null);
            }

            currentHoveredObject = newHoveredObject;

            if (newHoveredObject != null)
            {
                Debug.Log("Simulating pointer enter event on " + newHoveredObject);
                ExecuteEvents.Execute(newHoveredObject, pointerEventData, ExecuteEvents.pointerEnterHandler);
            }
        }
    }

    public static Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform found = FindChildRecursive(child, childName);
            if (found != null)
                return found;
        }
        return null;
    }

    #endregion

    #region Filter Results

    void FilterRaycastResults(List<RaycastResult> raycastResults)
    {
        sortedResults = new Dictionary<int, List<GameObject>>()
        {
            { 0, new List<GameObject>() }, // Toggles
            { 1, new List<GameObject>() }, // Buttons
            { 2, new List<GameObject>() }, // Dropdowns
            { 3, new List<GameObject>() }, // Sliders
            { 4, new List<GameObject>() }  // InputFields
        };

        foreach (RaycastResult result in raycastResults)
        {
            Graphic graphic = result.gameObject.GetComponent<Graphic>();

            if (graphic != null)
            {
                if (!graphic.raycastTarget)
                {
                    Debug.Log($"Graphic found on {result.gameObject} was not a raycast target");
                    continue;
                }
                if (graphic.color.a <= 0.01f)
                {
                    Debug.Log($"Graphic found on {result.gameObject} was not a visible target, still accepting");
                }
            }
            else
            {
                Debug.Log("No Graphic found on " + result.gameObject);
            }

            // Determine the type of interactable component and sort accordingly
            if (result.gameObject.GetComponent<Toggle>() != null)
                sortedResults[0].Add(result.gameObject);
            else if (result.gameObject.GetComponent<Button>() != null)
                sortedResults[1].Add(result.gameObject);
            else if (result.gameObject.GetComponent<TMP_Dropdown>() != null)
                sortedResults[2].Add(result.gameObject);
            else if (result.gameObject.GetComponent<Slider>() != null)
                sortedResults[3].Add(result.gameObject);
            else if (result.gameObject.GetComponent<TMP_InputField>() != null)
                sortedResults[4].Add(result.gameObject);
            else
            {
                Debug.Log("No interactable component found on " + result.gameObject);
                continue;
            }
        }

        // Toggles with prefix "Item" go first
        sortedResults[0].Sort((a, b) =>
        {
            bool aIsItem = a.name.StartsWith("Item");
            bool bIsItem = b.name.StartsWith("Item");
            return aIsItem == bIsItem ? 0 : (aIsItem ? -1 : 1); // "Item" names come first
        });

        // Flatten sorted results into a single list, preserving order
        filteredResults = new List<GameObject>();
        foreach (var category in sortedResults)
            filteredResults.AddRange(category.Value);

        Debug.Log($"Filtered raycast results count: {filteredResults.Count}");
        
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