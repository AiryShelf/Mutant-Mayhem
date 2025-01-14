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
