using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSource : MonoBehaviour
{
    public StructureSO structure;
    int startPower;
    public int powerGenerated { get; private set; }
    int neighborBonus;
    public ConnectorBase myConnectorBase;

    void Start()
    {
        if (structure != null)
        {
            powerGenerated = structure.powerGenerated;
            neighborBonus = structure.powerNeighborBonus;
        }
        startPower = powerGenerated;

        PowerManager.Instance.AddPowerSource(this); 
    }

    void OnDestroy()
    {
        if (PowerManager.Instance != null)
            PowerManager.Instance.RemovePowerSource(this);       
    }

    public void AddNeighborBonus()
    {
        powerGenerated += neighborBonus;
    }

    public void RemoveNeighborBonus()
    {
        powerGenerated -= neighborBonus;
    }

    public void ResetNeighborBonus()
    {
        powerGenerated = startPower;
    }

    public void SetNeighborBonus(int neighborBonus)
    {
        powerGenerated = startPower + neighborBonus;
    }
}
