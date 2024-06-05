using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIUpgrade : MonoBehaviour
{
    public UpgradeType type;
    public string UiName;
    [TextArea(3,10)]
    public string tooltipDescription;
    
    public GameObject upgradeTextPrefab;

    [Header("For Gun Upgrades:")]
    public bool isGunUpg;
    public GunType gunType;

    [HideInInspector] public GameObject upgradeTextInstance;
    TextMeshProUGUI upgradeText;
    UpgradeSystem upgradeSystem;

    void Start()
    {
        upgradeSystem = FindObjectOfType<UpgradeSystem>();
        upgradeText = upgradeTextInstance.GetComponent<TextMeshProUGUI>();
        UpdateText();
    }

    // This allows the enum to be referenced via UI button OnClick
    public void InvokeOnClick(UIUpgrade u)
    {
        //Debug.Log("OnClick called");

        upgradeSystem.OnUpgradeButtonClicked(u.type, isGunUpg, 0);
        
        UpdateText();
    }

    void UpdateText()
    {
        int upgLvl;
        int upgCost;

        if (isGunUpg)
        {
            upgLvl = upgradeSystem.playerStatsUpgLevels[type];
            upgCost = upgradeSystem.playerStatsUpgCurrCosts[type];
        }
        else
        {
            upgLvl = upgradeSystem.playerStatsUpgLevels[type];
            upgCost = upgradeSystem.playerStatsUpgCurrCosts[type];
        }

        upgradeText.text = 
            UiName + " Lvl " + (upgLvl + 1) + "\n" + "$" + upgCost;       
    }
}
