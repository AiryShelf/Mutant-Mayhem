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
    public GameObject textInstance;
    [SerializeField] RectTransform myRectTransform;
    ScrollRectController scrollRectController;
    BuildingSystem buildingSystemController;

    void Awake()
    {
        image.sprite = structureSO.uiImage;
        scrollRectController = GetComponentInParent<ScrollRectController>();
        buildingSystemController = FindObjectOfType<BuildingSystem>();
    }

    void Start()
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

    public void OnSelect(BaseEventData data)
    {
        // Lock the scroll rect to this selected object.
        scrollRectController.SnapTo(myRectTransform);
        buildingSystemController.SwitchTools(structureSO.structureType);
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
