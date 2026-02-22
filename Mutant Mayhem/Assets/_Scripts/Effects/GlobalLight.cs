using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GlobalLight : MonoBehaviour
{
    public static GlobalLight Instance;

    [SerializeField] Light2D globalLight;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (PlanetManager.Instance != null && PlanetManager.Instance.currentPlanet != null)
        {
            SetGlobalLightColor(PlanetManager.Instance.currentPlanet.globalLightColor);
            SetGlobalLightIntensity(PlanetManager.Instance.currentPlanet.globalLightIntensity);
        }
    }

    public void SetGlobalLightColor(Color color)
    {
        globalLight.color = color;
    }

    public void SetGlobalLightIntensity(float intensity)
    {
        globalLight.intensity = intensity;
    }
}
