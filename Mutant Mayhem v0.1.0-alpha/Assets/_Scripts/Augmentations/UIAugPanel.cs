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
    public int augLvlsAdded;

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
        foreach (AugmentationBaseSO augmentation in augmentations)
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
                Debug.Log("Auto-selected an Aug");
                EventSystem.current.SetSelectedGameObject(uiAug.gameObject);
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
        string description = "";
        if (level == 0)
            description = aug.GetNeutralDescription(augManager, level);
        else if (level > 0)
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
        {
            if (!augManager.selectedAugsWithLvls.ContainsKey(selectedUiAugmentation.aug))
            {
                Debug.LogAssertion("Did not find Max Augs in dictionary");
                return;
            }

            // Make sure removing maxAugs doesnt bring current count below max.
            int currentLvl = augManager.selectedAugsWithLvls[selectedUiAugmentation.aug];
            if (augManager.GetCurrentLevelCount() - currentLvl <= augManager.maxAugs - augLvlsAdded)
            {
                augManager.maxAugs -= augLvlsAdded;
                augLvlsAdded = 0;
            }
            else
            {
                Debug.Log("Could not remove MaxAugs.  Too many Aug levels selected");
                MessagePanel.PulseMessage("Can't remove Max Augs.  Remove levels from other Augs first", Color.red);
                return;
            }
        }

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

        // Find level, do checks
        AugmentationBaseSO aug = selectedUiAugmentation.aug;
        int level = 0;
        int currentLvlCount = augManager.GetCurrentLevelCount();
        if (augManager.selectedAugsWithLvls.ContainsKey(aug))
        {
            level = augManager.selectedAugsWithLvls[aug]; 
        }

        if (level >= aug.maxLvl)
        {
            Debug.Log("Max Aug lvl already reached");
            MessagePanel.PulseMessage("Max Aug level already reached!", Color.red);
            return;
        }

        if (level >= 0 && currentLvlCount >= augManager.maxAugs && !(aug is Aug_MaxAugs))
        {
            Debug.Log("Max Aug levels already selected");
            MessagePanel.PulseMessage("Max Aug levels already selected!", Color.red);
            return;
        }

        int nextLevelCost = GetLevelCost(aug, level + 1, true);
        if (augManager.currentResearchPoints < nextLevelCost)
        {
            Debug.Log("Not enough research points");
            MessagePanel.PulseMessage("Not enough research points!", Color.red);
            return;
        }

        // Add level
        augManager.IncrementLevel(aug, true);
        level++;

        // Apply cost
        augManager.currentResearchPoints -= nextLevelCost;
        selectedUiAugmentation.totalCost -= nextLevelCost;

        // Handle maxAugs raised
        if (aug is Aug_MaxAugs _maxAugs)
        {
            if (level <= 0)
            {
                augManager.maxAugs += _maxAugs.lvlNegIncrement;
                augLvlsAdded += _maxAugs.lvlNegIncrement;
            }
            else
            {
                augManager.maxAugs += _maxAugs.lvlAddIncrement;
                augLvlsAdded += _maxAugs.lvlAddIncrement;
            }
        }

        Debug.Log("Current Level raised to: " + level + ". Subtracted " + nextLevelCost + " from currentResearchPoints");
        Debug.Log("Selected UIAug's total cost is " + selectedUiAugmentation.totalCost);
            

        selectedUiAugmentation.UpdateIconAndText();
        UpdatePanelTextandButtons();
    }

    public void OnMinusLevelClicked()
    {
        if (selectedUiAugmentation == null)
        {
            Debug.LogWarning("No aug selected to minus a level from");
            return;
        }

        // Find level, do checks
        AugmentationBaseSO aug = selectedUiAugmentation.aug;
        int level = 0;
        int currentLvlCount = augManager.GetCurrentLevelCount();
        if (augManager.selectedAugsWithLvls.ContainsKey(aug))
        {
            level = augManager.selectedAugsWithLvls[aug];
        }

        if (level <= aug.minLvl)
        {
            Debug.Log("Min Aug lvl already reached");
            MessagePanel.PulseMessage(aug.augmentationName + " is maxed out!", Color.red);
            return;
        }
        
        if (level <= 0 && currentLvlCount + 1 > augManager.maxAugs && !(aug is Aug_MaxAugs))
        {
            Debug.Log("Max Aug levels already selected");
            MessagePanel.PulseMessage("Max Aug levels already selected!", Color.red);
            return;
        }

        // Handle Max Augs lowered
        if (aug is Aug_MaxAugs _maxAugs)
        {
            int maxAugLevelsInc;
            int levelInc;
            if (level > 0)
            {
                maxAugLevelsInc = _maxAugs.lvlAddIncrement;
                levelInc = -1;
            }
            else
            {
                maxAugLevelsInc = _maxAugs.lvlNegIncrement;
                levelInc = 1;
            }

            if (augManager.GetCurrentLevelCount() + levelInc <= augManager.maxAugs - maxAugLevelsInc)
            {
                augManager.maxAugs -= maxAugLevelsInc;
                augLvlsAdded -= maxAugLevelsInc;
            }
            else
            {
                Debug.Log("Can't lower MaxAugs without going over the level limit");
                MessagePanel.PulseMessage("Can't lower " + _maxAugs.augmentationName + ".  Remove levels from other Augs first", Color.red);
                return;
            }
        }

        // Subtract level
        augManager.IncrementLevel(aug, false);
        level--;
        
        // Apply Refund
        int levelRefund = GetLevelCost(aug, level, false);
        augManager.currentResearchPoints += levelRefund;
        selectedUiAugmentation.totalCost += levelRefund;

        Debug.Log("Current Level lowered to: " + level + ". Added " + levelRefund.ToString() + " to currentResearchPoints");
        Debug.Log("Selected UIAug's total cost is " + selectedUiAugmentation.totalCost.ToString());
        selectedUiAugmentation.UpdateIconAndText();

        UpdatePanelTextandButtons();
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
