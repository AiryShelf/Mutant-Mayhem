using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightTile : MonoBehaviour, ITileObjectExplodable
{
    [SerializeField] protected List<Light2D> lights;

    public string explosionPoolName;

    public void Explode()
    {
        if (!string.IsNullOrEmpty(explosionPoolName))
        {
            GameObject explosion = PoolManager.Instance.GetFromPool(explosionPoolName);
            Vector3Int rootPos = TileManager.Instance.WorldToGrid(transform.position);
            explosion.transform.position = TileManager.Instance.TileCellsCenterToWorld(rootPos);
        }
    }

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
