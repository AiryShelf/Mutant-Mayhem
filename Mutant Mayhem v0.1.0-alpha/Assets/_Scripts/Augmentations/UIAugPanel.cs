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
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] TextMeshProUGUI researchPointsText;
    [SerializeField] TextMeshProUGUI selectedCountText;
    [SerializeField] TextMeshProUGUI maxCountText;
    [SerializeField] bool selectFirstAug = true;

    AugManager augManager;
    UIAugmentation selectedAugmentation;

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
                selectedAugmentation = uiAug;
                EventSystem.current.SetSelectedGameObject(uiAug.gameObject);
                Debug.Log("Auto-selected an Aug");
            }
        }

        UpdatePanelText();
    }

    public void SelectAugmentation(UIAugmentation newSelection)
    {
        selectedAugmentation = newSelection;
        UpdatePanelText();
    }

    public void UpdatePanelText()
    {
        if (selectedAugmentation == null)
            Debug.LogWarning("No Aug selected");

        Color costColor;
        if (ProfileManager.Instance.currentProfile.researchPoints < selectedAugmentation.aug.cost)
            costColor = Color.red;
        else
            costColor = Color.green;

        nameText.text = "Selected: " + selectedAugmentation.aug.augmentationName;
        costText.text = "Cost: " + selectedAugmentation.aug.cost;
        costText.color = costColor;
        descriptionText.text = "Description: " + selectedAugmentation.aug.description;
        researchPointsText.text = "Research Points: " + augManager.currentResearchPoints;
        selectedCountText.text = "Selected: " + augManager.selectedAugsWithLvls.Count;
        maxCountText.text = "Max Augs: " + augManager.maxAugs;
    }

    public void OnAddClicked()
    {
        if (selectedAugmentation != null)
        {
            selectedAugmentation.AddToSelectedAugs();
        }

        UpdatePanelText();
        UpdateUIAugs();
    }

    public void OnRemoveClicked()
    {
        if (selectedAugmentation != null)
        {
            selectedAugmentation.RemoveFromSelectedAugs();
        }

        UpdatePanelText();
        UpdateUIAugs();
    }

    public void UpdateUIAugs()
    {
        foreach (GameObject child in buttonContainer)
        {
            UIAugmentation aug = child.GetComponent<UIAugmentation>();
            aug.UpdateIconAndText();
        }
    }
}
