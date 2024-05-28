using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildMenuController : MonoBehaviour
{
    public GridLayoutGroup buttonLayoutGroup;
    public GridLayoutGroup textLayoutGroup;
    [SerializeField] List<GameObject> structureButtonPrefabs;
    [HideInInspector] public List<GameObject> structureButtonInstances;
    public FadeCanvasGroupsWave fadeCanvasGroups;


    void Awake()
    {
        // Clear objects in layout groups
        for (int i = buttonLayoutGroup.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(buttonLayoutGroup.transform.GetChild(i).gameObject);
        }

        for (int i = textLayoutGroup.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(textLayoutGroup.transform.GetChild(i).gameObject);
        }

        // Initialize structures list and fade groups list
        foreach (GameObject obj in structureButtonPrefabs)
        {
            // Create button in button layout group
            GameObject newButton = Instantiate(obj, buttonLayoutGroup.transform);
            structureButtonInstances.Add(newButton);

            // Create text in text layout group
            UIStructure uIStructure = newButton.GetComponent<UIStructure>();
            uIStructure.textInstance = Instantiate(uIStructure.textPrefab, 
                                                   textLayoutGroup.transform);

            // Initialize FadeCanvasGroup list
            fadeCanvasGroups.individualGroups.Add(uIStructure.textInstance.GetComponent<CanvasGroup>());
            fadeCanvasGroups.individualGroups.Add(newButton.GetComponent<CanvasGroup>());
        }       
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
