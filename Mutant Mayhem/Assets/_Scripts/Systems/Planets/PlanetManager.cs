using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetManager : MonoBehaviour
{
    public static PlanetManager Instance;

    public List<Planet> planets;
    public Planet currentPlanet;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetCurrentPlanet(0);
    }

    public void SetCurrentPlanet(int index)
    {
        currentPlanet = planets[index];
        ApplyPlanetProperties();
    }

    public void SetCurrentPlanet(Planet planet)
    {
        int index = planets.IndexOf(planet);
        if (index != -1)
        {
            currentPlanet = planets[index];
        }
        else
            Debug.LogError("Planet not found in PlanetManager's list");
    }

    public void ApplyPlanetProperties()
    {
        if (currentPlanet == null)
        {
            Debug.LogError("No planet is set.");
            return;
        }
    }
}
