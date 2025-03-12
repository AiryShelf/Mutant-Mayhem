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

    void Oestroy()
    {
        PowerManager.Instance.RemovePowerSource(this);       
    }
}
