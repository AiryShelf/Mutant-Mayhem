using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIUpgrade : MonoBehaviour
{
    public UpgradeType type;
    [SerializeField] string UiName;
    [TextArea(3,10)]
    public string tooltipDescription;
    
    public GameObject upgradeTextPrefab;

    [Header("For Gun Upgrades:")]
    [SerializeField] bool isGunUpg;
    [SerializeField] int playerGunIndex;

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
    public void InvokeOnClick(UIUpgrade myUpg)
    {
        //Debug.Log("OnClick called");

        upgradeSystem.OnUpgradeButtonClicked(myUpg.type, isGunUpg, playerGunIndex);
        
        UpdateText();
    }

    void UpdateText()
    {

        int upgLvl = 1;
        int upgCost = 1;

        // Check for Gun Stats
        if (isGunUpg)
        {
            switch (playerGunIndex)
            {
                case 0:
                {
                    upgLvl = upgradeSystem.laserPistolUpgLevels[type];
                    upgCost = upgradeSystem.laserPistolUpgCurrCosts[type];
                    break;
                }

                case 1:
                {
                    upgLvl = upgradeSystem.SMGUpgLevels[type];
                    upgCost = upgradeSystem.SMGUpgCurrCosts[type];
                    break;
                }
            }
        }
        else
        {
            // PlayerStats
            upgLvl = upgradeSystem.playerStatsUpgLevels[type];
            upgCost = upgradeSystem.playerStatsUpgCurrCosts[type];
        }

        upgradeText.text = 
            UiName + " Lvl " + (upgLvl + 1) + "\n" + "$" + upgCost;       
    }
}
