using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class EngineeringBay : MonoBehaviour, IPowerConsumer, ITileObject
{
    [SerializeField] StructureSO engineeringBaySO;
    [SerializeField] List<AnimatedTile> powerOnDamageTiles;
    [SerializeField] List<AnimatedTile> powerOffDamageTiles;
    [SerializeField] List<Light2D> lights;

    float healthRatio;
    int damageIndex;

    void OnDestroy()
    {
        UpgradePanelManager.Instance.ClosePanel(StructureType.EngineeringBay);
    }

    public void PowerOn()
    {
        Vector3Int rootPos = TileManager.Instance.WorldToGrid(transform.position);
        rootPos = TileManager.Instance.GridToRootPos(rootPos);

        damageIndex = Mathf.FloorToInt(powerOnDamageTiles.Count * healthRatio);
        TileManager.AnimatedTilemap.SetTile(rootPos, powerOnDamageTiles[damageIndex]);

        SwitchLights(true);
        BuildingSystem.Instance.UnlockStructures(engineeringBaySO, false);
    }

    public void PowerOff()
    {
        Vector3Int rootPos = TileManager.Instance.WorldToGrid(transform.position);
        rootPos = TileManager.Instance.GridToRootPos(rootPos);

        damageIndex = Mathf.FloorToInt(powerOffDamageTiles.Count * healthRatio);
        TileManager.AnimatedTilemap.SetTile(rootPos, powerOffDamageTiles[damageIndex]);

        SwitchLights(false);
        BuildingSystem.Instance.LockStructures(engineeringBaySO, false);
    }

    public void UpdateHealthRatio(float healthRatio)
    {
        this.healthRatio = healthRatio;
    }

    void SwitchLights(bool on)
    {
        foreach (var light in lights)
            light.gameObject.SetActive(on);
    }
}
