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

    void Awake()
    {
        image.sprite = structureSO.uiImage;
        scrollRectController = GetComponentInParent<ScrollRectController>();
        buildingSystem = FindObjectOfType<BuildingSystem>();

    }

    void OnEnable()
    {
        if (initialized)
        {
            // If unlocked to player
            if (BuildingSystem._StructsAvailDict[structureSO.structureType])
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
        if (BuildingSystem._StructsAvailDict[structureSO.structureType])
        {
            MakeInteractable();
        }
        else
        {
            button.interactable = false;
        }
    }

    public void OnSelect(BaseEventData data)
    {
        // Lock the scroll rect to this selected object.
        scrollRectController.SnapTo(myRectTransform);
        buildingSystem.SwitchTools(structureSO.structureType);
    }
    public void OnDeselect(BaseEventData data)
    {
        buildingSystem.structureInHand = buildingSystem.AllStructureSOs[0];
    }
    
    public void MakeInteractable()
    {
        button.interactable = true;

        if (structureSO.actionType == ActionType.Build)
        {
            textInstance.GetComponent<TextMeshProUGUI>().text = 
                structureSO.tileName + "\n" +
                "$" + structureSO.tileCost + "\n" +
                structureSO.maxHealth + " HP";
        }
        else 
        {
            textInstance.GetComponent<TextMeshProUGUI>().text =
                structureSO.tileName;
        }

    }
}
