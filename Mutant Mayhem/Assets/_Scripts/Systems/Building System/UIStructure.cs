using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UIStructure : MonoBehaviour, ISelectHandler
{
    public StructureSO structureSO;
    [SerializeField] Button button;
    [SerializeField] Image image;
    public GameObject textPrefab;
    [SerializeField] float textLockedAlpha = 0.3f;
    [HideInInspector] public GameObject textInstance;
    TextMeshProUGUI myText;
    [SerializeField] RectTransform myRectTransform;
    ScrollRectController scrollRectController;
    BuildingSystem buildingSystem;
    Player player;
    public int originalSiblingIndex;

    bool initialized;

    string cyanColorTag;
    string greenColorTag;
    string yellowColorTag;
    string redColorTag;
    string endColorTag = "</color>";

    void Awake()
    {
        cyanColorTag = "<color=#" + ColorUtility.ToHtmlStringRGB(Color.cyan) + ">";
        greenColorTag = "<color=#" + ColorUtility.ToHtmlStringRGB(Color.green) + ">";
        yellowColorTag = "<color=#" + ColorUtility.ToHtmlStringRGB(Color.yellow) + ">";
        redColorTag = "<color=#" + ColorUtility.ToHtmlStringRGB(Color.red) + ">";
    }

    void OnEnable()
    {
        if (initialized)
        {
            // If unlocked to player
            if (BuildingSystem._UnlockedStructuresDict[structureSO.structureType])
            {
                MakeInteractable(true);
            }
            else
            {
                button.interactable = false;
            }

            SetText(BuildingSystem.PlayerCredits);
        }
    }

    void Start()
    {
        // If unlocked to player
        if (BuildingSystem._UnlockedStructuresDict[structureSO.structureType])
        {
            MakeInteractable(true);
        }
        else
        {
            button.interactable = false;
        }

        SetText(BuildingSystem.PlayerCredits);
    }

    public void Initialize(BuildingSystem buildingSystem, ScrollRectController scrollRectController)
    {
        initialized = true;
        image.sprite = structureSO.uiImage;
        this.scrollRectController = scrollRectController;
        this.buildingSystem = buildingSystem;
        BuildingSystem.OnPlayerCreditsChanged += SetText;
        PowerManager.Instance.OnPowerChanged += OnPowerOrSupplyChanged;
        SupplyManager.OnSupplyBalanceChanged += OnPowerOrSupplyChanged;
        player = FindObjectOfType<Player>();
    }

    #region Select

    public void OnSelect(BaseEventData data)
    {
        TryToSelect(true);
    }

    public bool TryToSelect(bool SetMenuSelection)
    {
        if (!initialized)
            return false;

        if (!button.interactable)
            return false;

        // Lock the scroll rect to this selected object.        
        scrollRectController.SnapTo(myRectTransform);

        buildingSystem.ChangeStructureInHand(structureSO);
        //Debug.Log("OnSelect ran");

        button.Select();
        if (SetMenuSelection)
            BuildingSystem.Instance.buildMenuController.SetMenuSelection(structureSO);
        return true;
    }

    public void MakeInteractable(bool interactable)
    {
        button.interactable = interactable;

        SetText(BuildingSystem.PlayerCredits);
    }

    #endregion

    #region Text

    void OnPowerOrSupplyChanged(int balance)
    {
        SetText(BuildingSystem.PlayerCredits);
    }

    void SetText(float playerCredits)
    {
        if (textInstance == null)
            return;
        //Debug.Log("SetText ran in UiStructure");

        // Set text for locked/unlock

        bool unlocked = BuildingSystem._UnlockedStructuresDict[structureSO.structureType];

        if (myText == null)
            myText = textInstance.GetComponent<TextMeshProUGUI>();
        if (unlocked)
            myText.alpha = 1;
        else
            myText.alpha = textLockedAlpha;

        // Set structure info text
        if (structureSO.actionType != ActionType.Build)
        {
            myText.text = structureSO.tileName;
            return;
        }

        CreateTextStrings(BuildingSystem.PlayerCredits);
    }

    void CreateTextStrings(float playerCredits)
    {
        // If the button is not interactable, show unlock info only
        if (!button.interactable)
        {
            myText.text = $"{structureSO.tileName}\n{redColorTag}{structureSO.unlockText}{endColorTag}";
            return;
        }

        // Create string for power cost/gain
        string powerString = "";
        string powerCostColorTag;
        if (structureSO.powerCost > 0)
        {
            if (structureSO.powerCost <= PowerManager.Instance.powerBalance)
            {
                powerCostColorTag = yellowColorTag;
                powerString = $"{powerCostColorTag}<sprite=1>-{structureSO.powerCost}{endColorTag} ";
            }
            else
            {
                powerCostColorTag = redColorTag;
                powerString = $"{powerCostColorTag}<sprite=0>-{structureSO.powerCost}{endColorTag} ";
            }
        }
        else if (structureSO.powerCost < 0)
        {
            powerCostColorTag = greenColorTag;
            powerString = $"{powerCostColorTag}<sprite=1>+{Mathf.Abs(structureSO.powerCost)}{endColorTag} ";
        }

        // Create string for supply cost/gain
        string supplyString = "";
        string supplyCostColorTag;
        if (structureSO.supplyCost > 0)
        {
            if (structureSO.supplyCost <= SupplyManager.SupplyBalance)
                supplyCostColorTag = yellowColorTag;
            else
                supplyCostColorTag = redColorTag;

            supplyString = $"{supplyCostColorTag}<sprite=2>-{structureSO.supplyCost}{endColorTag}";
        }
        else if (structureSO.supplyCost < 0)
        {
            supplyCostColorTag = greenColorTag;
            supplyString = $"{supplyCostColorTag}<sprite=2>+{Mathf.Abs(structureSO.supplyCost)}{endColorTag}";
        }

        // Set yellow or red depending on affordability
        int totalCost = Mathf.FloorToInt(structureSO.tileCost * buildingSystem.structureCostMult);
        if (playerCredits >= totalCost)
        {
            myText.text = structureSO.tileName + "\n" +
            yellowColorTag + "$" + totalCost + " " + endColorTag + powerString + supplyString;
        }
        else
        {
            myText.text = structureSO.tileName + "\n" +
            redColorTag + "$" + totalCost + " " + endColorTag + powerString + supplyString;
        }
    }

    #endregion
}
