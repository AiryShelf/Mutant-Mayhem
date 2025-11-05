using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class EngineeringBay : MonoBehaviour, IPowerConsumer, ITileObject, ITileObjectExplodable
{
    [SerializeField] StructureSO engineeringBaySO;
    [SerializeField] List<AnimatedTile> powerOnDamageTiles;
    [SerializeField] List<AnimatedTile> powerOffDamageTiles;
    [SerializeField] List<Light2D> lights;

    public string explosionPoolName;

    public void Explode()
    {
        if (!string.IsNullOrEmpty(explosionPoolName))
        {
            GameObject explosion = PoolManager.Instance.GetFromPool(explosionPoolName);
            explosion.transform.position = transform.position;
        }
    }
    

    float healthRatio;
    int damageIndex;
    bool isPowerOn = false;

    void OnDestroy()
    {
        UpgradePanelManager.Instance.ClosePanel(StructureType.EngineeringBay);
    }

    public void PowerOn()
    {
        Debug.Log("Engineering Bay Power On");
        isPowerOn = true;
        UpdateHealthRatio(healthRatio);

        SwitchLights(true);
        BuildingSystem.Instance.UnlockStructures(engineeringBaySO, false);
    }

    public void PowerOff()
    {
        Debug.Log("Engineering Bay Power Off");
        isPowerOn = false;
        UpdateHealthRatio(healthRatio);

        SwitchLights(false);
        BuildingSystem.Instance.LockStructures(engineeringBaySO, false);
    }

    public void UpdateHealthRatio(float healthRatio)
    {
        this.healthRatio = healthRatio;

        Vector3Int rootPos = TileManager.Instance.WorldToGrid(transform.position);
        rootPos = TileManager.Instance.GridToRootPos(rootPos);
        if (rootPos == Vector3Int.zero)
            return;

        damageIndex = GetDamageIndex(healthRatio);

        if (isPowerOn)
            TileManager.AnimatedTilemap.SetTile(rootPos, powerOnDamageTiles[damageIndex]);
        else
            TileManager.AnimatedTilemap.SetTile(rootPos, powerOffDamageTiles[damageIndex]);
    }

    void SwitchLights(bool on)
    {
        foreach (var light in lights)
            light.gameObject.SetActive(on);
    }

    int GetDamageIndex(float healthRatio)
    {
        int damageIndex;
        int spriteCount = powerOnDamageTiles.Count;

        // Reserve index 0 for full health
        if (healthRatio >= 1f)
        {
            damageIndex = 0;
        }
        else
        {
            // Divide remaining indices (1 to spriteCount-1) across the 0–99% damage range
            float normalized = 1f - healthRatio;
            damageIndex = 1 + Mathf.FloorToInt(normalized * (spriteCount - 1));
            damageIndex = Mathf.Clamp(damageIndex, 1, spriteCount - 1);
        }

        return damageIndex;
    }
}
