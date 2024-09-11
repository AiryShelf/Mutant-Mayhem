using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanelBuilder : MonoBehaviour
{
    [SerializeField] List<GameObject> UIUpgradePrefabs;
    [SerializeField] GridLayoutGroup buttonsGrid;
    [SerializeField] GridLayoutGroup textGrid;
    [SerializeField] FadeCanvasGroupsWave fadeCanvasGroups;

    [Header("Unlockables (Optional)")]
    [SerializeField] GameObject unlockPanel;
    [SerializeField] TextMeshProUGUI unlockCostText;
    [SerializeField] int unlockCost;
    [SerializeField] string techUnlockMessageName;
    [Header("Gun")]
    [SerializeField] UpgradeFamily upgradeFamily;
    [SerializeField] int playerGunIndex;
    [SerializeField] Image toolbarImage;
    [Header("Structure")]
    [SerializeField] List<Structure> structuresToUnlock;
    [SerializeField] bool addTurretSlot;

    bool unlocked = false;
    Player player;
    UpgradeManager upgradeManager;
    BuildingSystem buildingSystem;

    void Awake()
    {
        player = FindObjectOfType<Player>();
        upgradeManager = FindObjectOfType<UpgradeManager>();
        buildingSystem = FindObjectOfType<BuildingSystem>();
        BuildingSystem.OnPlayerCreditsChanged += UpdateUnlockCostTextColor;

        // Clear editor objects in layout groups
        for (int i = buttonsGrid.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(buttonsGrid.transform.GetChild(i).gameObject);
        }
        for (int i = textGrid.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(textGrid.transform.GetChild(i).gameObject);
        } 

        // Clear unlock button
        if (unlockPanel)
            unlockPanel.gameObject.SetActive(false);

        // Check if unlockable
        if (unlockPanel != null)
        {
            ShowUnlockButton();
            UpdateUnlockCostTextColor(BuildingSystem.PlayerCredits);
            return;
        }

        // Initialize panel
        Initialize();
        StartCoroutine(InitializeFadeGroups());
    }

    void UpdateUnlockCostTextColor(float playerCredits)
    {
        if (unlockCostText == null)
            return;

        // Update unlock cost text color as per afforadability
        // Could be handled by new Event OnPlayerCreditsChanged
        if (!unlocked && unlockPanel != null)
        {
            Color color;
            if (playerCredits >= unlockCost)
                color = Color.yellow;
            else
                color = Color.red;

            unlockCostText.text = "$" + unlockCost.ToString();
            unlockCostText.color = color;
        }
    }

    void Initialize()
    {
        // Initialize upgrade lists into UI
        foreach (GameObject upgrade in UIUpgradePrefabs)
        {
            // Create button, get text prefab
            GameObject buttonPrefab = Instantiate(upgrade, buttonsGrid.transform);
            UIUpgrade uIUpgrade = buttonPrefab.GetComponent<UIUpgrade>();
            GameObject textPrefab = uIUpgrade.upgradeTextPrefab;

            // Create text obj and give text instances to uIUpgrade
            GameObject txtObj = Instantiate(textPrefab, textGrid.transform);
            StartCoroutine(SetTextReference(uIUpgrade, txtObj));

            //uIUpgrade.Initialize();

            // Add to fade canvas groups
            fadeCanvasGroups.individualElements.Add(buttonPrefab.GetComponent<CanvasGroup>());
            fadeCanvasGroups.individualElements.Add(txtObj.GetComponent<CanvasGroup>());
        } 
    }

    IEnumerator SetTextReference(UIUpgrade upg, GameObject obj)
    {
        yield return new WaitForFixedUpdate();
        upg.upgradeText = obj.GetComponent<TextMeshProUGUI>();
        upg.Initialize();
    }

    IEnumerator InitializeFadeGroups()
    {
        yield return new WaitForFixedUpdate();
        fadeCanvasGroups.Initialize();
    }

    void ShowUnlockButton()
    {
        unlockPanel.gameObject.SetActive(true);
    }

    public void OnUnlockClick()
    {
        if (BuildingSystem.PlayerCredits >= unlockCost && !unlocked)
        {
            BuildingSystem.PlayerCredits -= unlockCost;
            unlocked = true;

            // Unlock gun
            if (upgradeFamily == UpgradeFamily.GunStats)
            {
                player.playerShooter.gunsUnlocked[playerGunIndex] = true;
                toolbarImage.color = new Color(1,1,1,1);
                upgradeManager.PlayUpgradeEffectAt(Camera.main.ScreenToWorldPoint(toolbarImage.transform.position));
            }

            // Unlock Structures, add turret
            buildingSystem.UnlockStructures(structuresToUnlock);
            if (addTurretSlot)
                player.stats.structureStats.maxTurrets++;

            // Initialize and open upgrades panel
            Initialize();
            //StartCoroutine(DelayTrigger());
            Destroy(unlockPanel.gameObject);

            upgradeManager.PlayUpgradeButtonEffect();

            MessagePanel.ShowMessage(techUnlockMessageName + " unlocked!", Color.green);
        }
        else
        {
            MessagePanel.ShowMessage("Not enough Credits!", Color.red);
        }
    }

    IEnumerator DelayTrigger()
    {
        yield return new WaitForSeconds(1f);
        fadeCanvasGroups.isTriggered = true;
    }

    bool IsGunUnlocked()
    {
        bool isGunUnlocked = false;
        if (player.playerShooter.gunsUnlocked[playerGunIndex])
        {
            isGunUnlocked = true;
        }

        return isGunUnlocked;
    }

}
