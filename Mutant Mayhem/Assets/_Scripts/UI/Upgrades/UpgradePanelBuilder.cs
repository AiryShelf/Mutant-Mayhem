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

    [Header("Unlockable")]
    [SerializeField] UpgradeFamily upgradeFamily;
    [SerializeField] int playerGunIndex;
    [SerializeField] GameObject unlockPanel;
    [SerializeField] TextMeshProUGUI costText;
    [SerializeField] int unlockCost;
    [SerializeField] string unlockName;
    
    bool unlocked = false;
    Player player;
    MessagePanel messagePanel;
    UpgradeSystem upgradeSystem;

    void Awake()
    {
        player = FindObjectOfType<Player>();
        messagePanel = FindObjectOfType<MessagePanel>();
        upgradeSystem = FindObjectOfType<UpgradeSystem>();

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

    void FixedUpdate()
    {
        // Update cost text color as per afforadability
        if (!unlocked && unlockPanel != null)
        {
            Color color;
            if (BuildingSystem.PlayerCredits >= unlockCost)
                color = Color.yellow;
            else
                color = Color.red;

            costText.text = "$" + unlockCost.ToString();
            costText.color = color;
        }
    }

    void Start()
    {
        // Clear unlock button
        if (unlockPanel)
            unlockPanel.gameObject.SetActive(false);

        // If is a gun upgrade, check for unlock
        bool isUnlocked;
        if (upgradeFamily == UpgradeFamily.GunStats && unlockPanel != null)
        {
            isUnlocked = CheckIfGunUnlocked();
            if (!isUnlocked)
            {
                ShowUnlockButton();
                return;
            }
        }

        // Initialize panel
        Initialize();
        fadeCanvasGroups.Initialize();
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
            uIUpgrade.upgradeTextInstance = Instantiate(textPrefab, textGrid.transform);

            // Add to fade canvas groups
            fadeCanvasGroups.individualElements.Add(
                buttonPrefab.GetComponent<CanvasGroup>());
            fadeCanvasGroups.individualElements.Add(
                uIUpgrade.upgradeTextInstance.GetComponent<CanvasGroup>());

            // Fade out for unlock
            /*
            if (unlockPanel != null)
            {
                buttonPrefab.GetComponent<CanvasGroup>().alpha = 0;
                uIUpgrade.upgradeTextInstance.GetComponent<CanvasGroup>().alpha = 0;
                uIUpgrade.statValueTextInstance.GetComponent<CanvasGroup>().alpha = 0;
            }
            */
        }
    }

    void ShowUnlockButton()
    {
        unlockPanel.gameObject.SetActive(true);
    }

    public void OnButtonClick()
    {
        if (BuildingSystem.PlayerCredits >= unlockCost && !unlocked)
        {
            BuildingSystem.PlayerCredits -= unlockCost;
            unlocked = true;

            // Unlock gun
            if (upgradeFamily == UpgradeFamily.GunStats)
            {
                player.playerShooter.gunsUnlocked[playerGunIndex] = true;
            }

            // Initialize and open panel
            Initialize();
            //StartCoroutine(DelayTrigger());
            unlockPanel.gameObject.SetActive(false);

            upgradeSystem.PlayUpgradeEffects();

            messagePanel.ShowMessage(unlockName + " unlocked!", Color.green);
        }
        else
        {
            messagePanel.ShowMessage("Not enough Credits!", Color.red);
        }
    }

    IEnumerator DelayTrigger()
    {
        yield return new WaitForSeconds(1f);
        fadeCanvasGroups.isTriggered = true;
    }

    bool CheckIfGunUnlocked()
    {
        bool isGunUnlocked = false;
        if (player.playerShooter.gunsUnlocked[playerGunIndex])
        {
            isGunUnlocked = true;
        }

        return isGunUnlocked;
    }

}