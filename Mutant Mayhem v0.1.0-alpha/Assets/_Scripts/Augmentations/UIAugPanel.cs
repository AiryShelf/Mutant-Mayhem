using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIAugPanel : MonoBehaviour
{
    [SerializeField] GameObject augmentationButtonPrefab;
    [SerializeField] Transform buttonContainer;
    [SerializeField] Transform selectedContainer;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI totalCostGainText;
    [SerializeField] TextMeshProUGUI levelValueText;
    [SerializeField] TextMeshProUGUI lowerLevelCostText;
    [SerializeField] TextMeshProUGUI raiseLevelCostText;
    [SerializeField] Button plusButton;
    [SerializeField] Button minusButton;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] TextMeshProUGUI researchPointsText;
    [SerializeField] TextMeshProUGUI AugsAddedText;
    [SerializeField] TextMeshProUGUI maxAugsText;
    [SerializeField] bool selectFirstAug = true;

    AugManager augManager;
    public UIAugmentation selectedUiAugmentation;
    public int augsAdded;

    void Start()
    {
        PopulateAugsList(AugManager.Instance.availableAugmentations);
    }

    public void PopulateAugsList(List<AugmentationBaseSO> augmentations)
    {
        augManager = AugManager.Instance;

        // Clear any children inside the buttonContainer
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        // Populate container
        bool firstSelected = false;
        foreach (var augmentation in augmentations)
        {
            GameObject button = Instantiate(augmentationButtonPrefab, buttonContainer);
            UIAugmentation uiAug = button.GetComponent<UIAugmentation>();
            uiAug.Setup(augmentation, augManager, this);

            // Select first element
            if (!selectFirstAug)
                return;

            if (!firstSelected)
            {
                firstSelected = true;
                selectedUiAugmentation = uiAug;
                EventSystem.current.SetSelectedGameObject(uiAug.gameObject);
                Debug.Log("Auto-selected an Aug");
            }

            //UpdatePanelTextandButtons();
        }
    }

    public void SelectAugmentation(UIAugmentation newSelection)
    {
        if (selectedUiAugmentation != newSelection)
        {
            if (selectedUiAugmentation != null)
            {
                selectedUiAugmentation.selected = false;
                selectedUiAugmentation.UpdateIconAndText();
            }
            selectedUiAugmentation = newSelection;
            selectedUiAugmentation.selected = true;
            selectedUiAugmentation.UpdateIconAndText();
        }
        UpdatePanelTextandButtons();
    }

    #region Update UI

    public void UpdatePanelTextandButtons()
    {
        if (selectedUiAugmentation == null)
            Debug.LogWarning("No Aug selected");

        // Get aug and level
        AugmentationBaseSO aug = selectedUiAugmentation.aug;
        int _totalCost = selectedUiAugmentation.totalCost;
        int level;
        if (augManager.selectedAugsWithLvls.ContainsKey(aug))
            level = augManager.selectedAugsWithLvls[aug];
        else
            level = 0;

        // Other info Text
        nameText.text = "Selected: " + aug.augmentationName;
        researchPointsText.text = "Research Points: " + augManager.currentResearchPoints;
        AugsAddedText.text = "Aug Levels: " + augManager.GetCurrentLevelCount();
        maxAugsText.text = "Max Levels: " + augManager.maxAugs;

        // Update TotalCost text
        string costOrGain;
        Color costTextColor;
        if (_totalCost <= 0)
        {
            costOrGain = "Current Cost: ";
            costTextColor = Color.yellow;
        }
        else 
        {
            costOrGain = "Current Gain: ";
            costTextColor = Color.green;
        }

        totalCostGainText.text = costOrGain + Mathf.Abs(_totalCost) + " RP";
        totalCostGainText.color = costTextColor;

        // Description Text
        string description = "Raise the level for benefits, or lower it to gain RP at a cost.  " + 
                             "Certain augs may not have negative levels";
        if (level > 0)
            description = aug.GetPositiveDescription(augManager, level);
        else if (level < 0)
            description = aug.GetNegativeDescription(augManager, level);

        descriptionText.text = "Description: " + description;

        // Level valueText, lvlcostText, and +/- buttons
        Color lvlTextColor;
        if (level > 0)
            lvlTextColor = Color.cyan;
        else if (level < 0)
            lvlTextColor = Color.red;
        else 
            lvlTextColor = Color.white;

        levelValueText.text = level.ToString();
        levelValueText.color = lvlTextColor;

        int raiseLvlCost = Mathf.Abs(GetLevelCost(aug, level + 1, true));
        int lowerLvlCost = Mathf.Abs(GetLevelCost(aug, level - 1, false));
        if (raiseLvlCost <= augManager.currentResearchPoints)
            costTextColor = Color.yellow;
        else
            costTextColor = Color.red;
        raiseLevelCostText.text = "-" + raiseLvlCost + "\n" + "RP";
        raiseLevelCostText.color = costTextColor;
        lowerLevelCostText.text = "+" + lowerLvlCost + "\n" + "RP";
        
        // Buttons interactability
        if (level < aug.maxLvl)
            plusButton.interactable = true;
        else
            plusButton.interactable = false;
        if (level > aug.minLvl)
            minusButton.interactable = true;
        else
            minusButton.interactable = false;
        
        UpdateUIAugs();
    }

    public void UpdateUIAugs()
    {
        foreach (Transform child in buttonContainer.transform)
        {
            if (child.TryGetComponent(out UIAugmentation aug))
                aug.UpdateIconAndText();
        }
    }

    #endregion

    #region Add/Remove Augs+Lvls

    public bool AddAug(int level)
    {
        if (selectedUiAugmentation == null)
        {
            return false;
        }

        bool added = selectedUiAugmentation.AddToSelectedAugs(level);
        UpdatePanelTextandButtons();
        UpdateUIAugs();

        return added;
    }

    public void RemoveAug()
    {
        if (selectedUiAugmentation == null)
        {
            return;
        }

        selectedUiAugmentation.RemoveFromSelectedAugs();
        UpdatePanelTextandButtons();
    }

    public void OnRemoveClicked()
    {
        // Handle maxAugs
        if (selectedUiAugmentation.aug is Aug_MaxAugs _maxAugs)
            augManager.maxAugs -= augsAdded;

        // Remove aug
        augManager.currentResearchPoints -= selectedUiAugmentation.totalCost;
        Debug.Log("Subtracted " + selectedUiAugmentation.totalCost + "from RP");
        selectedUiAugmentation.totalCost = 0;
        RemoveAug();
    }

    public void OnPlusLevelClicked()
    {
        if (selectedUiAugmentation == null)
        {
            Debug.LogWarning("No aug selected to add a level to");
            return;
        }

        // Find level, add or remove
        AugmentationBaseSO aug = selectedUiAugmentation.aug;
        int level;
        int currentLvlCount = augManager.GetCurrentLevelCount();
        if (augManager.selectedAugsWithLvls.ContainsKey(aug))
        {
            level = augManager.selectedAugsWithLvls[aug];
            if (level >= aug.maxLvl)
            {
                Debug.Log("Max Aug lvl already reached");
                MessagePanel.ShowMessage("Max Aug level already reached!", Color.red);
                return;
            }
            
            // Add level
            augManager.selectedAugsWithLvls[aug]++;
            level++;

            currentLvlCount = augManager.GetCurrentLevelCount();
            if (currentLvlCount > augManager.maxAugs)
            {
                // Revert if over max Augs
                augManager.selectedAugsWithLvls[aug]--;
                level--;
                Debug.Log("Max Aug levels already selected");
                MessagePanel.ShowMessage("Max Aug levels already selected!", Color.red);
                return;
            }
        }
        else if (currentLvlCount < augManager.maxAugs)
        {
            level = 1;
        }
        else 
        {
            Debug.Log("Max Augs already selected");
            MessagePanel.ShowMessage("Max Aug levels already selected!", Color.red);
            return;
        }

        // Get cost
        int levelCost = GetLevelCost(aug, level, true);

        if (augManager.currentResearchPoints >= levelCost)
        {
            // Add to selected Augs
            if (!augManager.selectedAugsWithLvls.ContainsKey(aug) && !AddAug(level))
            {
                Debug.LogError("Failed to add Aug to selection");
                return;
            }

            // Apply cost
            augManager.currentResearchPoints -= levelCost;
            selectedUiAugmentation.totalCost -= levelCost;

            // Handle maxAugs raised
            if (aug is Aug_MaxAugs _maxAugs)
            {
                augManager.maxAugs += _maxAugs.lvlAddIncrement;
                augsAdded += _maxAugs.lvlAddIncrement;
            }

            Debug.Log("Current Level raised to: " + level + ". Subtracted " + levelCost.ToString() + " from currentResearchPoints");
            Debug.Log("Selected UIAug's total cost is " + selectedUiAugmentation.totalCost.ToString());
            selectedUiAugmentation.UpdateIconAndText();
        }
        else
        {
            // Undo change to level
            Debug.Log("Not enough research points");
            MessagePanel.ShowMessage("Not enough research points!", Color.red);
            augManager.selectedAugsWithLvls[aug]--;
        }

        UpdatePanelTextandButtons();

        if (level == 0)
        {
            RemoveAug();
        }
    }

    public void OnMinusLevelClicked()
    {
        if (selectedUiAugmentation == null)
        {
            Debug.LogWarning("No aug selected to minus a level from");
            return;
        }

        // Find level, add or remove
        AugmentationBaseSO aug = selectedUiAugmentation.aug;
        int level;
        int currentLvlCount = augManager.GetCurrentLevelCount();
        if (augManager.selectedAugsWithLvls.ContainsKey(aug))
        {
            level = augManager.selectedAugsWithLvls[aug];
            if (level <= aug.minLvl)
            {
                Debug.Log("Min Aug lvl already reached");
                MessagePanel.ShowMessage("Max Aug level already reached!", Color.red);
                return;
            }

            // Subtract level
            augManager.selectedAugsWithLvls[aug]--;
            level--;

            currentLvlCount = augManager.GetCurrentLevelCount();
            if (currentLvlCount > augManager.maxAugs)
            {
                // Revert if over max Augs
                augManager.selectedAugsWithLvls[aug]++;
                level++;
                Debug.Log("Max Aug levels already selected");
                MessagePanel.ShowMessage("Max Aug levels already selected!", Color.red);
                return;
            }
        }
        else if (currentLvlCount < augManager.maxAugs)
        {
            level = -1;
        }
        else
        {
            Debug.Log("Max Augs already selected");
            MessagePanel.ShowMessage("Max Aug levels already selected!", Color.red);
            return;
        }

        // Get Refund
        int levelRefund = GetLevelCost(aug, level, false);
        
        // Add to selected augs
        if (!augManager.selectedAugsWithLvls.ContainsKey(aug) && !AddAug(level))
        {
            Debug.LogError("Failed to add Aug to selection");
            return;
        }
        
        // Apply Refund
        augManager.currentResearchPoints += levelRefund;
        selectedUiAugmentation.totalCost += levelRefund;

        // Handle maxAugs lowered
        if (aug is Aug_MaxAugs _maxAugs)
        {
            augManager.maxAugs -= _maxAugs.lvlAddIncrement;
            augsAdded -= _maxAugs.lvlAddIncrement;
        }

        Debug.Log("Current Level lowered to: " + level + ". Added " + levelRefund.ToString() + " to currentResearchPoints");
        Debug.Log("Selected UIAug's total cost is " + selectedUiAugmentation.totalCost.ToString());
        selectedUiAugmentation.UpdateIconAndText();

        UpdatePanelTextandButtons();

        if (level == 0)
        {
            RemoveAug();
        }
    }

    #endregion

    #region Tools

    int GetLevelCost(AugmentationBaseSO aug, int level, bool addingLevel)
    {
        if (addingLevel)
        {
            int levelCost;
            if (level > 0)
            {
                levelCost = Mathf.FloorToInt(aug.cost + (level - 1) * 
                        (aug.lvlCostIncrement * aug.lvlCostIncrementMult));
            }
            else if (level == 0)
            {
                levelCost = aug.refund;
            }
            else
            {
                levelCost = Mathf.FloorToInt(aug.refund + -(level) * 
                        (aug.lvlRefundIncrement * aug.lvlRefundIncrementMult));
            }

            return levelCost;
        }
        else
        {
            int levelRefund;
            if (level > 0)
            {
                levelRefund = Mathf.FloorToInt(aug.cost + level * 
                            (aug.lvlCostIncrement * aug.lvlCostIncrementMult));
            }
            else if (level == 0)
            {
                levelRefund = aug.cost;
            }
            else
            {
                levelRefund = Mathf.FloorToInt(aug.refund + -(level + 1) * 
                            (aug.lvlRefundIncrement * aug.lvlRefundIncrementMult));
            }

            return levelRefund;
        }

        
    }

    #endregion
}
