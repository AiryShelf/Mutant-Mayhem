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
            explosion.transform.position = transform.position;
            Explosion explosionComp = explosion.GetComponent<Explosion>();
            if (explosionComp != null)
                explosionComp.Explode();
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
