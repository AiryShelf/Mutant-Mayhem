using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIUpgrade : MonoBehaviour
{
    public UpgradeFamily upgradeFamily;
    public PlayerStatsUpgrade playerStatsUpgrade;
    public StructureStatsUpgrade structureStatsUpgrade;
    public ConsumablesUpgrade consumablesUpgrade;
    public GunStatsUpgrade gunStatsUpgrade;

    public Image buttonImage;
    [SerializeField] Color buttonImageHoverColor;
    Color buttonImageStartColor;
    public Button clickableAreaButton;
    public bool unlocked = true;

    [SerializeField] string UiName;
    [TextArea(3,10)]
    public string tooltipDescription;
    
    [SerializeField] bool showLevelsText;
    public GameObject upgradeTextPrefab;

    [Header("For Gun Upgrades:")]
    [SerializeField] int playerGunIndex;

    [HideInInspector] public TextMeshProUGUI upgradeText;
    UpgradeManager upgradeManager;
    Player player;
    string cyanColorTag;
    string greenColorTag;
    string yellowColorTag;
    string redColorTag;
    string endColorTag = "</color>";

    bool initialized;

    void OnEnable()
    {
        buttonImageStartColor = buttonImage.color;
        BuildingSystem.OnPlayerCreditsChanged += UpdateTextDelay;

        if (initialized)
            UpdateTextDelay(BuildingSystem.PlayerCredits);
    }

    void OnDisable()
    {
        BuildingSystem.OnPlayerCreditsChanged -= UpdateTextDelay;

        StopAllCoroutines();
    }

    public void Initialize()
    {
        //buttonImage = GetComponentInChildren<Image>();
        if (unlocked)
            buttonImage.enabled = true;
        else
            buttonImage.enabled = false;
            
        upgradeManager = UpgradeManager.Instance;
        player = FindObjectOfType<Player>();

        cyanColorTag = "<color=#" + ColorUtility.ToHtmlStringRGB(Color.cyan) + ">";
        greenColorTag = "<color=#" + ColorUtility.ToHtmlStringRGB(Color.green) + ">";
        yellowColorTag = "<color=#" + ColorUtility.ToHtmlStringRGB(Color.yellow) + ">";
        redColorTag = "<color=#" + ColorUtility.ToHtmlStringRGB(Color.red) + ">";

        initialized = true;
        if (gameObject.activeSelf)
        {
            UpdateText(BuildingSystem.PlayerCredits);
            //Debug.Log("Initialized UiUpgrade Text");
        }
    }

    // This allows the enum to be referenced via UI button OnClick
    public void InvokeOnClick(UIUpgrade myUpg)
    {  
        if (!buttonImage.enabled)
        {
            MessagePanel.PulseMessage("Upgrade/Consumable is locked!  Unlock the associated tech first", Color.yellow);
            return;
        }

        //Debug.Log("OnClick called");
        switch (upgradeFamily)
        {
            case UpgradeFamily.PlayerStats:
                upgradeManager.OnUpgradeButtonClicked(playerStatsUpgrade);
                break;
            case UpgradeFamily.StructureStats:
                upgradeManager.OnUpgradeButtonClicked(structureStatsUpgrade);
                break;
            case UpgradeFamily.Consumables:
                upgradeManager.OnUpgradeButtonClicked(consumablesUpgrade);
                break;
            case UpgradeFamily.GunStats:
                upgradeManager.OnUpgradeButtonClicked(gunStatsUpgrade, playerGunIndex);
                break;
            default:
                Debug.LogWarning("Unhandled upgrade family: " + upgradeFamily);
                break;
        }
        
        UpdateText(BuildingSystem.PlayerCredits);
    }

    void UpdateTextDelay(float playerCredits)
    {
        StartCoroutine(UpdateTextDelayed(playerCredits));
    }

    IEnumerator UpdateTextDelayed(float playerCredits)
    {
        if (!initialized)
            yield break;
        
        yield return new WaitForSeconds(0.2f);

        UpdateText(playerCredits);
    }

    public void Selected()
    {
        Debug.Log("UiUpgrade: Selected ran");

        buttonImage.color = buttonImageHoverColor;
    }

    #region Update Text

    public void UpdateText(float playerCredits)
    {
        if (upgradeText == null)
        {
            //Debug.LogError("UiUpgrade could not find it's upgradeText");
            return;
        }

        if (!buttonImage.enabled)
        {
            upgradeText.color = Color.grey;
        }
        else
        {
            upgradeText.color = Color.white;
        }

        int upgLvl = 1;
        int maxLvl = int.MaxValue;
        int upgCost = 1;
        string upgAmountString = "";
        string statValueString = "";
        
        // Check for families
        if (upgradeFamily == UpgradeFamily.Consumables)
        {
            // Consumables
            upgLvl = upgradeManager.consumablesUpgLevels[consumablesUpgrade];
            upgCost = UpgStatGetter.GetUpgCost(player, consumablesUpgrade, upgradeManager);
            statValueString = UpgStatGetter.GetStatValue(player, consumablesUpgrade);
            upgAmountString = UpgStatGetter.GetUpgAmount(player, consumablesUpgrade);
        }
        else if (upgradeFamily == UpgradeFamily.GunStats)
        {
            statValueString = UpgStatGetter.GetStatValue(player, gunStatsUpgrade, playerGunIndex);
            upgAmountString = UpgStatGetter.GetUpgAmount(player, gunStatsUpgrade, playerGunIndex, upgradeManager);

            // GunStats
            switch (playerGunIndex)
            {
                case 0:
                {
                    upgLvl = upgradeManager.laserUpgLevels[gunStatsUpgrade];
                    maxLvl = upgradeManager.laserUpgMaxLevels[gunStatsUpgrade];
                    upgCost = Mathf.FloorToInt(upgradeManager.gunStatsCostMult * 
                              upgradeManager.laserUpgCurrCosts[gunStatsUpgrade]);
                    break;
                }
                case 1:
                {
                    upgLvl = upgradeManager.bulletUpgLevels[gunStatsUpgrade];
                    maxLvl = upgradeManager.bulletUpgMaxLevels[gunStatsUpgrade];
                    upgCost = Mathf.FloorToInt(upgradeManager.gunStatsCostMult * 
                              upgradeManager.bulletUpgCurrCosts[gunStatsUpgrade]);
                    break;
                }
                case 4:
                {
                    upgLvl = upgradeManager.repairGunUpgLevels[gunStatsUpgrade];
                    maxLvl = upgradeManager.repairGunUpgMaxLevels[gunStatsUpgrade];
                    // No cost mult for Repair gun, yet
                    upgCost = upgradeManager.repairGunUpgCurrCosts[gunStatsUpgrade];
                    break;
                }
            }
            
            if (player.playerShooter.gunSights != null)
                player.playerShooter.gunSights.RefreshSettings();
        }
        else if (upgradeFamily == UpgradeFamily.PlayerStats)
        {
            // PlayerStats
            upgLvl = upgradeManager.playerStatsUpgLevels[playerStatsUpgrade];
            maxLvl = upgradeManager.playerStatsUpgMaxLevels[playerStatsUpgrade];
            upgCost = Mathf.FloorToInt(upgradeManager.playerStatsCostMult * 
                        upgradeManager.playerStatsUpgCurrCosts[playerStatsUpgrade]);
            statValueString = UpgStatGetter.GetStatValue(player, playerStatsUpgrade);
            upgAmountString = UpgStatGetter.GetUpgAmount(playerStatsUpgrade, upgradeManager);
        }
        else if (upgradeFamily == UpgradeFamily.StructureStats)
        {
            // StructureStats
            upgLvl = upgradeManager.structureStatsUpgLevels[structureStatsUpgrade];
            maxLvl = upgradeManager.structureStatsUpgMaxLevels[structureStatsUpgrade];
            upgCost = Mathf.FloorToInt(upgradeManager.structureStatsCostMult * 
                        upgradeManager.structureStatsUpgCurrCosts[structureStatsUpgrade]);
            statValueString = UpgStatGetter.GetStatValue(upgradeManager.player, structureStatsUpgrade);
            upgAmountString = UpgStatGetter.GetUpgAmount(structureStatsUpgrade);
        }
        
        // Change cost text color depending on affordability
        string costColorTag;
        if (playerCredits >= upgCost)
            costColorTag = yellowColorTag;
        else
            costColorTag = redColorTag;

        // Upgrade buttons text
        if (showLevelsText)
        {
            // Levels text
            if (upgLvl + 1 > maxLvl)
            {
                upgradeText.text = UiName + ": " + greenColorTag + statValueString + 
                                    "\n" + "Max level reached!" + endColorTag;
            }
            else
            {
                upgradeText.text = UiName + ": " + greenColorTag + statValueString + " " + cyanColorTag + upgAmountString + endColorTag + 
                                    "\nLvl " + (upgLvl + 1) + ": " + costColorTag + "$" + upgCost + endColorTag;
            }
        }
        else
        {
            // No levels text
            upgradeText.text = UiName + ": " + greenColorTag + statValueString + " " + cyanColorTag + upgAmountString + endColorTag +
                                "\n" + costColorTag + "$" + upgCost + endColorTag; 
        }

        //Debug.Log("Upgrade UI text updated");
    }

    #endregion
}
