using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIAugmentation : MonoBehaviour, ISelectHandler, IDeselectHandler, 
                              IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] Color selectedColor;
    [SerializeField] Color addedColor;
    [SerializeField] Color addedNegColor;
    [SerializeField] Color highlightedColor = Color.white;
    Color originalIconColor;
    Color originalTextColor;

    [Header("Dynamic from UIAugPanel, don't set here")]
    public AugmentationBaseSO aug;
    public int totalCost;

    AugManager augManager;
    UIAugPanel augPanel;
    public bool selected;
    bool isHovered;

    public void Setup(AugmentationBaseSO augmentation, AugManager manager, UIAugPanel panel)
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
    }
    
    public void OnDeselect(BaseEventData data)
    {
        UpdateIconAndText();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateIconAndText();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
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
        Color costColor;

        // Set Cost or Gain text
        if (totalCost == 0)
        {
            costColor = originalTextColor/1.2f;
            costText.text = "";
        }
        else if (totalCost < 0)
        {
            costColor = Color.yellow;
            costText.text = "-" + Mathf.Abs(totalCost) + " RP";  
        }
        else
        {
            costColor = Color.green;
            costText.text = "+" + Mathf.Abs(totalCost) + " RP";  
        }  

        if (aug == null)
        {
            Debug.Log("Aug is null");
            return;
        }
        
        // Set button colors for selection state
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
            costText.color = Color.black + costColor / 1.7f;
        }

        if (isHovered) // Highlight when hovered
        {
            icon.color += highlightedColor/4;
            nameText.color += highlightedColor/4;
            costText.color += highlightedColor/4;
        }

        // Add level number to end of name
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
