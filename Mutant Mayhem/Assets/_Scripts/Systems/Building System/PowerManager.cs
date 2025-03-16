using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerManager : MonoBehaviour
{
    public static PowerManager Instance;
    public event Action<int> OnPowerChanged;

    public List<PowerSource> powerSources;
    public List<PowerConsumer> powerConsumers;

    public int powerBalance;
    public int powerAvailable;
    public int powerConsumed;
    public int powerTotal;
    public int powerDemand;
    public int powerCut;
    [SerializeField] float timeToCheckPower;
    [SerializeField] float cutTimeMax = 15;
    [SerializeField] float cutTimeMin = 5;

    Coroutine cutPowerCoroutine;
    List<PowerConsumer> consumersCut = new List<PowerConsumer>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        StartCoroutine(CheckPower());
    }

    public void AddPowerSource(PowerSource source)
    {
        powerSources.Add(source);
        powerTotal += source.powerGenerated;

        if (cutPowerCoroutine != null)
            StopCoroutine(cutPowerCoroutine);
        
        cutPowerCoroutine = StartCoroutine(CutConsumers());

        CalculatePower();
    }

    public void RemovePowerSource(PowerSource source)
    {
        powerSources.Remove(source);
        powerTotal -= source.powerGenerated;

        if (cutPowerCoroutine != null)
            StopCoroutine(cutPowerCoroutine);
        
        cutPowerCoroutine = StartCoroutine(CutConsumers());

        CalculatePower();
    }

    public void AddPowerConsumer(PowerConsumer consumer)
    {
        if (powerConsumers.Contains(consumer)) return;
        
        powerConsumers.Add(consumer);
        powerConsumed += consumer.powerConsumed;
        powerDemand += consumer.powerConsumed;

        if (cutPowerCoroutine != null)
            StopCoroutine(cutPowerCoroutine);
        
        cutPowerCoroutine = StartCoroutine(CutConsumers());
        

        CalculatePower();
    }

    public void RemovePowerConsumer(PowerConsumer consumer)
    {
        powerConsumers.Remove(consumer);
        powerConsumed -= consumer.powerConsumed;
        powerDemand -= consumer.powerConsumed;

        if (cutPowerCoroutine != null)
            StopCoroutine(cutPowerCoroutine);
        
        cutPowerCoroutine = StartCoroutine(CutConsumers());

        CalculatePower();
    }

    void CalculatePower()
    {
        powerAvailable = powerTotal - powerConsumed;
        powerBalance = powerTotal - powerDemand;
        
        OnPowerChanged?.Invoke(powerBalance);
    }

    IEnumerator CheckPower()
    {
        while (true)
        {
            yield return new WaitForSeconds(2);

            if (cutPowerCoroutine == null && powerConsumed > powerTotal)
                cutPowerCoroutine = StartCoroutine(CutConsumers());
        }
    }

    #region Power Cuts

    IEnumerator CutConsumers()
    {
        // Cut consumers until power is balanced
        while (powerConsumed > powerTotal)
        {
            // Randomly select a consumer to cut
            int index = UnityEngine.Random.Range(0, powerConsumers.Count);
            CutConsumer(powerConsumers[index]);
            
            // Yield to allow state update
            //yield return null;
        }

        // Check if any cut consumers can be restored to better balance power
        consumersCut.Sort((a, b) => a.powerConsumed.CompareTo(b.powerConsumed));
        List<PowerConsumer> restoredConsumers = new List<PowerConsumer>();
        foreach (var consumer in consumersCut)
        {
            if (powerConsumed + consumer.powerConsumed <= powerTotal)
            {
                consumer.PowerOn();
                powerConsumed += consumer.powerConsumed;
                powerConsumers.Add(consumer);
                restoredConsumers.Add(consumer);
            }
        }
        foreach (var consumer in restoredConsumers)
        {
            consumersCut.Remove(consumer);
        }
        
        CalculatePower();
        
        float cutTime = UnityEngine.Random.Range(cutTimeMin, cutTimeMax);
        yield return new WaitForSeconds(cutTime);
        
        // Restore any remaining cut consumers after waiting
        RestoreCutPower();
        
        // If still unbalanced, continue cutting
        if (powerConsumed > powerTotal)
            cutPowerCoroutine = StartCoroutine(CutConsumers());
    }

    void CutConsumer(PowerConsumer consumer)
    {
        consumer.PowerOff();
        powerConsumed -= consumer.powerConsumed;
        consumersCut.Add(consumer);
        powerConsumers.Remove(consumer);

        CalculatePower();
    }

    void RestoreCutPower()
    {
        foreach(var consumer in consumersCut)
        {
            consumer.DelayPowerOn();
            powerConsumed += consumer.powerConsumed;
            powerConsumers.Add(consumer);
        }

        consumersCut.Clear();

        CalculatePower();
    }

    #endregion
}
