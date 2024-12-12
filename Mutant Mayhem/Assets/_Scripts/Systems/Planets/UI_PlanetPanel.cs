using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_PlanetPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI planetNameText;
    [SerializeField] Transform infoPanel;
    [SerializeField] GameObject propertyCardPrefab;
    [SerializeField] GameObject modifierTextPrefab;
    [SerializeField] Color buffModifierColor = Color.green;
    [SerializeField] Color debuffModifierColor = Color.red;
    [SerializeField] TextMeshProUGUI missionText;

    void Start()
    {
        LoadPropertyCards(PlanetManager.Instance.currentPlanet);
    }

    void ClearInfoPanel()
    {
        foreach (Transform trans in infoPanel)
        {
            Destroy(trans.gameObject);
        }
    }

    public void LoadPropertyCards(Planet planet)
    {
        ClearInfoPanel();

        foreach (PlanetPropertySO planetProperty in PlanetManager.Instance.currentPlanet.properties)
        {
            UI_PropertyCard propertyCard = Instantiate(propertyCardPrefab, infoPanel).GetComponent<UI_PropertyCard>();
            propertyCard.propertyNameText.text = planetProperty.propertyName;

            foreach (StatModifierEntry entry in planetProperty.statModifierEntries)
            {
                TextMeshProUGUI modifierText = Instantiate(modifierTextPrefab, propertyCard.modifierLayoutGroup).GetComponent<TextMeshProUGUI>();
                modifierText.text = "    " + entry.modifierUiName;
                if (entry.isDebuff)
                    modifierText.color = debuffModifierColor;
                else
                    modifierText.color = buffModifierColor;
            }
        }
    }   
}
