using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SupplyDepot : MonoBehaviour, IPowerConsumer
{
    [SerializeField] StructureSO supplyDepotSO;
    [SerializeField] SpriteRenderer spriteLightSprite;
    [SerializeField] List<Light2D> lights;

    public void PowerOn()
    {
        spriteLightSprite.enabled = true;
        foreach (var light in lights)
            light.gameObject.SetActive(true);

        BuildingSystem.Instance.UnlockStructures(supplyDepotSO, false);
        StopAllCoroutines();
    }

    public void PowerOff()
    {
        spriteLightSprite.enabled = false;
        foreach (var light in lights)
            light.gameObject.SetActive(false);

        BuildingSystem.Instance.LockStructures(supplyDepotSO, false);
        StopAllCoroutines();
    }
}
