using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SupplyDepot : MonoBehaviour, IPowerConsumer, ITileObjectExplodable
{
    [SerializeField] StructureSO supplyDepotSO;
    [SerializeField] SupplyProducer supplyProducer;
    [SerializeField] SpriteRenderer spriteLightSprite;
    [SerializeField] List<Light2D> lights;

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

    public void PowerOn()
    {
        spriteLightSprite.enabled = true;
        foreach (var light in lights)
            light.gameObject.SetActive(true);

        BuildingSystem.Instance.UnlockStructures(supplyDepotSO, false);
        StopAllCoroutines();
        supplyProducer.enabled = true;
    }

    public void PowerOff()
    {
        spriteLightSprite.enabled = false;
        foreach (var light in lights)
            light.gameObject.SetActive(false);

        BuildingSystem.Instance.LockStructures(supplyDepotSO, false);
        StopAllCoroutines();
        supplyProducer.enabled = false;
    }
}
