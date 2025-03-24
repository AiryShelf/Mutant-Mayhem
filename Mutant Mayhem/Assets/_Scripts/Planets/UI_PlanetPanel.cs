using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UI_PlanetPanel : MonoBehaviour
{
    [SerializeField] Button mapButton;
    [SerializeField] TextMeshProUGUI mapButtonText;
    [Header("Back To Ship Button Colors")]
    [SerializeField] ColorBlock backToShipButtonColors;
    ColorBlock showMapButtonStartColors;
    [Space][Space]
    [SerializeField] Transform highRezPlanetsGroup;
    [SerializeField] TextMeshProUGUI bodyTypeText;
    [SerializeField] TextMeshProUGUI bodyNameText;
    [SerializeField] Transform infoPanel;
    [SerializeField] GameObject propertyCardPrefab;
    [SerializeField] GameObject modifierTextPrefab;
    [SerializeField] Color buffModifierColor = Color.green;
    [SerializeField] Color debuffModifierColor = Color.red;
    [SerializeField] TextMeshProUGUI toPassMissionText;
    [SerializeField] FadeCanvasGroupsWave mainFadeGroup;
    [SerializeField] FadeCanvasGroupsWave mapFadeGroup;
    [SerializeField] FadeRenderers mapFadeRenderers;
    [SerializeField] FadeRenderers mainFadeRenderers;
    [SerializeField] ParticleSystemFader particleFader;
    [SerializeField] ParticleSystemTrailFader trailFader;

    bool isMapOpen = false;
    Dictionary<PlanetSO, GameObject> _highRezPlanets = new Dictionary<PlanetSO, GameObject>();

    void Start()
    {
        if (mapButton != null)
            showMapButtonStartColors = mapButton.colors;
        LoadPropertyCards(PlanetManager.Instance.currentPlanet);

        if (highRezPlanetsGroup == null) return;

        foreach (PlanetSO planet in PlanetManager.Instance.planetsSource)
        {
            if (planet.highRezPlanetPrefab == null) continue;

            GameObject obj = Instantiate(planet.highRezPlanetPrefab, highRezPlanetsGroup.position, Quaternion.identity, highRezPlanetsGroup);
            if (planet != PlanetManager.Instance.currentPlanet)
                obj.SetActive(false);

            _highRezPlanets.Add(planet, obj);
        }
    }

    void ClearInfoPanel()
    {
        foreach (Transform trans in infoPanel)
        {
            Destroy(trans.gameObject);
        }
    }

    public void LoadPropertyCards(PlanetSO planet)
    {
        if (planet == null) return;
        
        ClearInfoPanel();
        bodyTypeText.text = planet.typeOfBody;
        bodyNameText.text = planet.bodyName;
        toPassMissionText.text = planet.mission.toPassText;

        foreach (PlanetPropertySO planetProperty in planet.properties)
        {
            UI_PlanetPropertyCard propertyCard = Instantiate(propertyCardPrefab, infoPanel).GetComponent<UI_PlanetPropertyCard>();
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

    public void OnShowMapPressed()
    {
        if (isMapOpen)
        {
            // Close Map
            mainFadeRenderers.FadeIn();
            mainFadeGroup.isTriggered = true;
            mapFadeRenderers.FadeOut();
            mapFadeGroup.isTriggered = false;
            particleFader.FadeOutNewParticles();
            trailFader.FadeOut();
            isMapOpen = false;
            mapButtonText.text = "System Map";
            mapButton.colors = showMapButtonStartColors;

            ShowHighRezPlanet(true);
        }
        else
        {
            // Open Map
            mainFadeRenderers.FadeOut();
            mainFadeGroup.isTriggered = false;
            mapFadeRenderers.FadeIn();
            mapFadeGroup.isTriggered = true;
            particleFader.FadeInNewParticles();
            trailFader.FadeIn();
            isMapOpen = true;
            mapButtonText.text = "Back to Ship";
            mapButton.colors = backToShipButtonColors;

            ShowHighRezPlanet(false);
        }
    } 

    void ShowHighRezPlanet(bool show)
    {
        if (_highRezPlanets.ContainsKey(PlanetManager.Instance.currentPlanet))
        {
            _highRezPlanets[PlanetManager.Instance.currentPlanet].SetActive(show);
        }
        else
        {
            Debug.LogError($"UI_PlanetPanel: Could not find {PlanetManager.Instance.currentPlanet} in _highRezPlanets");
        }
    }
}
