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
    [SerializeField] RectTransform myRectTransform;
    ScrollRectController scrollRectController;
    BuildingSystem buildingSystem;
    Player player;

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
                MakeInteractable();
            }
            else
            {
                button.interactable = false;
            }
        }
    }

    void Start()
    {
        // If unlocked to player
        if (BuildingSystem._UnlockedStructuresDict[structureSO.structureType])
        {
            MakeInteractable();
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
        player = FindObjectOfType<Player>();
    }

    public void OnSelect(BaseEventData data)
    {
        if (!initialized)
            return;

        buildingSystem.StartCoroutine(buildingSystem.DelayUIReselect());
        // Lock the scroll rect to this selected object.
        scrollRectController.SnapTo(myRectTransform);

        if (!button.interactable)
            return;

        buildingSystem.ChangeStructureInHand(structureSO);
        //Debug.Log("OnSelect ran");

        if (structureSO.isTurret)
        {
            int turrets = TurretManager.Instance.currentNumTurrets;
            int maxTurrets = player.stats.structureStats.maxTurrets;

            if (turrets == maxTurrets)
                MessagePanel.PulseMessage(turrets + " of " + maxTurrets + " turrets built!  Unlock more in Structure Upgrades", Color.red);
            else if (maxTurrets == 0)
                MessagePanel.PulseMessage("You can't build any turrets yet!  Unlock Structure Upgrades first", Color.red);
            else
                MessagePanel.PulseMessage(turrets + " of " + maxTurrets + " turrets built", Color.cyan);
        }

        // Force selection
        //EventSystem.current.SetSelectedGameObject(this.gameObject);
    }
    
    public void MakeInteractable()
    {
        button.interactable = true;
        SetText(BuildingSystem.PlayerCredits);
    }

    void SetText(float playerCredits)
    {
        if (textInstance == null)
            return;
        //Debug.Log("SetText ran in UiStructure");

        // Set text for locked/unlock
        
        bool unlocked = BuildingSystem._UnlockedStructuresDict[structureSO.structureType];
        TextMeshProUGUI text = textInstance.GetComponent<TextMeshProUGUI>();
        if (unlocked)
            text.alpha = 1;
        else
            text.alpha = textLockedAlpha;
        
        // Set structure info text
        if (structureSO.actionType != ActionType.Build)
        {
            textInstance.GetComponent<TextMeshProUGUI>().text = structureSO.tileName;
            return;
        }
        
        int totalCost = Mathf.FloorToInt(structureSO.tileCost * buildingSystem.structureCostMult * 
                                         PlanetManager.Instance.currentPlanet.buildCostMultiplier);
        // Set yellow or red depending on affordability
        if (playerCredits >= totalCost)
        {
            textInstance.GetComponent<TextMeshProUGUI>().text = 
            structureSO.tileName + "\n" +
            yellowColorTag + "$" + totalCost + "\n" +
            greenColorTag + structureSO.maxHealth + " HP" + endColorTag;
        }
        else
        {
            textInstance.GetComponent<TextMeshProUGUI>().text = 
            structureSO.tileName + "\n" +
            redColorTag + "$" + totalCost + "\n" +
            greenColorTag + structureSO.maxHealth + " HP" + endColorTag;
        }
    }
}
