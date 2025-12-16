using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PhotonicsBay : MonoBehaviour, IPowerConsumer, ITileObjectExplodable
{
    [SerializeField] StructureSO photonicsBaySO;
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
        UpgradePanelManager.Instance.ClosePanel(StructureType.PhotonicsBay);
    }

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
