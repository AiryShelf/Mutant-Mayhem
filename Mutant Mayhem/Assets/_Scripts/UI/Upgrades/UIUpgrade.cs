using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIUpgrade : MonoBehaviour
{
    public UpgradeFamily upgradeFamily;
    public PlayerStatsUpgrade playerStatsUpgrade;
    public QCubeStatsUpgrade qCubeStatsUpgrade;
    public ConsumablesUpgrade consumablesUpgrade;
    public GunStatsUpgrade gunStatsUpgrade;

    [SerializeField] string UiName;
    [TextArea(3,10)]
    public string tooltipDescription;
    
    [SerializeField] bool showLevelsText;
    public GameObject upgradeTextPrefab;


    [Header("For Gun Upgrades:")]
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
        switch (myUpg.upgradeFamily)
        {
            case UpgradeFamily.PlayerStats:
                upgradeSystem.OnUpgradeButtonClicked(myUpg.playerStatsUpgrade);
                break;
            case UpgradeFamily.QCubeStats:
                upgradeSystem.OnUpgradeButtonClicked(myUpg.qCubeStatsUpgrade);
                break;
            case UpgradeFamily.Consumables:
                upgradeSystem.OnUpgradeButtonClicked(myUpg.consumablesUpgrade);
                break;
            case UpgradeFamily.GunStats:
                upgradeSystem.OnUpgradeButtonClicked(myUpg.gunStatsUpgrade, myUpg.playerGunIndex);
                break;
            default:
                Debug.LogWarning("Unhandled upgrade family: " + myUpg.upgradeFamily);
                break;
        }
        
        UpdateText();
    }

    void UpdateText()
    {

        int upgLvl = 1;
        int upgCost = 1;
        
        // Check for families
        if (upgradeFamily == UpgradeFamily.Consumables)
        {
            // Consumables
            upgLvl = upgradeSystem.consumablesUpgLevels[consumablesUpgrade];
            upgCost = upgradeSystem.consumablesUpgCurrCosts[consumablesUpgrade];
        }
        else if (upgradeFamily == UpgradeFamily.GunStats)
        {
            // GunStats
            switch (playerGunIndex)
            {
                case 0:
                {
                    upgLvl = upgradeSystem.laserPistolUpgLevels[gunStatsUpgrade];
                    upgCost = upgradeSystem.laserPistolUpgCurrCosts[gunStatsUpgrade];
                    break;
                }

                case 1:
                {
                    upgLvl = upgradeSystem.SMGUpgLevels[gunStatsUpgrade];
                    upgCost = upgradeSystem.SMGUpgCurrCosts[gunStatsUpgrade];
                    break;
                }
            }
        }
        else if (upgradeFamily == UpgradeFamily.PlayerStats)
        {
            // PlayerStats
            upgLvl = upgradeSystem.playerStatsUpgLevels[playerStatsUpgrade];
            upgCost = upgradeSystem.playerStatsUpgCurrCosts[playerStatsUpgrade];
        }
        else if (upgradeFamily == UpgradeFamily.QCubeStats)
        {
            // QCubeStats
            upgLvl = upgradeSystem.qCubeStatsUpgLevels[qCubeStatsUpgrade];
            upgCost = upgradeSystem.qCubeStatsUpgCurrCosts[qCubeStatsUpgrade];
        }
        

        if (showLevelsText)
        {
            upgradeText.text = 
                UiName + " Lvl " + (upgLvl + 1) + "\n" + "$" + upgCost;  
        }
        else
        {
            upgradeText.text = 
                UiName + "\n" + "$" + upgCost; 
        }     
    }
}
