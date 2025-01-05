using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIBuildMenuController : MonoBehaviour
{
    public GridLayoutGroup buttonLayoutGrid;
    public GridLayoutGroup textLayoutGrid;
    [SerializeField] List<GameObject> structureButtonPrefabs;
    public List<UIStructure> uiStructureList;
    [SerializeField] ScrollRectController scrollRectController;
    [SerializeField] CanvasGroup myCanvasGroup;
    public FadeCanvasGroupsWave fadeCanvasGroups;
    //[SerializeField] GameObject tutorialBuildPanelPrefab;

    int currentIndex;
    Player player;
    BuildingSystem buildingSystem;
    InputAction scrollAction;
    InputAction swapWithDestroyAction;

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
        // Initialize structures list and fade groups list
        foreach (GameObject obj in structureButtonPrefabs)
        {
            // Create button in button layout group
            GameObject newButton = Instantiate(obj, buttonLayoutGrid.transform);
            UIStructure uiStructure = newButton.GetComponent<UIStructure>();
            uiStructure.Initialize(buildingSystem, scrollRectController);
            uiStructureList.Add(uiStructure);

            // Create text in text layout group
            uiStructure.textInstance = Instantiate(uiStructure.textPrefab, 
                                                   textLayoutGrid.transform);

            // Initialize FadeCanvasGroup list
            fadeCanvasGroups.individualElements.Add(uiStructure.textInstance.GetComponent<CanvasGroup>());
            fadeCanvasGroups.individualElements.Add(newButton.GetComponent<CanvasGroup>());
        }

        //SetButtonNavigation();
    }

    void SetButtonNavigation()
    {
        for (int i = 0; i < uiStructureList.Count; i++)
        {
            Button button = uiStructureList[i].GetComponent<Button>();
            Navigation nav = button.navigation;
            nav.mode = Navigation.Mode.Explicit;

            if (i == 0)
            {
                // First button - no upward navigation
                nav.selectOnUp = null;
                nav.selectOnDown = uiStructureList[i + 1].GetComponent<Button>();

                button.navigation = nav;
            }
            else if (i == uiStructureList.Count - 1)
            {
                // Last button - no downward navigation
                nav.selectOnUp = uiStructureList[i - 1].GetComponent<Button>();
                nav.selectOnDown = null;

                button.navigation = nav;
            }
            else
            {
                // Middle buttons - navigate both up and down
                //nav.selectOnUp = structureButtonInstances[i - 1].GetComponent<Button>();
                //nav.selectOnDown = structureButtonInstances[i + 1].GetComponent<Button>();
            }

            //button.navigation = nav;
        }
    }

    public void RefreshBuildList()
    {
        foreach (UIStructure structure in uiStructureList)
        {
            // Make unlocked structures interactable
            if (BuildingSystem._UnlockedStructuresDict[structure.structureSO.structureType])
                structure.MakeInteractable();
        }
    }

    public void OpenBuildMenu(bool open)
    {
        if (open)
        {
            fadeCanvasGroups.isTriggered = true;
            myCanvasGroup.blocksRaycasts = true;
        }
        else
        {
            fadeCanvasGroups.isTriggered = false;
            myCanvasGroup.blocksRaycasts = false;
        }
    }

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
                Debug.Log("BuildMenu selection changed to " + uiStructure.name);
                return true;
            }
        }

        Debug.LogError("UIBuildMenuController: SetMenuSelection failed");
        return false;
    }

    void OnScroll(InputAction.CallbackContext context)
    {
        if (!player.stats.playerShooter.isBuilding)
            return;

        float scrollDelta = context.ReadValue<float>();

        // Compare
        if (scrollDelta > 0)
        {
            ScrollUp();
        }
        else if (scrollDelta < 0)
        {
            ScrollDown();
        }
    }

    void ScrollDown()
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

    void ScrollUp()
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
}
