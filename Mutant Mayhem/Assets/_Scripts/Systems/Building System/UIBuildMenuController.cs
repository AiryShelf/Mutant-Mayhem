using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIBuildMenuController : MonoBehaviour
{
    [SerializeField] float touchscreenScrollDelay = 0.2f;
    [SerializeField] float dragDeadzone = 10f;
    public GridLayoutGroup buttonLayoutGrid;
    public GridLayoutGroup textLayoutGrid;
    [SerializeField] List<GameObject> structureButtonPrefabs;
    public List<UIStructure> uiStructureList;
    [SerializeField] ScrollRectController scrollRectController;
    [SerializeField] CanvasGroup myCanvasGroup;
    public FadeCanvasGroupsWave fadeCanvasGroups;

    public bool isTouchScrolling = false;
    bool isScrollDelay = false;
    public Vector2 touchStartPos;
    int currentIndex;
    Player player;
    BuildingSystem buildingSystem;
    InputAction scrollAction;
    InputAction swapWithDestroyAction;
    bool isMenuOpen = false;

    void Awake()
    {
        player = FindObjectOfType<Player>();

        // Clear objects in layout groups
        for (int i = buttonLayoutGrid.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(buttonLayoutGrid.transform.GetChild(i).gameObject);
        }

        for (int i = textLayoutGrid.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(textLayoutGrid.transform.GetChild(i).gameObject);
        }

        buildingSystem = FindObjectOfType<BuildingSystem>();
        InitializeBuildList();
    }
    
    void OnEnable()
    {
        if (player == null)
        {
            Debug.LogError("BuildMenuController did not find Player in scene");
            return;
        }
        //PlayerCredits = playerStartingCredits;

        InputActionMap uiActionMap = player.inputAsset.FindActionMap("UI");
        scrollAction = uiActionMap.FindAction("Scroll");
        InputActionMap playerActionMap = player.inputAsset.FindActionMap("Player");
        swapWithDestroyAction = playerActionMap.FindAction("Toolbar");

        scrollAction.performed += OnScroll;
        swapWithDestroyAction.performed += buildingSystem.SwapWithDestroyTool;
    }

    void OnDisable()
    {
        scrollAction.performed -= OnScroll;
    }

    void Start()
    {
        fadeCanvasGroups.InitializeToFadedOut(); 
        myCanvasGroup.blocksRaycasts = false; 
    }

    void InitializeBuildList()
    {
        uiStructureList.Clear();
        int index = 0;
        // Initialize structures list and fade groups list
        foreach (GameObject obj in structureButtonPrefabs)
        {
            // Create button in button layout group
            GameObject newButton = Instantiate(obj, buttonLayoutGrid.transform);
            UIStructure uiStructure = newButton.GetComponent<UIStructure>();

            uiStructure.originalSiblingIndex = index;
            index++;

            uiStructure.Initialize(buildingSystem, scrollRectController);
            uiStructureList.Add(uiStructure);

            // Create text in text layout group
            uiStructure.textInstance = Instantiate(uiStructure.textPrefab, 
                                                   textLayoutGrid.transform);

            // Initialize FadeCanvasGroup list
            fadeCanvasGroups.individualElements.Add(uiStructure.textInstance.GetComponent<CanvasGroup>());
            fadeCanvasGroups.individualElements.Add(newButton.GetComponent<CanvasGroup>());
        }

        RefreshBuildList();
    }

    #region Refresh / Toggle

    public void RefreshBuildList()
    {
        foreach (UIStructure structure in uiStructureList)
        {
            // Make unlocked structures interactable
            if (BuildingSystem._UnlockedStructuresDict[structure.structureSO.structureType])
                structure.MakeInteractable(true);
            else
                structure.MakeInteractable(false);
            
        }

        uiStructureList = uiStructureList
            .OrderByDescending(structure => BuildingSystem._UnlockedStructuresDict[structure.structureSO.structureType])
            .ThenBy(structure => structure.originalSiblingIndex)
            .ToList();

        for (int i = 0; i < uiStructureList.Count; i++)
        {
            uiStructureList[i].transform.SetSiblingIndex(i);
            uiStructureList[i].textInstance.transform.SetSiblingIndex(i);
            
        }
    }

    public void ToggleBuildMenu()
    {
        fadeCanvasGroups.isTriggered = !isMenuOpen;
        myCanvasGroup.blocksRaycasts = !isMenuOpen;
        isMenuOpen = !isMenuOpen;
    }

    #endregion

    #region Select

    public bool SetMenuSelection(StructureSO structure)
    {
        foreach (UIStructure uiStructure in uiStructureList)
        {
            if (uiStructure == null)
                continue;
            
            if (uiStructure.structureSO.tileName == structure.tileName)
            {
                if (!uiStructure.TryToSelect())
                    return false;
                    
                currentIndex = uiStructureList.IndexOf(uiStructure);
                //Debug.Log("BuildMenu selection changed to " + uiStructure.name);
                return true;
            }
        }

        Debug.LogError("UIBuildMenuController: SetMenuSelection failed");
        return false;
    }

    #endregion

    #region Scroll

    void OnScroll(InputAction.CallbackContext context)
    {
        Debug.Log("OnScroll performed");
        if (!isMenuOpen)
            return;
        
        Vector2 scroll = context.ReadValue<Vector2>();
        
        if (scroll.y > 0)
            ScrollUp();
        else if (scroll.y < 0)
            ScrollDown();
    }

    private bool IsPositionWithinRect(RectTransform rectTransform, Vector2 screenPosition)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPosition);
    }

    public void ScrollUp()
    {
        if (currentIndex <= 0)
            return;

        int startIndex = currentIndex;
        currentIndex--;
        
        for (int i = currentIndex; i >= 0; i--) 
        {
            if (SetMenuSelection(uiStructureList[i].structureSO))
                return;
            currentIndex--;
        }

        currentIndex = startIndex;  
    }

    public void ScrollDown()
    {
        if (currentIndex >= uiStructureList.Count - 1)
            return;

        int startIndex = currentIndex;
        currentIndex++;
        
        for (int i = currentIndex; i < uiStructureList.Count; i++) 
        {
            if (SetMenuSelection(uiStructureList[i].structureSO))
                return;
            currentIndex++;
        }

        currentIndex = startIndex;        
    }

    #endregion
}
