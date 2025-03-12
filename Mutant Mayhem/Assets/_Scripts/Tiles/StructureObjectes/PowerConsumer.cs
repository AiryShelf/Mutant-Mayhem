using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerConsumer : MonoBehaviour
{
    public int powerConsumed = 1;
    [SerializeField] SpriteRenderer noPowerIcon;

    void Start()
    {
        PowerManager.Instance.AddPowerConsumer(this);
    }

    void OnDestroy()
    {
        PowerManager.Instance.RemovePowerConsumer(this);
    }

    public virtual void CutPower() 
    { 
        noPowerIcon.enabled = true;
    }

    public virtual void RestorePower() 
    { 
        noPowerIcon.enabled = false;
    }
}
