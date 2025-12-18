using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;



public class UiUpgradePanel : UI_PanelBase
{
    public StructureType structureToBuildForUnlock;
    [SerializeField] protected List<GameObject> UIUpgradePrefabs = new List<GameObject>();
    [SerializeField] protected List<GameObject> UIUpgradePrefabs2 = new List<GameObject>();
    public GridLayoutGroup buttonsGrid;
    public GridLayoutGroup textGrid;
    public GridLayoutGroup buttonsGrid2;
    public GridLayoutGroup textGrid2;
    [SerializeField] CanvasGroup closeButton;
    [SerializeField] protected CanvasGroup mainPanelCanvasGroup;
    [SerializeField] protected CanvasGroup poweredCanvasGroup;
    [SerializeField] protected CanvasGroup noPowerCanvasGroup;
    public bool hasPower = false;

    [Header("Unlock Gun")]
    [SerializeField] protected UpgradeFamily upgradeFamily;
    [SerializeField] protected int playerGunIndex;

    protected Player player;
    protected PanelInteract panelInteract;

    protected virtual void Awake()
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

    protected virtual void Start()
    {
        // Initialize upgrade lists into UI
        StartCoroutine(DelayInitializeFadeGroups());
    }

    public void TriggerClosePanel()
    {
        player.OnEscapePressed(new InputAction.CallbackContext());
    }

    IEnumerator DelayInitializeFadeGroups()
    {
        yield return new WaitForSeconds(0.5f);

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
    protected virtual void PopulateUpgrades(List<GameObject> upgradePrefabs, GridLayoutGroup buttonsGroup, GridLayoutGroup textGroup)
    {
        if (upgradePrefabs.Count < 1 || buttonsGroup == null || textGroup == null)
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
        closeButton.blocksRaycasts = false;
        closeButton.interactable = false;
        RefreshUpgradesText(BuildingSystem.PlayerCredits);
        fadeCanvasGroups.InitializeToFadedOut();
    }

    protected virtual void SetTextReference(UIUpgrade upg, GameObject obj)
    {
        upg.upgradeText = obj.GetComponent<TextMeshProUGUI>();
        upg.Initialize();
    }

    public virtual void RefreshUpgradesText(float playerCredits)
    {
        //Debug.Log("Refreshing upgradesText with playerCredits: " + playerCredits);
        foreach (Transform child in buttonsGrid.transform)
        {
            UIUpgrade upg = child.GetComponent<UIUpgrade>();
            if (upg != null)
            {
                upg.Initialize();
            }
        }
    }

    public virtual void OnPowerOn(bool playEffect)
    {
        hasPower = true;

        // Unlock gun
        if (upgradeFamily == UpgradeFamily.GunStats)
            player.playerShooter.UnlockGun(playerGunIndex);

        ShowUpgradesPanel();

        if (playEffect)
        {
            UpgradeManager.Instance.upgradeEffects.PlayUnlockEffect(transform.position);
        }      
    }

    public virtual void OnPowerOff(bool playEffect)
    {
        hasPower = false;

        // Lock gun
        //if (upgradeFamily == UpgradeFamily.GunStats && playEffect)
            //player.playerShooter.LockGun(playerGunIndex);

        ShowNoPowerPanel();
    }

    public virtual void OpenPanel(PanelInteract interactSource)
    {
        closeButton.blocksRaycasts = true;
        closeButton.interactable = true;
        fadeCanvasGroups.isTriggered = true;
        mainPanelCanvasGroup.alpha = 1;
        mainPanelCanvasGroup.blocksRaycasts = true;
        mainPanelCanvasGroup.interactable = true;
        
        if (hasPower)
            ShowUpgradesPanel();
        else
            ShowNoPowerPanel();

        panelInteract = interactSource;
        //Debug.Log("UiUpgradePanel: Opened panel for " + structureToBuildForUnlock);
    }

    public virtual void ClosePanel()
    {
        closeButton.blocksRaycasts = false;
        closeButton.interactable = false;
        fadeCanvasGroups.isTriggered = false;
        mainPanelCanvasGroup.alpha = 0;
        mainPanelCanvasGroup.blocksRaycasts = false;
        mainPanelCanvasGroup.interactable = false;

        if (panelInteract != null)
        {
            panelInteract.StopAllCoroutines();
            panelInteract = null;
        }
        //Debug.Log("UiUpgradePanel: Closed panel for " + structureToBuildForUnlock);
    }

    protected virtual void ShowUpgradesPanel()
    {
        poweredCanvasGroup.alpha = 1;
        poweredCanvasGroup.blocksRaycasts = true;
        poweredCanvasGroup.interactable = true;

        noPowerCanvasGroup.alpha = 0;
        noPowerCanvasGroup.blocksRaycasts = false;
        noPowerCanvasGroup.interactable = false;
    }

    protected virtual void ShowNoPowerPanel()
    {
        poweredCanvasGroup.alpha = 0;
        poweredCanvasGroup.blocksRaycasts = false;
        poweredCanvasGroup.interactable = false;

        noPowerCanvasGroup.alpha = 1;
        noPowerCanvasGroup.blocksRaycasts = true;
        noPowerCanvasGroup.interactable = true;
    }
}
