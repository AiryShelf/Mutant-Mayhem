using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BallisticsBay : MonoBehaviour, IPowerConsumer, ITileObjectExplodable
{
    [SerializeField] StructureSO ballisticsBaySO;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] List<Light2D> lights;

    public string explosionPoolName;

    public void Explode()
    {
        if (!string.IsNullOrEmpty(explosionPoolName))
        {
            GameObject explosion = PoolManager.Instance.GetFromPool(explosionPoolName);
            Vector3Int rootPos = TileManager.Instance.WorldToGrid(transform.position);
            rootPos = TileManager.Instance.GridToRootPos(rootPos);
            explosion.transform.position = TileManager.Instance.TileCellsCenterToWorld(rootPos);
        }
    }

    void OnDestroy()
    {
        UpgradePanelManager.Instance.ClosePanel(StructureType.BallisticsBay);
    }
    
    public void PowerOn()
    {
        spriteRenderer.enabled = true;
        foreach (var light in lights)
            light.gameObject.SetActive(true);

        BuildingSystem.Instance.UnlockStructures(ballisticsBaySO, false);

    }

    public void PowerOff()
    {
        spriteRenderer.enabled = false;
        foreach (var light in lights)
            light.gameObject.SetActive(false);

        BuildingSystem.Instance.LockStructures(ballisticsBaySO, false);
    }
}
