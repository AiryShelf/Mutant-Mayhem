using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class UIBuildMenuController : MonoBehaviour
{
    public GridLayoutGroup buttonLayoutGrid;
    public GridLayoutGroup textLayoutGrid;
    [SerializeField] List<GameObject> structureButtonPrefabs;
    [HideInInspector] public List<GameObject> structureButtonInstances;
    [SerializeField] CanvasGroup myCanvasGroup;
    public FadeCanvasGroupsWave fadeCanvasGroups;
    [SerializeField] GameObject tutorialBuildPanelPrefab;
    [SerializeField] RectTransform gamePlayCanvas;

    InputSystemUIInputModule uIInputModule;
    InputActionReference origLeftClickAction;

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

        // Initialize structures list and fade groups list
        foreach (GameObject obj in structureButtonPrefabs)
        {
            // Create button in button layout group
            GameObject newButton = Instantiate(obj, buttonLayoutGrid.transform);
            structureButtonInstances.Add(newButton);

            // Create text in text layout group
            UIStructure uIStructure = newButton.GetComponent<UIStructure>();
            uIStructure.textInstance = Instantiate(uIStructure.textPrefab, 
                                                   textLayoutGrid.transform);

            // Initialize FadeCanvasGroup list
            fadeCanvasGroups.individualElements.Add(uIStructure.textInstance.GetComponent<CanvasGroup>());
            fadeCanvasGroups.individualElements.Add(newButton.GetComponent<CanvasGroup>());
        }      
    }

    void Start()
    {
        fadeCanvasGroups.Initialize(); 
        myCanvasGroup.blocksRaycasts = false;

        uIInputModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();

        origLeftClickAction = uIInputModule.leftClick;
    }

    public void TriggerFadeGroups(bool active)
    {
        if (active)
        {
            if (!TutorialManager.tutorialShowedBuild)
            {
                StartCoroutine(DelayOpen());
                return;
            }

            fadeCanvasGroups.isTriggered = true;
            myCanvasGroup.blocksRaycasts = true;
            uIInputModule.leftClick = null;
        }
        else
        {
            fadeCanvasGroups.isTriggered = false;
            myCanvasGroup.blocksRaycasts = false;
            uIInputModule.leftClick = origLeftClickAction;
        }
    }

    IEnumerator DelayOpen()
    {
        Instantiate(tutorialBuildPanelPrefab, gamePlayCanvas);

        yield return new WaitForFixedUpdate();

        fadeCanvasGroups.isTriggered = true;
        myCanvasGroup.blocksRaycasts = true;
    }
}
