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
    public List<PowerConsumer> currentConsumers;

    public int powerBalance;
    public int powerAvailable;
    public int powerConsumed;
    public int powerTotal;
    public int powerDemand;
    public int powerCut;
    [SerializeField] float cutTimeMax = 15;
    [SerializeField] float cutTimeMin = 5;

    Coroutine cutPowerCoroutine;
    List<PowerConsumer> consumersCut = new List<PowerConsumer>();
    bool initialized = false;

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
        StartCoroutine(DelayInitialize());
    }

    IEnumerator DelayInitialize()
    {
        yield return new WaitForFixedUpdate();

        initialized = true;
        CalculatePower(true);
        OnPowerChanged?.Invoke(powerBalance);
        StartCoroutine(CheckPower());
    }

    public void AddPowerSource(PowerSource source)
    {
        if (source == null) return;
        
        powerSources.Add(source);

        StartCoroutine(RecalculateNeighborBonuses());

        if (!initialized) return;

        if (cutPowerCoroutine != null)
            StopCoroutine(cutPowerCoroutine);
        
        cutPowerCoroutine = StartCoroutine(CutConsumers());
    }

    public void RemovePowerSource(PowerSource source)
    {
        if (source == null) 
        {
            Debug.LogError("PowerManager: Tired to remove null power source");
            return;
        }

        powerSources.Remove(source);

        StartCoroutine(RecalculateNeighborBonuses());

        if (!initialized) return;

        if (cutPowerCoroutine != null)
            StopCoroutine(cutPowerCoroutine);
        
        cutPowerCoroutine = StartCoroutine(CutConsumers());
    }

    public void AddPowerConsumer(PowerConsumer consumer)
    {
        if (consumer == null || powerConsumers.Contains(consumer)) return;
        
        powerConsumers.Add(consumer);
        currentConsumers.Add(consumer);

        powerConsumed += consumer.powerConsumed;
        powerDemand += consumer.powerConsumed;

        if (!initialized) return;

        CalculatePower(false);

        if (cutPowerCoroutine != null)
            StopCoroutine(cutPowerCoroutine);
        
        cutPowerCoroutine = StartCoroutine(CutConsumers());
    }

    public void RemovePowerConsumer(PowerConsumer consumer)
    {
        if (consumer == null) return;

        powerConsumers.Remove(consumer);
        currentConsumers.Remove(consumer);

        if (consumersCut.Contains(consumer))
            consumersCut.Remove(consumer);
        else
            powerConsumed -= consumer.powerConsumed;
        powerDemand -= consumer.powerConsumed;

        if (!initialized) return;

        CalculatePower(false);

        if (cutPowerCoroutine != null)
            StopCoroutine(cutPowerCoroutine);
        
        cutPowerCoroutine = StartCoroutine(CutConsumers());
    }

    void CalculatePower(bool checkSources)
    {
        if (checkSources)
        {
            powerTotal = 0;
            foreach (var source in powerSources)
                powerTotal += source.powerGenerated;
        }

        powerAvailable = powerTotal - powerConsumed;
        powerBalance = powerTotal - powerDemand;
        
        OnPowerChanged?.Invoke(powerBalance);
    }

    IEnumerator CheckPower()
    {
        while (true)
        {
            yield return new WaitForSeconds(2);

            //if (cutPowerCoroutine == null && powerConsumed > powerTotal)
                //cutPowerCoroutine = StartCoroutine(CutConsumers());
        }
    }

    #region Power Cuts

    IEnumerator CutConsumers()
    {
        yield return null;

        if (powerConsumed > powerTotal)
            MessagePanel.PulseMessage("WARNING: Power Outages!", Color.red);

        // Cut consumers until power is balanced
        while (powerConsumed > powerTotal && currentConsumers.Count > 0)
        {
            // Randomly select a consumer to cut
            int index = UnityEngine.Random.Range(0, currentConsumers.Count);
            CutConsumer(currentConsumers[index]);
            
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
                currentConsumers.Add(consumer);
                restoredConsumers.Add(consumer);
            }
        }
        foreach (var consumer in restoredConsumers)
        {
            consumersCut.Remove(consumer);
        }
        
        CalculatePower(false);
        
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
        currentConsumers.Remove(consumer);

        CalculatePower(false);
    }

    void RestoreCutPower()
    {
        foreach(var consumer in consumersCut)
        {
            consumer.PowerOn();
            powerConsumed += consumer.powerConsumed;
            currentConsumers.Add(consumer);
        }

        consumersCut.Clear();

        CalculatePower(false);
    }

    #endregion

    #region  Neighbor Bonus

    
    IEnumerator RecalculateNeighborBonuses()
    {
        yield return null;
        foreach (var source in powerSources)
        {
            if (source.myConnectorBase != null)
                source.myConnectorBase.CheckConnections(false);
        }
        CalculatePower(true);
    }

    #endregion
}
