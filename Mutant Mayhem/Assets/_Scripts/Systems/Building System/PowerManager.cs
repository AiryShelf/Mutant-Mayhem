using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerManager : MonoBehaviour
{
    public static PowerManager Instance;

    public List<PowerSource> powerSources;
    public List<PowerConsumer> powerConsumers;

    public int powerAvailable;
    public int powerConsumed;
    public int powerTotal;
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
        
    }

    public void AddPowerSource(PowerSource source)
    {
        powerSources.Add(source);
        powerTotal += source.powerGenerated;

        CalculatePower();
    }

    public void RemovePowerSource(PowerSource source)
    {
        powerSources.Remove(source);
        powerTotal -= source.powerGenerated;

        CalculatePower();
    }

    public void AddPowerConsumer(PowerConsumer consumer)
    {
        powerConsumers.Add(consumer);
        powerConsumed += consumer.powerConsumed;

        CalculatePower();

        if (powerConsumed > powerTotal)
        {
            CutConsumer(consumer);
        }
    }

    public void RemovePowerConsumer(PowerConsumer consumer)
    {
        powerConsumers.Remove(consumer);
        powerConsumed -= consumer.powerConsumed;

        CalculatePower();
    }

    void CalculatePower()
    {
        powerAvailable = powerTotal - powerConsumed;
    }

    IEnumerator CheckPower()
    {
        yield return new WaitForSeconds(2);

        if (cutPowerCoroutine == null && powerConsumed > powerTotal)
            cutPowerCoroutine = StartCoroutine(CutConsumers());

    }

    IEnumerator CutConsumers()
    {
        //yield return null;

        // Cut randomly until power balanced, hold for a random time
        while (powerConsumed > powerTotal)
        {
            int index = UnityEngine.Random.Range(0, powerConsumers.Count);
            CutConsumer(powerConsumers[index]);
        }

        float cutTime = UnityEngine.Random.Range(cutTimeMin, cutTimeMax);
        yield return new WaitForSeconds(cutTime);

        // Restore power, cut again.
        RestoreCutPower();

        if (powerConsumed > powerTotal)
            cutPowerCoroutine = StartCoroutine(CutConsumers());
    }

    void CutConsumer(PowerConsumer consumer)
    {
        consumer.CutPower();
        powerConsumed -= consumer.powerConsumed;
        consumersCut.Add(consumer);

        CalculatePower();
    }

    void RestoreCutPower()
    {
        foreach(var consumer in consumersCut)
        {
            consumer.RestorePower();
            powerConsumed += consumer.powerConsumed;
        }

        consumersCut.Clear();

        CalculatePower();
    }
}
