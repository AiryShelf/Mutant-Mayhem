using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PhotonicsBay : MonoBehaviour, IPowerConsumer
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] List<Light2D> lights;

    public void PowerOn()
    {
        spriteRenderer.enabled = true;
        foreach (var light in lights)
            light.gameObject.SetActive(true);
    }

    public void PowerOff()
    {
        spriteRenderer.enabled = false;
        foreach (var light in lights)
            light.gameObject.SetActive(false);
    }
}
