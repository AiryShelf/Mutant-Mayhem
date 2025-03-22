using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSource : MonoBehaviour
{
    public StructureSO structure;
    public int startPower;
    public int powerGenerated;
    public int neighborBonus;
    public StructureType myStructureType;

    public List<Vector3Int> occupiedCells = new List<Vector3Int>();

    void Start()
    {
        if (structure != null)
        {
            powerGenerated = structure.powerGenerated;
            neighborBonus = structure.powerNeighborBonus;
        }
        startPower = powerGenerated;

        var gridPos = TileManager.Instance.WorldToGrid(transform.position);
        occupiedCells = TileManager.Instance.GetStructurePositions(TileManager.StructureTilemap, gridPos);

        PowerManager.Instance.AddPowerSource(this); 
    }

    void OnDestroy()
    {
        if (PowerManager.Instance != null)
            PowerManager.Instance.RemovePowerSource(this);       
    }

    public void AddNeighborBonus()
    {
        //powerGenerated += neighborBonus;
    }

    public void ResetNeighborBonus()
    {
        //powerGenerated = startPower;
    }

    public void SetNeighborBonus(int neighborBonus)
    {
        powerGenerated = startPower + neighborBonus;
    }
}
