using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSource : MonoBehaviour
{
    int basePower;
    public int powerGenerated = 5;
    public int neighborBonus = 1;
    public StructureType myStructureType;

    public List<Vector3Int> occupiedCells = new List<Vector3Int>();

    void Start()
    {
        basePower = powerGenerated;
        var gridPos = TileManager.Instance.WorldToGrid(transform.position);
        occupiedCells = TileManager.Instance.GetStructurePositions(TileManager.StructureTilemap, gridPos);

        PowerManager.Instance.AddPowerSource(this); 
    }

    void OnDestroy()
    {
        PowerManager.Instance.RemovePowerSource(this);       
    }

    public void AddNeighborBonus()
    {
        powerGenerated += neighborBonus;
    }

    public void ResetNeighborBonus()
    {
        powerGenerated = basePower;
    }
}
