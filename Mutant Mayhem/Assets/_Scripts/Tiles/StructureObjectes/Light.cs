using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Light : MonoBehaviour
{
    [SerializeField] protected List<Light2D> lights;

    protected virtual void Start()
    {
        Daylight.OnSunrise += LightsOff;
        Daylight.OnSunset += LightsOn;

        if (Daylight.isDay)
            LightsOff();
        else
            LightsOn();
    }

    void OnDestroy()
    {
        Daylight.OnSunrise -= LightsOff;
        Daylight.OnSunset -= LightsOn;
    }

    protected virtual void LightsOn()
    {
        foreach (var light in lights)
            light.enabled = true;
    }

    protected virtual void LightsOff()
    {
        foreach (var light in lights)
            light.enabled = false;
    }
}
