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

        amountRepairedAtStart = StatsCounterPlayer.AmountRepaired;
    }

    void FixedUpdate()
    {
        if (isComplete) 
            return;

        progress = (StatsCounterPlayer.AmountRepaired - amountRepairedAtStart) / amountToRepair;
        if (progress >= 1)
            SetTaskComplete();

        UpdateProgressText();
    }
}
