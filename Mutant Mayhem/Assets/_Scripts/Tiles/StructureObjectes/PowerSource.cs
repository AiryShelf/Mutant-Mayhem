using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSource : MonoBehaviour
{
    public int powerGenerated = 5;
    public int neighborBonus = 1;

    void Start()
    {
        PowerManager.Instance.AddPowerSource(this); 
    }

    void OnDestroy()
    {
        PowerManager.Instance.RemovePowerSource(this);       
    }

    public void TurnOn()
    {

    }

    public void TurnOff()
    {

    }
}
