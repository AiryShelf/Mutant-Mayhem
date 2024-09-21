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
    [SerializeField] Color addedNegColor;
    Color originalIconColor;
    Color originalTextColor;

    [Header("Dynamic from UIAugPanel, don't set here")]
    public AugmentationBaseSO aug;
    public int totalCost;

    AugManager augManager;
    UIAugPanel augPanel;
    public bool selected;

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

        UpdateIconAndText();                           
    }

    public void OnSelect(BaseEventData data)
    {
        augPanel.SelectAugmentation(this);
        UpdateIconAndText();
        
        Debug.Log("Selected an Aug");
    }
    
    public void OnDeselect(BaseEventData data)
    {
        UpdateIconAndText();
    }

    public bool AddToSelectedAugs(int level)
    {
        if (augManager.selectedAugsWithLvls.ContainsKey(aug))
        {
            Debug.LogError("UIAugmentation already exists in selected Augs, can't add");
            return false;
        } 

        // Check for duplicate type that isn't 'other'
        if (aug.type != AugmentationType.Other)
        {
            foreach (KeyValuePair<AugmentationBaseSO, int> kvp in augManager.selectedAugsWithLvls)
            {
                if (kvp.Key.type == aug.type)
                {
                    Debug.LogWarning("Tried to select Aug of conflicting Type");
                    return false;
                }
            }
        }

        // Check for duplicates
        if (augManager.selectedAugsWithLvls.ContainsKey(aug))
        {
            Debug.LogError("UIAugmentation already exists in selected Augs, can't add");
            return false;
        }

        augManager.selectedAugsWithLvls.Add(aug, level);
        Debug.Log("Added Augmentation: " + aug.augmentationName);

        UpdateIconAndText();
        return true;
    }

    public void RemoveFromSelectedAugs()
    {
        if (!augManager.selectedAugsWithLvls.ContainsKey(aug))
        {
            Debug.LogError("UIAugmentation not found in selected Augs, can't remove");
            return;
        }

        augManager.selectedAugsWithLvls.Remove(aug);
        Debug.Log("Removed an Augmentation");

        UpdateIconAndText();
    }

    public void UpdateIconAndText()
    {
        string costOrGainText;
        Color costColor;
        if (totalCost == 0)
        {
            costOrGainText = "Not added";
            costColor = originalTextColor/1.2f;
            costText.text = costOrGainText;
        }
        else if (totalCost < 0)
        {
            costOrGainText = "Cost: ";
            costColor = Color.yellow;
            costText.text = costOrGainText + Mathf.Abs(totalCost) + " RP";  
        }
        else
        {
            costOrGainText = "Gain: ";
            costColor = Color.green;
            costText.text = costOrGainText + Mathf.Abs(totalCost) + " RP";  
        }

          
        
        if (augManager.selectedAugsWithLvls.ContainsKey(aug))
        {
            int level = augManager.selectedAugsWithLvls[aug];
            if (level >= 0)
            {
                icon.color = addedColor;
                nameText.color = addedColor;
                costText.color = costColor;
            }
            else
            {
                icon.color = addedNegColor;
                nameText.color = addedNegColor;
                costText.color = costColor;
            }
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

        if (augManager.selectedAugsWithLvls.ContainsKey(aug))
        {
            nameText.text = aug.augmentationName + " " + augManager.selectedAugsWithLvls[aug];
        }
        else
        {
            nameText.text = aug.augmentationName;
        }
    }
}
