using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanel : MonoBehaviour
{
    public List<GameObject> UIUpgradePrefabs;
    public GridLayoutGroup buttonsGrid;
    public GridLayoutGroup textGrid;
    [SerializeField] CanvasGroup myCanvasGroup;
    public FadeCanvasGroupsWave fadeCanvasGroups;

    void Awake()
    {
        // Clear objects in layout groups
        for (int i = buttonsGrid.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(buttonsGrid.transform.GetChild(i).gameObject);
        }
        for (int i = textGrid.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(textGrid.transform.GetChild(i).gameObject);
        } 
    }

    void Start()
    {
        // Initialize upgrade lists into UI
        foreach (GameObject upgrade in UIUpgradePrefabs)
        {
            // Create button, get text prefab
            GameObject buttonPrefab = Instantiate(upgrade, buttonsGrid.transform);
            UIUpgrade uIUpgrade = buttonPrefab.GetComponent<UIUpgrade>();
            GameObject textPrefab = uIUpgrade.upgradeTextPrefab;

            // Create text obj and give text instance to uIUpgrade
            uIUpgrade.upgradeTextInstance = Instantiate(textPrefab, textGrid.transform);

            // Add to fade canvas groups
            fadeCanvasGroups.individualElements.Add(
                buttonPrefab.GetComponent<CanvasGroup>());
            fadeCanvasGroups.individualElements.Add(
                uIUpgrade.upgradeTextInstance.GetComponent<CanvasGroup>());
        }
        fadeCanvasGroups.Initialize();
    }

}
