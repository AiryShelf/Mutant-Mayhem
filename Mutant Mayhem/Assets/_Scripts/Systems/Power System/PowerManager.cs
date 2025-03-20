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
    [SerializeField] float timeToCheckPower;
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

        RecalculateNeighborBonuses();

        if (!initialized) return;

        CalculatePower(true);

        if (cutPowerCoroutine != null)
            StopCoroutine(cutPowerCoroutine);
        
        cutPowerCoroutine = StartCoroutine(CutConsumers());
    }

    public void RemovePowerSource(PowerSource source)
    {
        if (source == null) return;

        powerSources.Remove(source);

        RecalculateNeighborBonuses();

        if (!initialized) return;

        CalculatePower(true);

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
            yield return null;
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

    private void RecalculateNeighborBonuses()
    {
        // 1) Reset any neighbor-bonus portion of "powerGenerated" on all sources.
        foreach (var src in powerSources)
        {
            // *** No change here except clarifying comment. ***
            src.ResetNeighborBonus(); 
        }
        
        // 2) For each power source, check all the grid cells it occupies.
        foreach (var sourceA in powerSources)
        {
            var gridPos = TileManager.Instance.WorldToGrid(sourceA.transform.position);
            var sourceACells = new List<Vector3Int>(TileManager.Instance.GetStructurePositions(TileManager.AnimatedTilemap, gridPos));
            var sourceAType = sourceA.myStructureType;

            // For each cell that belongs to this power source...
            List<Vector3Int> foundDirections = new List<Vector3Int>();
            foreach (var cell in sourceACells)
            {
                // 3) Check neighbors in the 4 directions. (Up, Down, Left, Right)
                Vector3Int[] offsets = new Vector3Int[]
                {
                    Vector3Int.up,
                    Vector3Int.down,
                    Vector3Int.left,
                    Vector3Int.right
                };

                foreach (var offset in offsets)
                {
                    if (foundDirections.Contains(offset)) continue;

                    var neighborCell = cell + offset;

                    // Skip if neighborCell is one of the same source's own cells 
                    // (i.e., we don’t want to add a bonus for adjacency to itself).
                    if (sourceACells.Contains(neighborCell)) 
                    {
                        //Debug.Log("PowerManager: Check found source's occupied position, continue");
                        continue;
                    }

                    // 4) Query the TileManager for that neighbor cell’s StructureType.
                    var worldPos = TileManager.Instance.GridToWorld(neighborCell);
                    var structure = TileManager.Instance.GetStructureAt(worldPos);
                    //Debug.Log("PowerManager: structure is " + structure);
                    StructureType neighborType = StructureType.None;
                    if (structure != null)
                        neighborType = structure.structureType;
                    if (neighborType == sourceAType)
                    {
                        var rootPos = TileManager.Instance.GridToRootPos(neighborCell);
                        var obj = TileManager.StructureTilemap.GetInstantiatedObject(rootPos);
                        //Debug.Log("PowerManager: object is " + obj);

                        PowerSource sourceB = null;
                        if (obj != null)
                            sourceB = obj.GetComponent<PowerSource>();
                        // If found, add neighbor bonus to both sides
                        if (sourceB != null)
                        {
                            //Debug.Log("PowerManager: Adding neighbor bonus in direction " + offset);
                            sourceA.AddNeighborBonus();
                            //sourceB.AddNeighborBonus();
                            foundDirections.Add(offset);
                        }
                    }
                }
            }
        }
    }

    #endregion
}
