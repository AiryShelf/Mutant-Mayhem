using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiUpgradePanel : UI_PanelBase
{
    [SerializeField] List<GameObject> UIUpgradePrefabs;
    [SerializeField] GridLayoutGroup buttonsGrid;
    [SerializeField] GridLayoutGroup textGrid;
    public bool unlocked = false;

    [Header("Unlockables (Optional)")]
    [SerializeField] GameObject unlockPanel;
    [SerializeField] TextMeshProUGUI unlockCostText;
    [SerializeField] int unlockCost;
    [SerializeField] string techUnlockMessageName;

    [Header("Unlock Gun")]
    [SerializeField] UpgradeFamily upgradeFamily;
    [SerializeField] int playerGunIndex;

    [Header("Unlock Structure")]
    [SerializeField] List<Structure> structuresToUnlock;
    [SerializeField] bool addTurretSlot;

    Player player;
    BuildingSystem buildingSystem;

    void Awake()
    {
        player = FindObjectOfType<Player>();
        buildingSystem = FindObjectOfType<BuildingSystem>();

        // Clear editor objects in layout groups
        for (int i = buttonsGrid.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(buttonsGrid.transform.GetChild(i).gameObject);
        }
        for (int i = textGrid.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(textGrid.transform.GetChild(i).gameObject);
        } 
    }

    void Start()
    {
        // Show unlock panel
        if (unlockPanel != null)
        {
            //unlockPanel.gameObject.SetActive(false);
            unlockPanel.gameObject.SetActive(true);
            UpdateUnlockCostTextColor(BuildingSystem.PlayerCredits);
            return;
        }

        // Initialize upgrade panel
        Initialize();
    }

    void OnEnable()
    {
        BuildingSystem.OnPlayerCreditsChanged += UpdateUnlockCostTextColor;
    }

    void OnDisable()
    {
        BuildingSystem.OnPlayerCreditsChanged -= UpdateUnlockCostTextColor;
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
            SetTextReference(uIUpgrade, txtObj);

            //uIUpgrade.Initialize();

            // Add to fade canvas groups
            fadeCanvasGroups.individualElements.Add(buttonPrefab.GetComponent<CanvasGroup>());
            fadeCanvasGroups.individualElements.Add(txtObj.GetComponent<CanvasGroup>());
        }
        
        InitializeFadeGroups();
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

    void InitializeFadeGroups()
    {
        RefreshUpgradesText(BuildingSystem.PlayerCredits);
        fadeCanvasGroups.InitializeToFadedOut();
    }

    void SetTextReference(UIUpgrade upg, GameObject obj)
    {
        upg.upgradeText = obj.GetComponent<TextMeshProUGUI>();
        upg.Initialize();
    }

    public void OnUnlockClick()
    {
        float playerCredits = BuildingSystem.PlayerCredits;

        if (playerCredits >= unlockCost && !unlocked)
        {
            BuildingSystem.PlayerCredits -= unlockCost;
            unlocked = true;

            // Unlock gun
            if (upgradeFamily == UpgradeFamily.GunStats)
                player.playerShooter.UnlockGun(playerGunIndex);

            // Unlock Structures, add turret
            buildingSystem.UnlockStructures(structuresToUnlock);
            if (addTurretSlot)
                player.stats.structureStats.maxTurrets++;

            // Initialize and open upgrades panel
            Initialize();
            StartCoroutine(DelayFadeGroupTrigger());
            //StartCoroutine(DelayTrigger());
            Destroy(unlockPanel.gameObject);

            Debug.Log("UiUpgradePanel played upgEffect");
            UpgradeManager.Instance.upgradeEffects.PlayUpgradeButtonEffect();

            MessagePanel.PulseMessage(techUnlockMessageName + " unlocked!", Color.green);
        }
        else
        {
            MessagePanel.PulseMessage("Not enough Credits!", Color.red);
        }
    }

    public void RefreshUpgradesText(float playerCredits)
    {
        Debug.Log("Refreshing upgradesText with playerCredits: " + playerCredits);
        foreach (Transform child in buttonsGrid.transform)
        {
            UIUpgrade upg = child.GetComponent<UIUpgrade>();
            if (upg != null)
            {
                upg.Initialize();
            }
        }
    }

    IEnumerator DelayFadeGroupTrigger()
    {
        yield return new WaitForEndOfFrame();
        fadeCanvasGroups.isTriggered = true;
    }
}
