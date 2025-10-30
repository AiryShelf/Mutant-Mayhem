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
    public StructureType structureToBuildForUnlock;
    [SerializeField] List<GameObject> UIUpgradePrefabs;
    [SerializeField] List<GameObject> UIUpgradePrefabs2;
    public GridLayoutGroup buttonsGrid;
    public GridLayoutGroup textGrid;
    public GridLayoutGroup buttonsGrid2;
    public GridLayoutGroup textGrid2;
    [SerializeField] CanvasGroup mainPanelCanvasGroup;
    [SerializeField] CanvasGroup upgradesCanvasGroup;
    [SerializeField] CanvasGroup noPowerCanvasGroup;
    public bool hasPower = false;

    [Header("Unlockables (Optional)")]
    public string techUnlockMessageName;

    [Header("Unlock Gun")]
    [SerializeField] UpgradeFamily upgradeFamily;
    [SerializeField] int playerGunIndex;

    Player player;
    PanelInteract panelInteract;

    void Awake()
    {
        player = FindObjectOfType<Player>();

        noPowerCanvasGroup.alpha = 0;
        noPowerCanvasGroup.blocksRaycasts = false;
        noPowerCanvasGroup.interactable = false;

        // Clear editor objects in layout groups
        for (int i = buttonsGrid.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(buttonsGrid.transform.GetChild(i).gameObject);
        }
        for (int i = textGrid.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(textGrid.transform.GetChild(i).gameObject);
        }

        if (buttonsGrid2 == null || textGrid2 == null)
            return;
        for (int i = buttonsGrid2.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(buttonsGrid2.transform.GetChild(i).gameObject);
        }
        for (int i = textGrid2.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(textGrid2.transform.GetChild(i).gameObject);
        } 
    }

    void Start()
    {
        // Initialize upgrade lists into UI
        PopulateUpgrades(UIUpgradePrefabs, buttonsGrid, textGrid);
        PopulateUpgrades(UIUpgradePrefabs2, buttonsGrid2, textGrid2);

        InitializeFadeGroups();
    }

    /// <summary>
    /// Populates upgrade buttons and corresponding text objects for a given set of prefabs and target layout groups.
    /// </summary>
    /// <param name="upgradePrefabs">List of UI upgrade prefab GameObjects to instantiate.</param>
    /// <param name="buttonsGroup">Target GridLayoutGroup for the buttons.</param>
    /// <param name="textGroup">Target GridLayoutGroup for the text objects.</param>
    void PopulateUpgrades(List<GameObject> upgradePrefabs, GridLayoutGroup buttonsGroup, GridLayoutGroup textGroup)
    {
        if (upgradePrefabs == null || buttonsGroup == null || textGroup == null)
            return;
            
        foreach (GameObject upgrade in upgradePrefabs)
        {
            // Create button, get text prefab
            GameObject buttonPrefab = Instantiate(upgrade, buttonsGroup.transform);
            UIUpgrade uIUpgrade = buttonPrefab.GetComponent<UIUpgrade>();
            GameObject textPrefab = uIUpgrade.upgradeTextPrefab;

            // Create text obj and give text instance to uIUpgrade
            GameObject txtObj = Instantiate(textPrefab, textGroup.transform);
            SetTextReference(uIUpgrade, txtObj);

            // Add to fade canvas groups
            fadeCanvasGroups.individualElements.Add(buttonPrefab.GetComponent<CanvasGroup>());
            fadeCanvasGroups.individualElements.Add(txtObj.GetComponent<CanvasGroup>());
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

    public void OnPowerOn(bool playEffect)
    {
        hasPower = true;

        // Unlock gun
        if (upgradeFamily == UpgradeFamily.GunStats)
            player.playerShooter.UnlockGun(playerGunIndex);

        ShowUpgradesPanel();

        if (playEffect)
        {
            MessagePanel.PulseMessage(techUnlockMessageName + " unlocked!", Color.green);
            UpgradeManager.Instance.upgradeEffects.PlayUnlockEffect(transform.position);
        }      
    }

    public void OnPowerOff(bool playEffect)
    {
        hasPower = false;

        // Lock gun
        if (upgradeFamily == UpgradeFamily.GunStats && playEffect)
            player.playerShooter.LockGun(playerGunIndex);

        ShowNoPowerPanel();

        if (playEffect)
            MessagePanel.PulseMessage(techUnlockMessageName + " locked!", Color.red);
    }

    public void OpenPanel(PanelInteract interactSource)
    {
        fadeCanvasGroups.isTriggered = true;
        mainPanelCanvasGroup.alpha = 1;
        mainPanelCanvasGroup.blocksRaycasts = true;
        mainPanelCanvasGroup.interactable = true;

        panelInteract = interactSource;
        Debug.Log("UiUpgradePanel: Opened panel for " + structureToBuildForUnlock);
    }

    public void ClosePanel()
    {
        fadeCanvasGroups.isTriggered = false;
        mainPanelCanvasGroup.alpha = 0;
        mainPanelCanvasGroup.blocksRaycasts = false;
        mainPanelCanvasGroup.interactable = false;

        if (panelInteract != null)
        {
            panelInteract.StopAllCoroutines();
            panelInteract = null;
        }
        Debug.Log("UiUpgradePanel: Closed panel for " + structureToBuildForUnlock);
    }

    void ShowUpgradesPanel()
    {
        upgradesCanvasGroup.alpha = 1;
        upgradesCanvasGroup.blocksRaycasts = true;
        upgradesCanvasGroup.interactable = true;

        noPowerCanvasGroup.alpha = 0;
        noPowerCanvasGroup.blocksRaycasts = false;
        noPowerCanvasGroup.interactable = false;
    }

    void ShowNoPowerPanel()
    {
        upgradesCanvasGroup.alpha = 0;
        upgradesCanvasGroup.blocksRaycasts = false;
        upgradesCanvasGroup.interactable = false;

        noPowerCanvasGroup.alpha = 1;
        noPowerCanvasGroup.blocksRaycasts = true;
        noPowerCanvasGroup.interactable = true;
    }
}
