using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildMenuController : MonoBehaviour
{
    public GridLayoutGroup buttonLayoutGrid;
    public GridLayoutGroup textLayoutGrid;
    [SerializeField] List<GameObject> structureButtonPrefabs;
    [HideInInspector] public List<GameObject> structureButtonInstances;
    public FadeCanvasGroupsWave fadeCanvasGroups;


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
        fadeCanvasGroups.Initialize();     
    }

    public void OpenPanel(bool active)
    {
        if (active)
        {
            fadeCanvasGroups.isTriggered = true;
        }
        else
        {
            fadeCanvasGroups.isTriggered = false;
        }
    }

}
