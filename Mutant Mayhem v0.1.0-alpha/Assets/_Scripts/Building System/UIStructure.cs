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
    [HideInInspector] public GameObject textInstance;
    [SerializeField] RectTransform myRectTransform;
    ScrollRectController scrollRectController;
    BuildingSystem buildingSystem;

    bool initialized;

    string cyanColorTag;
    string greenColorTag;
    string yellowColorTag;
    string redColorTag;
    string endColorTag = "</color>";

    void Awake()
    {
        image.sprite = structureSO.uiImage;
        scrollRectController = GetComponentInParent<ScrollRectController>();
        buildingSystem = FindObjectOfType<BuildingSystem>();
        BuildingSystem.OnPlayerCreditsChanged += SetText;

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
        initialized = true;
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

    public void OnSelect(BaseEventData data)
    {
        // Lock the scroll rect to this selected object.
        scrollRectController.SnapTo(myRectTransform);
        buildingSystem.ChangeStructureInHand(structureSO);
    }
    public void OnDeselect(BaseEventData data)
    {
        //buildingSystem.structureInHand = buildingSystem.AllStructureSOs[0];
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
        Debug.Log("SetText ran in UiStructure");
        // Set structure info text
        if (structureSO.actionType != ActionType.Build)
        {
            textInstance.GetComponent<TextMeshProUGUI>().text = structureSO.tileName;
            return;
        }

        // Set yellow or red depending on affordability
        if (playerCredits >= structureSO.tileCost)
        {
            textInstance.GetComponent<TextMeshProUGUI>().text = 
            structureSO.tileName + "\n" +
            yellowColorTag + "$" + structureSO.tileCost + "\n" +
            greenColorTag + structureSO.maxHealth + " HP" + endColorTag;
        }
        else
        {
            textInstance.GetComponent<TextMeshProUGUI>().text = 
            structureSO.tileName + "\n" +
            redColorTag + "$" + structureSO.tileCost + "\n" +
            greenColorTag + structureSO.maxHealth + " HP" + endColorTag;
        }
    }
}
