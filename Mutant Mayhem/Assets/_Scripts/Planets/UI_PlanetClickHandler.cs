using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_PlanetClickHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public PlanetSO planetSO;
    [SerializeField] GameObject highlightPrefab;
    [SerializeField] GameObject highlightLockedPrefab;
    [SerializeField] GameObject selectedHighlightPrefab;
    [SerializeField] Transform parentTransform;
    GameObject highlightInstance;
    GameObject selectedHighlightInstance;
    public bool unlocked = false;

    void Awake()
    {
        
    }

    void Start()
    {
        unlocked = ProfileManager.Instance.IsPlanetUnlocked(planetSO.prerequisitePlanets);

        GameObject highlight;
        if (unlocked)
            highlight = highlightPrefab;
        else
            highlight = highlightLockedPrefab;

        highlightInstance = Instantiate(highlight, transform.position, Quaternion.identity, parentTransform);
        highlightInstance.transform.localScale = Vector3.one; 
        highlightInstance.SetActive(false);

        selectedHighlightInstance = Instantiate(selectedHighlightPrefab, transform.position, Quaternion.identity, parentTransform);
        if (PlanetManager.Instance.currentPlanet != planetSO)
        {
            selectedHighlightInstance.transform.localScale = Vector3.one; 
            selectedHighlightInstance.SetActive(false);
        }
    }

    void LateUpdate()
    {
        if (highlightInstance != null)
            highlightInstance.transform.rotation = Quaternion.identity;

        if (PlanetManager.Instance.currentPlanet != planetSO)
            selectedHighlightInstance.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (PlanetManager.Instance.currentPlanet != planetSO)
            UI_PlanetPanel.LoadPropertyCards(planetSO);

        highlightInstance.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (PlanetManager.Instance.currentPlanet != planetSO)
            UI_PlanetPanel.LoadPropertyCards(PlanetManager.Instance.currentPlanet);

        highlightInstance.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (UI_PlanetPanel.Instance == null) return;

        if (!unlocked)
        {
            MessagePanel.PulseMessage("Planet is locked!  Conquer the previous planets first!", Color.red);
            return;
        }

        PlanetManager.Instance.SetCurrentPlanet(planetSO);
        if (PlanetManager.Instance.currentPlanet != planetSO)
        {
            UI_PlanetPanel.LoadPropertyCards(planetSO);
            selectedHighlightInstance.SetActive(false);
        }
        else
            selectedHighlightInstance.SetActive(true);
    }

    void SetHighlightColor(Color color)
    {
        Image image = highlightInstance.GetComponent<Image>();
        if (image != null)
        {
            // Ensure the Image has its own material instance
            if (image.material == null || image.material.name == image.defaultMaterial.name)
            {
                image.material = new Material(image.defaultMaterial);
            }

            // Set the color property on the material instance
            if (image.material.HasProperty("_BaseColor"))
            {
                image.material.SetColor("_BaseColor", color);
                image.material.renderQueue -= 1;
            }
            else
            {
                Debug.LogWarning("SetHighlightColor: The material does not have a _Color property.");
            }
        }
        else
        {
            Debug.LogError("PlanetClickHandler: Image component not found on highlightInstance.");
        }
    }
}
