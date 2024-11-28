using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Task_UseLaserSword : Task
{
    [SerializeField] int timesToUse;
    int timesUsedAtStart;

    void Start()
    {
        UpdateProgressText();

        timesUsedAtStart = StatsCounterPlayer.MeleeAttacksByPlayer;
    }

    void FixedUpdate()
    {
        if (isComplete) 
            return;

        progress = ((float)StatsCounterPlayer.MeleeAttacksByPlayer - timesUsedAtStart) / timesToUse;
        if (progress >= 1)
            SetTaskComplete();

        UpdateProgressText();
    }
}
