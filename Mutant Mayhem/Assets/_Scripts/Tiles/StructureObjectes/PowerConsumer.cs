using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PowerConsumer : MonoBehaviour
{
    public int powerConsumed = 1;
    public SpriteRenderer noPowerIcon;
    public bool isOn = true;
    public UnityEvent onPowerOn;
    public UnityEvent onPowerOff;

    Coroutine delayOnRoutine;

    void Start()
    {
        AddConsumer();
        //noPowerIcon.transform.rotation = Quaternion.identity;
    }

    void OnDestroy()
    {
        RemoveConsumer();
    }

    public void AddConsumer()
    {
        PowerManager.Instance.AddPowerConsumer(this);
        StartCoroutine(RotateIcon());
    }

    public void RemoveConsumer()
    {
        StopAllCoroutines();
        if (PowerManager.Instance != null)
            PowerManager.Instance.RemovePowerConsumer(this);
    }

    void FixedUpdate()
    {
        noPowerIcon.transform.rotation = Quaternion.identity;
    }

    IEnumerator RotateIcon()
    {
        yield return new WaitForFixedUpdate();

        
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
