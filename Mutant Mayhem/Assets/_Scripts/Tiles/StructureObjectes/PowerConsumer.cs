using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PowerConsumer : MonoBehaviour
{
    public int powerConsumed = 1;
    [SerializeField] SpriteRenderer noPowerIcon;
    public bool isOn = true;
    public UnityEvent onPowerOn;
    public UnityEvent onPowerOff;

    void Start()
    {
        PowerManager.Instance.AddPowerConsumer(this);
    }

    void OnDestroy()
    {
        PowerManager.Instance.RemovePowerConsumer(this);
    }

    public virtual void TurnOn() 
    { 
        noPowerIcon.enabled = false;
        isOn = true;
        if (onPowerOn != null)
            onPowerOn.Invoke();
    }

    public virtual void TurnOff() 
    { 
        noPowerIcon.enabled = true;
        isOn = false;
        if (onPowerOff != null)
            onPowerOff.Invoke();
    }
}
