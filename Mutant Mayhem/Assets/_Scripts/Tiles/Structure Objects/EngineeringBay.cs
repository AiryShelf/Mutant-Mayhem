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

        damageIndex = Mathf.FloorToInt(powerOffDamageTiles.Count * healthRatio);

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
}
