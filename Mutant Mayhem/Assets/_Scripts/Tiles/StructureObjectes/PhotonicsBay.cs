using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PhotonicsBay : MonoBehaviour, IPowerConsumer
{
    [SerializeField] StructureSO photonicsBaySO;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] List<Light2D> lights;

    public void PowerOn()
    {
        spriteRenderer.enabled = true;
        foreach (var light in lights)
            light.gameObject.SetActive(true);

        BuildingSystem.Instance.UnlockStructures(photonicsBaySO, false);
    }

    public void PowerOff()
    {
        spriteRenderer.enabled = false;
        foreach (var light in lights)
            light.gameObject.SetActive(false);

        BuildingSystem.Instance.LockStructures(photonicsBaySO, false);
    }
}
