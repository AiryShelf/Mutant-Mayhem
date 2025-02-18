using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIAugPanel : MonoBehaviour
{
    [SerializeField] GameObject augmentationButtonPrefab;
    [SerializeField] Transform buttonContainer;

    [Header("Details Section:")]
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI totalCostGainText;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] TextMeshProUGUI rpAvailableText;
    [SerializeField] TextMeshProUGUI AugsAddedText;
    [SerializeField] TextMeshProUGUI maxAugsText;

    [Header("Adjust Level Section:")]
    [SerializeField] Image levelPanel;
    [SerializeField] Color levelPanelSelectedColor;
    [SerializeField] Color levelPanelFlashColor;
    [SerializeField] TextMeshProUGUI levelValueText;
    [SerializeField] TextMeshProUGUI lowerLevelCostText;
    [SerializeField] TextMeshProUGUI raiseLevelCostText;
    [SerializeField] Button plusButton;
    [SerializeField] Button minusButton;
    [SerializeField] Button removeButton;
    Color levelPanelStartColor;
    
    [SerializeField] bool selectFirstAug = true;

    AugManager augManager;
    public UIAugmentation selectedUiAugmentation;
    int augLvlsAdded;

    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        augManager = AugManager.Instance;
        //augManager.RefreshCurrentRP();

        levelPanelStartColor = levelPanel.color;
        PopulateAugsList(AugManager.Instance.availableAugmentations);
        UpdateRPAndLevelInfo();
    }

    public void PopulateAugsList(List<AugmentationBaseSO> augmentations)
    {
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
                continue;

            if (!firstSelected)
            {
                firstSelected = true;
                selectedUiAugmentation = uiAug;
                Debug.Log("Auto-selected an Aug");
                EventSystem.current.SetSelectedGameObject(uiAug.gameObject);
            }
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

    void UpdateRPAndLevelInfo()
    {
        rpAvailableText.text = "Available RP: " + augManager.currentResearchPoints;
        AugsAddedText.text = "Aug Levels: " + augManager.GetCurrentLevelCount();
        maxAugsText.text = "Max Levels: " + augManager.maxAugs;
    }

    public void UpdatePanelTextandButtons()
    {
        UpdateRPAndLevelInfo();
        levelPanel.color = levelPanelStartColor;

        if (selectedUiAugmentation == null)
        {
            //Debug.LogWarning("No Aug selected on UI panel update");
            return;
        }

        levelPanel.color = levelPanelSelectedColor;

        // Get aug and level
        AugmentationBaseSO aug = selectedUiAugmentation.augBaseSO;
        int _totalCost = selectedUiAugmentation.totalCost;
        int level;
        if (AugManager.selectedAugsWithLvls.ContainsKey(aug))
            level = AugManager.selectedAugsWithLvls[aug];
        else
            level = 0;

        // Update name and TotalCost text
        if (aug == null)
        {
            nameText.text = "Select an aug above!";
            nameText.color = Color.yellow;
        }
        else
        {
            nameText.text = aug.augmentationName;
            nameText.color = Color.cyan;
        }
        string costOrGain;
        Color costTextColor;
        if (_totalCost < 0)
        {
            costOrGain = "Current Cost: ";
            costTextColor = Color.yellow;
        }
        else if (_totalCost > 0)
        {
            costOrGain = "Current Gain: ";
            costTextColor = Color.green;
        }
        else 
        {
            costOrGain = "";
            costTextColor = Color.white;
        }

        totalCostGainText.text = costOrGain + Mathf.Abs(_totalCost) + " RP";
        totalCostGainText.color = costTextColor;

        // Description Text
        string description = "";
        Color descriptionColor = Color.white;
        if (level == 0)
        {
            description = aug.GetNeutralDescription(augManager, level);
        }
        else if (level > 0)
        {
            descriptionColor = Color.cyan + (Color.white / 2);
            description = aug.GetPositiveDescription(augManager, level);
        }
        else if (level < 0)
        {
            descriptionColor = Color.red - (Color.white / 4f);
            description = aug.GetNegativeDescription(augManager, level);
        }

        descriptionText.text = "Description: " + description;
        descriptionText.color = descriptionColor;

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

        int raiseLvlCost = GetLevelCost(aug, level + 1, true);
        int lowerLvlCost = GetLevelCost(aug, level - 1, false);
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
        
        if (level != 0)
            removeButton.interactable = true;
        else
            removeButton.interactable = false;
        
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
        // Check RP available to remove aug
        if (augManager.currentResearchPoints - selectedUiAugmentation.totalCost < 0)
        {
            Debug.Log("Not enough RP to remove selected Aug");
            MessagePanel.PulseMessage("Can't remove Aug.  Not enough RP available!", Color.red);
            return;
        }

        // Handle maxAugs
        if (selectedUiAugmentation.augBaseSO is Aug_MaxAugs _maxAugs)
        {
            if (!AugManager.selectedAugsWithLvls.ContainsKey(selectedUiAugmentation.augBaseSO))
            {
                Debug.LogError("Did not find Max Augs in dictionary");
                return;
            }

            // Make sure removing maxAugs doesn't bring current count below max.
            int currentLvl = AugManager.selectedAugsWithLvls[selectedUiAugmentation.augBaseSO];
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
        AugmentationBaseSO aug = selectedUiAugmentation.augBaseSO;
        int level = 0;
        int currentLvlCount = augManager.GetCurrentLevelCount();
        if (AugManager.selectedAugsWithLvls.ContainsKey(aug))
        {
            level = AugManager.selectedAugsWithLvls[aug]; 
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
            
        EventSystem.current.SetSelectedGameObject(selectedUiAugmentation.gameObject);

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
        AugmentationBaseSO aug = selectedUiAugmentation.augBaseSO;
        int level = 0;
        int currentLvlCount = augManager.GetCurrentLevelCount();
        if (AugManager.selectedAugsWithLvls.ContainsKey(aug))
        {
            level = AugManager.selectedAugsWithLvls[aug];
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

        EventSystem.current.SetSelectedGameObject(selectedUiAugmentation.gameObject);

        selectedUiAugmentation.UpdateIconAndText();
        UpdatePanelTextandButtons();
    }

    #endregion

    #region Tools

    public void TrackRPCosts()
    {
        AugManager.selectedAugsTotalCosts.Clear();

        foreach (Transform trans in buttonContainer)
        {
            UIAugmentation uiAug = trans.gameObject.GetComponent<UIAugmentation>();
            if (uiAug == null)
            {
                Debug.LogError("AugPanel: Could not find UIAugmentation in buttonContainer");
                return;
            }

            AugmentationBaseSO aug = uiAug.augBaseSO;
            foreach (var kvp in AugManager.selectedAugsWithLvls)
            {
                if (kvp.Key.augmentationName == aug.augmentationName)
                    AugManager.selectedAugsTotalCosts.Add(aug, uiAug.totalCost);
            }
        }
    }

    public void RefreshRPCosts()
    {
        foreach (Transform trans in buttonContainer)
        {
            UIAugmentation uiAug = trans.gameObject.GetComponent<UIAugmentation>();
            if (uiAug == null)
            {
                Debug.LogError("AugPanel: Could not find UIAugmentation in buttonContainer");
                return;
            }

            AugmentationBaseSO aug = uiAug.augBaseSO;
            if (aug == null)
            {
                Debug.LogError("AugPanel: Could not find AugmentationBaseSO in UIAugmentation");
                return;
            }

            foreach (var kvp in AugManager.selectedAugsTotalCosts)
            {
                if (kvp.Key.augmentationName == aug.augmentationName)
                {
                    uiAug.totalCost = kvp.Value;
                }
            }
        }
    }

    int GetLevelCost(AugmentationBaseSO aug, int level, bool addingLevel)
    {
        if (addingLevel)
        {
            int levelCost;
            if (level > 0)
            {
                // Progressive cost increase formula for adding levels
                levelCost = Mathf.FloorToInt(
                    aug.cost + Mathf.Pow((level - 1) * aug.lvlCostIncrement, aug.lvlCostIncrementPower)
                );
            }
            else if (level == 0)
            {
                // Base refund when no levels are added
                levelCost = aug.refund;
            }
            else
            {
                // Refund calculation for negative levels
                levelCost = Mathf.FloorToInt(
                    aug.refund + Mathf.Pow(-level * aug.lvlRefundIncrement, aug.lvlRefundIncrementPower)
                );
            }

            return levelCost;
        }
        else
        {
            int levelRefund;
            if (level > 0)
            {
                // Refund calculation for positive levels
                levelRefund = Mathf.FloorToInt(
                    aug.cost + Mathf.Pow(level * aug.lvlCostIncrement, aug.lvlCostIncrementPower)
                );
            }
            else if (level == 0)
            {
                // Refund when level is at the base
                levelRefund = aug.cost;
            }
            else
            {
                // Progressive refund logic for negative levels
                levelRefund = Mathf.FloorToInt(
                    aug.refund + Mathf.Pow(-(level + 1) * aug.lvlRefundIncrement, aug.lvlRefundIncrementPower)
                );
            }

            return levelRefund;
        }
    }

    public void FlashLevelPanel()
    {
        GameTools.StartCoroutine(GameTools.FlashImage(levelPanel, 0.4f, 0.2f, levelPanelFlashColor, levelPanelSelectedColor));
    }

    #endregion
}
