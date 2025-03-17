using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class EngineeringBay : MonoBehaviour, IPowerConsumer, ITileObject
{
    [SerializeField] StructureSO engineeringBaySO;
    [SerializeField] List<AnimatedTile> powerOnDamageTiles;
    [SerializeField] List<AnimatedTile> powerOffDamageTiles;
    [SerializeField] Light2D glow;

    float healthRatio;
    int damageIndex;

    public void PowerOn()
    {
        Vector3Int rootPos = TileManager.Instance.WorldToGrid(transform.position);
        rootPos = TileManager.Instance.GridToRootPos(rootPos);

        damageIndex = Mathf.FloorToInt(powerOnDamageTiles.Count * healthRatio);
        TileManager.AnimatedTilemap.SetTile(rootPos, powerOnDamageTiles[damageIndex]);

        glow.gameObject.SetActive(true);
        BuildingSystem.Instance.UnlockStructures(engineeringBaySO);
    }

    public void PowerOff()
    {
        Vector3Int rootPos = TileManager.Instance.WorldToGrid(transform.position);
        rootPos = TileManager.Instance.GridToRootPos(rootPos);

        damageIndex = Mathf.FloorToInt(powerOffDamageTiles.Count * healthRatio);
        TileManager.AnimatedTilemap.SetTile(rootPos, powerOffDamageTiles[damageIndex]);

        glow.gameObject.SetActive(false);
        BuildingSystem.Instance.LockStructures(engineeringBaySO);
    }

    public void UpdateHealthRatio(float healthRatio)
    {
        this.healthRatio = healthRatio;
    }
}
