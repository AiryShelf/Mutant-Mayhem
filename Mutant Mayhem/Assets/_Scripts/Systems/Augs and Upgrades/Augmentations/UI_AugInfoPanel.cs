using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_AugInfoPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI classText;
    [SerializeField] VerticalLayoutGroup nameGroup;
    [SerializeField] VerticalLayoutGroup valueGroup;
    [SerializeField] GameObject namePrefab;
    [SerializeField] GameObject valuePrefab;

    void Start()
    {
        switch (ClassManager.Instance.selectedClass)
        {
            case PlayerClass.Fighter:
                classText.text = "Fighter Class";
                break;
            case PlayerClass.Neutral:
                classText.text = "Neutral Class";
                break;
            case PlayerClass.Builder:
                classText.text = "Builder Class";
                break;
            
        }

        Dictionary<AugmentationBaseSO, int> augsWithLevels = AugManager.Instance.selectedAugsWithLvls;

        // Create stat group, add entries
        StatGroupWrapper statGroup = new StatGroupWrapper(nameGroup, valueGroup, namePrefab, valuePrefab);

        if (augsWithLevels.Count < 1)
        {
            statGroup.AddStat("No Augs Selected!", "");
        }

        foreach (var kvp in augsWithLevels)
        {
            //string posOrNeg;
            Color textColor;
            int level = kvp.Value;
            
            if (level >= 0)
            {
                //posOrNeg = "+";
                textColor = Color.cyan;
            }
            else
            {
               //posOrNeg = "-";
               textColor = Color.red;
            }

            string value = $"Lvl {level}";
            statGroup.AddStat($"{kvp.Key.augmentationName}", value, textColor);
        }
    }
}
