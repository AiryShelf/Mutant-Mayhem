using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class PlanetUnlockLines : MonoBehaviour
{
    [SerializeField] Transform solarSystem;
    [SerializeField] GameObject lineRendererPrefab;
    [SerializeField] float zOffset = -3f;
    [SerializeField] Color completeColor = new Color(0f, 1f, 0f, 1f);
    [SerializeField] Color incompleteColor = new Color(1f, 0f, 0f, 1f);

    UI_PlanetClickHandler myClickHandler;
    List<Vector3> prerequisitePositions = new List<Vector3>();
    List<bool>prerequisitesComplete = new List<bool>();
    List<LineRenderer> lineRenderers = new List<LineRenderer>();

    void Start()
    {
        if (solarSystem == null)
        {
            Debug.LogError("PlanetUnlockLines: SolarSystem is not assigned to " + transform.parent.parent.gameObject);
            Destroy(this);
            return;
        }

        myClickHandler = GetComponent<UI_PlanetClickHandler>();
        if (myClickHandler == null)
        {
            Debug.LogError("PlanetUnlockLines: No attached PlanetClickHandler!");
            Destroy(this);
            return;
        }

        List<UI_PlanetClickHandler> planetClickHandlers = FindHandlers();
        FindPrerequisitePositions(planetClickHandlers);
        CreateLineRenderers();
    }

    void Update()
    {
        RefreshLineRenderers();
    }

    List<UI_PlanetClickHandler> FindHandlers()
    {
        List<UI_PlanetClickHandler> planetClickHandlers = 
            new List<UI_PlanetClickHandler>(solarSystem.GetComponentsInChildren<UI_PlanetClickHandler>());

        //Debug.Log($"Found {planetClickHandlers.Count} UI_PlanetClickHandler components.");
        return planetClickHandlers;
    }

    void FindPrerequisitePositions(List<UI_PlanetClickHandler> planetClickHandlers)
    {
        foreach (var handler in planetClickHandlers)
        {
            if (myClickHandler.planetSO.prerequisitePlanets.Contains(handler.planetSO))
            {
                prerequisitePositions.Add(handler.transform.position);

                if (ProfileManager.Instance.currentProfile.completedPlanets.Contains(handler.planetSO.bodyName))
                    prerequisitesComplete.Add(true);
                else
                    prerequisitesComplete.Add(false);
            }
        }

        for (int i = 0; i < prerequisitePositions.Count; i++)
        {
            prerequisitePositions[i] = new Vector3(prerequisitePositions[i].x, prerequisitePositions[i].y, prerequisitePositions[i].z + zOffset);
        }

        if (prerequisitePositions.Count < 1)
        {
            Debug.LogWarning("PlanetUnlockLines: Did not find any prerequisite positions!  Destroying self");
            Destroy(this);
        }
    }

    void CreateLineRenderers()
    {
        foreach (var pos in prerequisitePositions)
        {
            LineRenderer lr = Instantiate(lineRendererPrefab, transform).GetComponent<LineRenderer>();
            lr.SetPosition(0, transform.position);
            lineRenderers.Add(lr);
        }
    }

    void RefreshLineRenderers()
    {
        for (int i = 0; i < lineRenderers.Count; i++)
        {
            lineRenderers[i].SetPosition(1, prerequisitePositions[i]);

            Renderer renderer = lineRenderers[i].GetComponent<Renderer>();
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);

            if (prerequisitesComplete[i])
            {
                block.SetColor("_BaseColor", completeColor);
            }
            else
            {
                block.SetColor("_BaseColor", incompleteColor);
            }

            renderer.SetPropertyBlock(block);
        }
    }
}
