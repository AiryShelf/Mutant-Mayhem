using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIUpgrade : MonoBehaviour
{
    public UpgradeType type;
    public string UiName;
    [TextArea(3,10)]
    public string tooltipDescription;
    
    public GameObject upgradeTextPrefab;

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
    public void InvokeOnClick(UIUpgrade u)
    {
        Debug.Log("OnClick called");
        upgradeSystem.OnUpgradeButtonClicked(u.type);

        UpdateText();
    }

    void UpdateText()
    {
        int upgLvl = upgradeSystem.upgradeLevels[type];
        int upgCost = upgradeSystem.upgradeCurrentCosts[type];

        upgradeText.text = 
            UiName + " Lvl " + (upgLvl + 1) + "\n" + "$" + upgCost;       
    }
}
