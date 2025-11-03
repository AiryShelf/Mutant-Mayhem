using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PowerConsumer : MonoBehaviour, IPowerConsumer
{
    public int powerConsumed = 1;
    public SpriteRenderer noPowerIcon;
    public bool isOn = true;
    public UnityEvent onPowerOn;
    public UnityEvent onPowerOff;

    Coroutine delayOnRoutine;

    void OnEnable()
    {
        AddConsumer();
    }

    void OnDisable()
    {
        RemoveConsumer();
    }

    public void AddConsumer()
    {
        if (PowerManager.Instance != null)
            PowerManager.Instance.AddPowerConsumer(this);
    }

    public void RemoveConsumer()
    {
        StopAllCoroutines();
        if (PowerManager.Instance != null)
            PowerManager.Instance.RemovePowerConsumer(this);
    }

    public virtual void PowerOn() 
    { 
        noPowerIcon.enabled = false;
        isOn = true;
        if (onPowerOn != null)
            onPowerOn.Invoke();
    }

    public void DelayPowerOn()
    {
        delayOnRoutine = StartCoroutine(DelayOn());
    }

    IEnumerator DelayOn()
    {
        yield return null;

        // This will only run when nullified externally, 
            // to prevent being turned on and back off again in the same frame  
        
        if (delayOnRoutine != null)
            PowerOn();

        delayOnRoutine = null;
    }

    public virtual void PowerOff() 
    { 
        delayOnRoutine = null;
        noPowerIcon.enabled = true;
        isOn = false;
        if (onPowerOff != null)
            onPowerOff.Invoke();
    }
}
