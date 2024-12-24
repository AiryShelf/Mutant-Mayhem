using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlanetPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI mapButtonText;
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

    public static UI_PlanetPanel Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        foreach (PlanetSO planet in PlanetManager.Instance.planetsSource)
        {
            if (planet.highRezPlanetPrefab == null) continue;

            GameObject obj = Instantiate(planet.highRezPlanetPrefab, highRezPlanetsGroup.position, Quaternion.identity, highRezPlanetsGroup);
            if (planet != PlanetManager.Instance.currentPlanet)
                obj.SetActive(false);

            _highRezPlanets.Add(planet, obj);
        }

        LoadPropertyCards(PlanetManager.Instance.currentPlanet);
    }

    void ClearInfoPanel()
    {
        foreach (Transform trans in infoPanel)
        {
            Destroy(trans.gameObject);
        }
    }

    public static void LoadPropertyCards(PlanetSO planet)
    {
        if (planet == null) return;
        
        Instance.ClearInfoPanel();
        Instance.bodyTypeText.text = planet.typeOfBody;
        Instance.bodyNameText.text = planet.bodyName;
        Instance.toPassMissionText.text = planet.mission.toPassText;

        foreach (PlanetPropertySO planetProperty in planet.properties)
        {
            UI_PlanetPropertyCard propertyCard = Instantiate(Instance.propertyCardPrefab, Instance.infoPanel).GetComponent<UI_PlanetPropertyCard>();
            propertyCard.propertyNameText.text = planetProperty.propertyName;

            foreach (StatModifierEntry entry in planetProperty.statModifierEntries)
            {
                TextMeshProUGUI modifierText = Instantiate(Instance.modifierTextPrefab, propertyCard.modifierLayoutGroup).GetComponent<TextMeshProUGUI>();
                modifierText.text = "    " + entry.modifierUiName;
                if (entry.isDebuff)
                    modifierText.color = Instance.debuffModifierColor;
                else
                    modifierText.color = Instance.buffModifierColor;
            }
        }

        Instance.RefreshLayout();
    }  

    public void RefreshLayout()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    public void OnShowMapPressed()
    {
        if (isMapOpen)
        {
            mainFadeRenderers.FadeIn();
            mainFadeGroup.isTriggered = true;
            mapFadeRenderers.FadeOut();
            mapFadeGroup.isTriggered = false;
            particleFader.FadeOutNewParticles();
            trailFader.FadeOut();
            isMapOpen = false;
            mapButtonText.text = "System Map";

            // Show high-rez planet
            if (_highRezPlanets.ContainsKey(PlanetManager.Instance.currentPlanet))
            {
                _highRezPlanets[PlanetManager.Instance.currentPlanet].SetActive(true);
            }
            else
            {
                Debug.LogError($"UI_PlanetPanel: Could not find {PlanetManager.Instance.currentPlanet} in _highRezPlanets");
            }
        }
        else
        {
            mainFadeRenderers.FadeOut();
            mainFadeGroup.isTriggered = false;
            mapFadeRenderers.FadeIn();
            mapFadeGroup.isTriggered = true;
            particleFader.FadeInNewParticles();
            trailFader.FadeIn();
            isMapOpen = true;
            mapButtonText.text = "Back to Ship";

            // Hide high-rez planet
            if (_highRezPlanets.ContainsKey(PlanetManager.Instance.currentPlanet))
            {
                _highRezPlanets[PlanetManager.Instance.currentPlanet].SetActive(false);
            }
            else
            {
                Debug.LogError($"UI_PlanetPanel: Could not find {PlanetManager.Instance.currentPlanet} in _highRezPlanets");
            }
        }
    } 
}
