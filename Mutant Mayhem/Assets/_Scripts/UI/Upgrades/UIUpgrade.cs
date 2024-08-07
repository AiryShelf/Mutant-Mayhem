using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIUpgrade : MonoBehaviour
{
    public UpgradeFamily upgradeFamily;
    public PlayerStatsUpgrade playerStatsUpgrade;
    public StructureStatsUpgrade structureStatsUpgrade;
    public ConsumablesUpgrade consumablesUpgrade;
    public GunStatsUpgrade gunStatsUpgrade;

    [SerializeField] string UiName;
    [TextArea(3,10)]
    public string tooltipDescription;
    
    [SerializeField] bool showLevelsText;
    public GameObject upgradeTextPrefab;

    [Header("For Gun Upgrades:")]
    [SerializeField] int playerGunIndex;


    [HideInInspector] public TextMeshProUGUI upgradeText;
    UpgradeManager upgradeSystem;
    Player player;
    string cyanColorTag;
    string greenColorTag;
    string yellowColorTag;
    string redColorTag;
    string endColorTag = "</color>";

    bool initialized;

    void Awake()
    {
        upgradeSystem = FindObjectOfType<UpgradeManager>();
        player = FindObjectOfType<Player>();

        cyanColorTag = "<color=#" + ColorUtility.ToHtmlStringRGB(Color.cyan) + ">";
        greenColorTag = "<color=#" + ColorUtility.ToHtmlStringRGB(Color.green) + ">";
        yellowColorTag = "<color=#" + ColorUtility.ToHtmlStringRGB(Color.yellow) + ">";
        redColorTag = "<color=#" + ColorUtility.ToHtmlStringRGB(Color.red) + ">";
    }

    void OnEnable()
    {
        if (initialized)
            StartCoroutine(UpdateText());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    public void Initialize()
    {
        initialized = true;
        if (gameObject.activeSelf)
            StartCoroutine(UpdateText());
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
            case UpgradeFamily.StructureStats:
                upgradeSystem.OnUpgradeButtonClicked(myUpg.structureStatsUpgrade);
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
        
        //UpdateText();
    }

    IEnumerator UpdateText()
    {
        //Debug.Log("Upgrade UI text updating");
        while (true)
        {
            int upgLvl = 1;
            int maxLvl = int.MaxValue;
            int upgCost = 1;
            string upgAmount = "";
            string statValue = "";
            
            // Check for families
            if (upgradeFamily == UpgradeFamily.Consumables)
            {
                // Consumables
                upgLvl = upgradeSystem.consumablesUpgLevels[consumablesUpgrade];
                upgCost = UpgStatGetter.GetUpgCost(player, consumablesUpgrade, upgradeSystem);
                statValue = UpgStatGetter.GetStatValue(player, consumablesUpgrade);
                upgAmount = UpgStatGetter.GetUpgAmount(player, consumablesUpgrade);
            }

            else if (upgradeFamily == UpgradeFamily.GunStats)
            {
                // GunStats
                switch (playerGunIndex)
                {
                    case 0:
                    {
                        upgLvl = upgradeSystem.laserUpgLevels[gunStatsUpgrade];
                        maxLvl = upgradeSystem.laserUpgMaxLevels[gunStatsUpgrade];
                        upgCost = upgradeSystem.laserUpgCurrCosts[gunStatsUpgrade];
                        break;
                    }

                    case 1:
                    {
                        upgLvl = upgradeSystem.bulletUpgLevels[gunStatsUpgrade];
                        maxLvl = upgradeSystem.bulletUpgMaxLevels[gunStatsUpgrade];
                        upgCost = upgradeSystem.bulletUpgCurrCosts[gunStatsUpgrade];
                        break;
                    }
                }
                statValue = UpgStatGetter.GetStatValue(player, gunStatsUpgrade, playerGunIndex);
                upgAmount = UpgStatGetter.GetUpgAmount(player, gunStatsUpgrade, playerGunIndex, upgradeSystem);
            }

            else if (upgradeFamily == UpgradeFamily.PlayerStats)
            {
                // PlayerStats
                upgLvl = upgradeSystem.playerStatsUpgLevels[playerStatsUpgrade];
                maxLvl = upgradeSystem.playerStatsUpgMaxLevels[playerStatsUpgrade];
                upgCost = upgradeSystem.playerStatsUpgCurrCosts[playerStatsUpgrade];
                statValue = UpgStatGetter.GetStatValue(player, playerStatsUpgrade);
                upgAmount = UpgStatGetter.GetUpgAmount(playerStatsUpgrade, upgradeSystem);
            }

            else if (upgradeFamily == UpgradeFamily.StructureStats)
            {
                // StructureStats
                upgLvl = upgradeSystem.structureStatsUpgLevels[structureStatsUpgrade];
                maxLvl = upgradeSystem.structureStatsUpgMaxLevels[structureStatsUpgrade];
                upgCost = upgradeSystem.structureStatsUpgCurrCosts[structureStatsUpgrade];
                statValue = UpgStatGetter.GetStatValue(upgradeSystem.player, structureStatsUpgrade);
                upgAmount = UpgStatGetter.GetUpgAmount(structureStatsUpgrade);
            }
            
            // Change text color depending on affordability
            string costColorTag;
            if (BuildingSystem.PlayerCredits >= upgCost)
                costColorTag = yellowColorTag;
            else
                costColorTag = redColorTag;
    
            // Upgrade buttons text
            if (showLevelsText)
            {
                // Levels text
                if (upgLvl + 1 > maxLvl)
                {
                    upgradeText.text = UiName + ": " + greenColorTag + statValue + endColorTag + 
                                    "\n" + greenColorTag + "Max level reached!" + endColorTag;
                }
                else
                {
                    upgradeText.text = UiName + ": " + greenColorTag + statValue + endColorTag + " " + cyanColorTag + upgAmount + endColorTag + 
                                    "\nLvl " + (upgLvl + 1) + ": " + costColorTag + "$" + upgCost + endColorTag;
                }
            }
            else
            {
                // No levels text
                upgradeText.text = UiName + ": " + greenColorTag + statValue + endColorTag + " " + cyanColorTag + upgAmount + endColorTag +
                                "\n" + costColorTag + "$" + upgCost + endColorTag; 
            }
            yield return new WaitForSeconds(0.2f);
        }
    }
}
