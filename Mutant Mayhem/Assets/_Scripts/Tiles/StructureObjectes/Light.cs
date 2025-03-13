using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Light : MonoBehaviour
{
    [SerializeField] protected List<Light2D> lights;

    protected virtual void Start()
    {
        Daylight.OnSunrise += TurnOff;
        Daylight.OnSunset += TurnOn;

        if (Daylight.isDay)
            TurnOff();
        else
            TurnOn();
    }

    void OnDestroy()
    {
        Daylight.OnSunrise -= TurnOff;
        Daylight.OnSunset -= TurnOn;
    }

    protected virtual void TurnOn()
    {
        foreach (var light in lights)
            light.enabled = true;
    }

    protected virtual void TurnOff()
    {
        foreach (var light in lights)
            light.enabled = false;
    }
}
