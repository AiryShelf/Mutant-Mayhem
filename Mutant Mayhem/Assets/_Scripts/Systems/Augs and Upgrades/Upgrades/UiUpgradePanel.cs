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
    public GridLayoutGroup buttonsGrid;
    public GridLayoutGroup textGrid;
    CanvasGroup mainPanelCanvasGroup;
    CanvasGroup upgradesCanvasGroup;
    CanvasGroup noPowerCanvasGroup;
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

        // Show upgrades panel
        upgradesCanvasGroup.GetComponent<CanvasGroup>().alpha = 1;
        noPowerCanvasGroup.GetComponent<CanvasGroup>().alpha = 0;

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

        // Show no power panel
        upgradesCanvasGroup.GetComponent<CanvasGroup>().alpha = 0;
        noPowerCanvasGroup.GetComponent<CanvasGroup>().alpha = 1;

        if (playEffect)
            MessagePanel.PulseMessage(techUnlockMessageName + " locked!", Color.red);
    }

    public void OpenPanel(PanelInteract interactSource)
    {
        fadeCanvasGroups.isTriggered = true;
        mainPanelCanvasGroup.blocksRaycasts = true;
        mainPanelCanvasGroup.interactable = true;

        panelInteract = interactSource;
    }

    public void ClosePanel()
    {
        fadeCanvasGroups.isTriggered = false;
        mainPanelCanvasGroup.blocksRaycasts = false;
        mainPanelCanvasGroup.interactable = false;

        if (panelInteract != null)
        {
            panelInteract.StopAllCoroutines();
            panelInteract = null;
        }
    }
}
