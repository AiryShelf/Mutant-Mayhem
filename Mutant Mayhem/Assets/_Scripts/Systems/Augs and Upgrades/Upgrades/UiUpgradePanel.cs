using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public enum UpgradePanelType
{
    None,
    Consumables,
    Exosuit,
    Lasers,
    Bullets,
    Structures,
    Repair,
    Explosives,
    Drones,
}

public class UiUpgradePanel : UI_PanelBase
{
    [SerializeField] List<GameObject> UIUpgradePrefabs;
    public GridLayoutGroup buttonsGrid;
    public GridLayoutGroup textGrid;
    [SerializeField] PanelSwitcher panelSwitcher;
    [SerializeField] UpgradePanelType upgradePanelType;
    public bool isUnlocked = false;

    [Header("Unlockables (Optional)")]
    [SerializeField] GameObject unlockPanel;
    [SerializeField] TextMeshProUGUI unlockPanelInfoText;
    [SerializeField] Image noPowerImage;
    [SerializeField] Image buildToUnlockImage;
    [SerializeField] Sprite buildSpriteNoPower;
    Sprite buildSpriteStart;
    [SerializeField] string unlockPanelNoPowerString;
    string unlockPanelStartString;
    [SerializeField] TextMeshProUGUI unlockCostText;
    [SerializeField] int unlockCost;
    public string techUnlockMessageName;

    [Header("Unlock Gun")]
    [SerializeField] UpgradeFamily upgradeFamily;
    [SerializeField] int playerGunIndex;

    [Header("Unlock Structure")] // Partially moved to StructureSO
    [SerializeField] bool addTurretSlot;

    [Header("Unlock Buttons")]
    [SerializeField] List<UIUpgrade> uiUpgradesToUnlock;

    Player player;
    BuildingSystem buildingSystem;

