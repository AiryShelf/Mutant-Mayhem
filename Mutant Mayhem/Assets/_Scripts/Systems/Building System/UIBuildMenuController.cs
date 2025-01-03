using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIBuildMenuController : MonoBehaviour
{
    public GridLayoutGroup buttonLayoutGrid;
    public GridLayoutGroup textLayoutGrid;
    [SerializeField] List<GameObject> structureButtonPrefabs;
    [HideInInspector] public List<GameObject> structureButtonInstances;
    [SerializeField] ScrollRectController scrollRectController;
    [SerializeField] CanvasGroup myCanvasGroup;
    public FadeCanvasGroupsWave fadeCanvasGroups;
    //[SerializeField] GameObject tutorialBuildPanelPrefab;
    [SerializeField] RectTransform gamePlayCanvas;

    BuildingSystem buildingSystem;

    void Awake()
    {
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
            structureButtonInstances.Add(newButton);
            UIStructure uIStructure = newButton.GetComponent<UIStructure>();
            uIStructure.Initialize(buildingSystem, scrollRectController);

            // Create text in text layout group
            uIStructure.textInstance = Instantiate(uIStructure.textPrefab, 
                                                   textLayoutGrid.transform);

            // Initialize FadeCanvasGroup list
            fadeCanvasGroups.individualElements.Add(uIStructure.textInstance.GetComponent<CanvasGroup>());
            fadeCanvasGroups.individualElements.Add(newButton.GetComponent<CanvasGroup>());
        }

        SetButtonNavigation();
    }

    void SetButtonNavigation()
    {
        for (int i = 0; i < structureButtonInstances.Count; i++)
        {
            Button button = structureButtonInstances[i].GetComponent<Button>();
            Navigation nav = button.navigation;
            nav.mode = Navigation.Mode.Explicit;

            if (i == 0)
            {
                // First button - no upward navigation
                nav.selectOnUp = null;
                nav.selectOnDown = structureButtonInstances[i + 1].GetComponent<Button>();

                button.navigation = nav;
            }
            else if (i == structureButtonInstances.Count - 1)
            {
                // Last button - no downward navigation
                nav.selectOnUp = structureButtonInstances[i - 1].GetComponent<Button>();
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
        foreach (GameObject obj in structureButtonInstances)
        {
            // Make unlocked structures interactable
            UIStructure uiStructure = obj.GetComponent<UIStructure>();
            if (BuildingSystem._UnlockedStructuresDict[uiStructure.structureSO.structureType])
                uiStructure.MakeInteractable();
        }
    }

    public void OpenBuildMenu(bool open)
    {
        if (open)
        {
            //if (!TutorialManager.TutorialShowedBuild)
                //StartCoroutine(DelayTutorialOpen());

            fadeCanvasGroups.isTriggered = true;
            myCanvasGroup.blocksRaycasts = true;
        }
        else
        {
            fadeCanvasGroups.isTriggered = false;
            myCanvasGroup.blocksRaycasts = false;
        }
    }

    IEnumerator DelayTutorialOpen()
    {
        yield return new WaitForSeconds(0.2f);

        //Instantiate(tutorialBuildPanelPrefab, gamePlayCanvas);
    }

    public void SetMenuSelection(StructureSO structure)
    {
        foreach (Transform trans in buttonLayoutGrid.transform)
        {
            UIStructure UiStructure = trans.GetComponent<UIStructure>();
            if (UiStructure == null)
                continue;
            
            if (structure == UiStructure.structureSO.ruleTileStructure.structureSO)
            {
                EventSystem.current.SetSelectedGameObject(trans.gameObject);
                Debug.Log("BuildMenu selection forced to " + trans.name);
                return;
            }
            
        }
        Debug.Log("BuildMenu selection force failed");
    }
}
