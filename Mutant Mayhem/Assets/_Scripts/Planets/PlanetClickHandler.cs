using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlanetClickHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public PlanetSO planetSO;
    [SerializeField] GameObject highlightPrefab;
    [SerializeField] GameObject selectedHighlightPrefab;
    [SerializeField] Transform parentTransform;
    GameObject highlightInstance;
    GameObject selectedHighlightInstance;

    void Start()
    {
        highlightInstance = Instantiate(highlightPrefab, transform.position, Quaternion.identity, parentTransform);
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

        PlanetManager.Instance.SetCurrentPlanet(planetSO);
        if (PlanetManager.Instance.currentPlanet != planetSO)
        {
            UI_PlanetPanel.LoadPropertyCards(planetSO);
            selectedHighlightInstance.SetActive(false);
        }
        else
            selectedHighlightInstance.SetActive(true);
    }
}