    void Awake()
    {
        if (buildToUnlockImage != null)
            buildSpriteStart = buildToUnlockImage.sprite;
        if (unlockPanelInfoText != null)
            unlockPanelStartString = unlockPanelInfoText.text;

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
        Initialize();

        // Show unlock panel
        if (unlockPanel != null)
        {
            //unlockPanel.gameObject.SetActive(false);
            unlockPanel.gameObject.SetActive(true);
            //fadeCanvasGroups.enabled = false;
            UpdateUnlockCostTextColor(BuildingSystem.PlayerCredits);
            return;
        }

        // Initialize upgrade panel
        
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
        //fadeCanvasGroups.enabled = true;

        // Initialize upgrade lists into UI
        foreach (GameObject upgrade in UIUpgradePrefabs)
        {
            // Create button, get text prefab
            GameObject buttonPrefab = Instantiate(upgrade, buttonsGrid.transform);
            UIUpgrade uIUpgrade = buttonPrefab.GetComponent<UIUpgrade>();
            GameObject textPrefab = uIUpgrade.upgradeTextPrefab;

            // Create text obj and give text instance to uIUpgrade
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
        if (!isUnlocked && unlockPanel != null)
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

    public void OnUnlocked(StructureSO structure, bool playEffect)
    {
        isUnlocked = true;

        // Unlock gun
        if (upgradeFamily == UpgradeFamily.GunStats)
            player.playerShooter.UnlockGun(playerGunIndex);

        // Unlock Structures, add turret
        //buildingSystem.UnlockStructures(structure, playEffect);
        if (addTurretSlot)
            player.stats.structureStats.maxTurrets++;

        // Initialize and open upgrades panel
        buttonsGrid.GetComponent<CanvasGroup>().alpha = 1;
        textGrid.GetComponent<CanvasGroup>().alpha = 1;
        //StartCoroutine(DelayFadeGroupTrigger(true, false));
        unlockPanel.SetActive(false);

        if (playEffect)
            UpgradeManager.Instance.upgradeEffects.PlayUnlockEffect();
        //Debug.Log("UiUpgradePanel played upgEffect");

        UnlockButtons(true);

        if (playEffect)
        {
            MessagePanel.PulseMessage(techUnlockMessageName + " unlocked!", Color.green);
        }
    }

    public void OnLocked(StructureSO structure, bool playEffect)
    {
        isUnlocked = false;

        // Unlock gun
        if (upgradeFamily == UpgradeFamily.GunStats && playEffect)
            player.playerShooter.LockGun(playerGunIndex);

        // Unlock Structures, add turret
        //buildingSystem.LockStructures(structure, playEffect);
        if (addTurretSlot)
            player.stats.structureStats.maxTurrets--;

        // Initialize and open upgrades panel
        buttonsGrid.GetComponent<CanvasGroup>().alpha = 0;
        textGrid.GetComponent<CanvasGroup>().alpha = 0;
        //StartCoroutine(DelayFadeGroupTrigger(false, true));
        unlockPanel.SetActive(true);

        //if (playEffect)
            //UpgradeManager.Instance.upgradeEffects.PlayUpgradeButtonEffect();
        //Debug.Log("UiUpgradePanel played upgEffect");

        UnlockButtons(false);

        if (playEffect)
        {
            MessagePanel.PulseMessage(techUnlockMessageName + " locked!", Color.red);
            if (unlockPanelInfoText != null)
                unlockPanelInfoText.text = unlockPanelStartString;
            if (buildToUnlockImage != null)
                buildToUnlockImage.sprite = buildSpriteStart;
                
            noPowerImage.enabled = false;
        }
        else
        {
            if (unlockPanelInfoText != null)
                unlockPanelInfoText.text = unlockPanelNoPowerString;
            if (buildSpriteNoPower != null)
                buildToUnlockImage.sprite = buildSpriteNoPower;

            noPowerImage.enabled = true;
        }
    }

    public void OnUnlockClick() // DEPRICATED
    {
        float playerCredits = BuildingSystem.PlayerCredits;

        if (playerCredits >= unlockCost && !isUnlocked)
        {
            BuildingSystem.PlayerCredits -= unlockCost;
            isUnlocked = true;

            // Unlock gun
            if (upgradeFamily == UpgradeFamily.GunStats)
                player.playerShooter.UnlockGun(playerGunIndex);

            // Unlock Structures, add turret
            buildingSystem.UnlockStructures(BuildingSystem.Instance.structureInHand, true);
            if (addTurretSlot)
                player.stats.structureStats.maxTurrets++;

            // Initialize and open upgrades panel
            buttonsGrid.GetComponent<CanvasGroup>().alpha = 1;
            textGrid.GetComponent<CanvasGroup>().alpha = 1;
            StartCoroutine(DelayFadeGroupTrigger(true, false));

            UpgradeManager.Instance.upgradeEffects.PlayUpgradeButtonEffect();
            //Debug.Log("UiUpgradePanel played upgEffect");

            UnlockButtons(true);

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

    IEnumerator DelayFadeGroupTrigger(bool isTriggered, bool isUnlockPanelActive)
    {
        yield return new WaitForEndOfFrame();

        fadeCanvasGroups.isTriggered = isTriggered;

        if (unlockPanel != null)
            unlockPanel.SetActive(isUnlockPanelActive);
    }

    void UnlockButtons(bool unlocked)
    {
        foreach (UIUpgrade upg in uiUpgradesToUnlock)
        {
            // Search PanelSwitcher's panels
            foreach (UI_PanelBase panel in panelSwitcher.panels)
            {
                // Search individual elements in panel
                foreach (CanvasGroup group in panel.fadeCanvasGroups.individualElements)
                {
                    UIUpgrade otherUpg = group.GetComponent<UIUpgrade>();
                    if (otherUpg == null)
                        continue;

                    // Turn button on/off
                    if (otherUpg.gameObject.name == upg.gameObject.name + "(Clone)")
                    {
                        otherUpg.buttonImage.enabled = unlocked;
                        otherUpg.unlocked = unlocked;
                    }
                }
            }
        }
    }
}
