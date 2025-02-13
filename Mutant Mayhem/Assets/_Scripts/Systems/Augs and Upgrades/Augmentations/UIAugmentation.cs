using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
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
    public AugmentationBaseSO augBaseSO;
    public int totalCost;

    AugManager augManager;
    UIAugPanel augPanel;
    Button myButton;
    public bool selected;
    bool isHovered;

    void Awake()
    {
        myButton = GetComponent<Button>();
    }

    public void Setup(AugmentationBaseSO augmentation, AugManager manager, UIAugPanel panel)
    {
        originalIconColor = icon.color;
        originalTextColor = nameText.color;

        augBaseSO = augmentation;
        augManager = manager;
        augPanel = panel;
        icon.sprite = augmentation.uiImage;
        nameText.text = augmentation.augmentationName;

        UpdateIconAndText();
    }

    public void OnSelect(BaseEventData data)
    {
        augPanel.SelectAugmentation(this);
        augPanel.FlashLevelPanel();
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
        if (augManager.selectedAugsWithLvls.ContainsKey(augBaseSO))
        {
            Debug.LogError("UIAugmentation already exists in selected Augs, can't add");
            return false;
        } 

        // Check for duplicates
        if (augManager.selectedAugsWithLvls.ContainsKey(augBaseSO))
        {
            Debug.LogError("UIAugmentation already exists in selected Augs, can't add");
            return false;
        }

        augManager.selectedAugsWithLvls.Add(augBaseSO, level);
        Debug.Log("Added Augmentation: " + augBaseSO.augmentationName);

        UpdateIconAndText();
        return true;
    }

    public void RemoveFromSelectedAugs()
    {
        if (!augManager.selectedAugsWithLvls.ContainsKey(augBaseSO))
        {
            Debug.LogError("UIAugmentation not found in selected Augs, can't remove");
            return;
        }

        augManager.selectedAugsWithLvls.Remove(augBaseSO);
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

        if (augBaseSO == null)
        {
            Debug.Log("Aug is null");
            return;
        }
        
        // Set button colors for selection state
        if (augManager.selectedAugsWithLvls.ContainsKey(augBaseSO))
        {
            int level = augManager.selectedAugsWithLvls[augBaseSO];
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
            icon.color += highlightedColor/2;
            nameText.color += highlightedColor/2;
            costText.color += highlightedColor/2;
        }

        // Add level number to end of name
        if (augManager.selectedAugsWithLvls.ContainsKey(augBaseSO))
        {
            nameText.text = augBaseSO.augmentationName + " " + augManager.selectedAugsWithLvls[augBaseSO];
        }
        else
        {
            nameText.text = augBaseSO.augmentationName;
        }
    }
}
