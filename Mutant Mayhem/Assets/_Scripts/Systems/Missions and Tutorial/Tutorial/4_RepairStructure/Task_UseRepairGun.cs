using System;
using System.Collections;
using UnityEngine;

public class Task_UseRepairGun : Task
{
    [SerializeField] int amountToRepair;
    float amountRepairedAtStart;

    void Start()
    {
        UpdateProgressText();

        amountRepairedAtStart = StatsCounterPlayer.AmountRepairedByPlayer;
        TileManager.Instance.ApplyDamageToStructuresOfType(StructureType.OneByOneWall, 100); // Pre-damage the walls for the tutorial
        TileManager.Instance.ApplyDamageToStructuresOfType(StructureType.OneByOneCorner, 50); // Pre-damage the walls for the tutorial
    }

    void FixedUpdate()
    {
        if (isComplete) 
            return;

        progress = (StatsCounterPlayer.AmountRepairedByPlayer - amountRepairedAtStart) / amountToRepair;
        if (progress >= 1)
            SetTaskComplete();

        UpdateProgressText();
    }
}
