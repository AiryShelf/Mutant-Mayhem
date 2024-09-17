using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIAugmentation : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] Color selectedColor;
    [SerializeField] Color addedColor;
    Color originalIconColor;
    Color originalTextColor;

    [Header("Dynamic from UIAugPanel, don't set")]
    public AugmentationBaseSO aug;

    AugManager augManager;
    UIAugPanel augPanel;
    bool selected;
    bool addedToManager;

    public void Setup(AugmentationBaseSO augmentation, AugManager manager, 
                      UIAugPanel panel)
    {
        originalIconColor = icon.color;
        originalTextColor = nameText.color;
        aug = augmentation;
        augManager = manager;
        augPanel = panel;
        icon.sprite = augmentation.uiImage;
        nameText.text = augmentation.augmentationName;
        costText.text = "Cost: " + augmentation.cost.ToString() + " RP"; 

        UpdateIconAndText();                           
    }

    public void OnSelect(BaseEventData data)
    {
        selected = true;
        augPanel.SelectAugmentation(this);
        UpdateIconAndText();
    }
    
    public void OnDeselect(BaseEventData data)
    {
        selected = false;
        UpdateIconAndText();
    }

    public void AddToSelectedAugs()
    {
        if (!addedToManager)
        {
            // Check RP
            if (augManager.currentResearchPoints < aug.cost)
            {
                Debug.Log("Not enough credits for Aug");
            }

            // Check maxAugs
            if (augManager.selectedAugsWithLvls.Count >= augManager.maxAugs)
            {
                Debug.Log("Max Augs already selected");
                return;
            }    

            // Check for duplicate type that isn't 'other'
            if (aug.type != AugmentationType.Other)
            {
                foreach (KeyValuePair<AugmentationBaseSO, int> kvp in augManager.selectedAugsWithLvls)
                {
                    if (kvp.Key.type == aug.type)
                    {
                        Debug.Log("Tried to select Aug of conflicting Type");
                        return;
                    }
                }
            }

            // Check for duplicates
            if (augManager.selectedAugsWithLvls.ContainsKey(aug))
            {
                Debug.LogError("UIAugmentation already exists in selected Augs, can't add");
                return;
            }

            addedToManager = true;
            augManager.selectedAugsWithLvls.Add(aug, 1);
            augManager.currentResearchPoints -= aug.cost;
            Debug.Log("Added Augmentation: " + aug.augmentationName);
        }

        UpdateIconAndText();
    }

    public void RemoveFromSelectedAugs()
    {
        if (addedToManager)
        {
            if (!augManager.selectedAugsWithLvls.ContainsKey(aug))
            {
                Debug.LogError("UIAugmentation not found in selected Augs, can't remove");
                return;
            }

            addedToManager = false;
            augManager.selectedAugsWithLvls.Remove(aug);
            // Add credits back depending on level and level mult
            if (!augManager.selectedAugsWithLvls.ContainsKey(aug))
            {
                Debug.LogError("Tried to remove an Aug from Manager that does not exist in dict");
            }
            int level = augManager.selectedAugsWithLvls[aug];
            // Clamp level - 1 to only account for extra levels cost
            int clampedLevel = Mathf.Clamp(level - 1, 0, int.MaxValue);
            augManager.currentResearchPoints += Mathf.FloorToInt(aug.cost + (aug.lvlCostIncrement * clampedLevel * aug.lvlCostIncrementMult)); 
            Debug.Log("Removed an Augmentation");
        }

        UpdateIconAndText();
    }

    public void UpdateIconAndText()
    {
        Color costColor;
        if (augManager.currentResearchPoints < aug.cost)
            costColor = Color.red;
        else
            costColor = Color.green;
        
        if (addedToManager)
        {
            icon.color = addedColor;
            nameText.color = addedColor;
            costText.color = addedColor;
        }
        else if (selected)
        {
            icon.color = selectedColor;
            nameText.color = selectedColor;
            costText.color = costColor;
        }
        else
        {
            icon.color = originalIconColor;
            nameText.color = originalTextColor;
            costText.color = Color.black + costColor / 2;
        }
    }
}
