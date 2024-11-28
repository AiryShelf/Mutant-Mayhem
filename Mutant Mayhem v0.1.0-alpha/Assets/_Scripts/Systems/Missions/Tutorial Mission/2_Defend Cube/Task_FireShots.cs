using System;
using System.Collections;
using UnityEngine;

public class Task_FireShots : Task
{
    [SerializeField] int shotsToFire;
    int timesUsedAtStart;

    void Start()
    {
        UpdateProgressText();

        timesUsedAtStart = StatsCounterPlayer.ShotsFiredByPlayer;
    }

    void FixedUpdate()
    {
        if (isComplete) 
            return;

        progress = ((float)StatsCounterPlayer.ShotsFiredByPlayer - timesUsedAtStart) / shotsToFire;
        if (progress >= 1)
            SetTaskComplete();

        UpdateProgressText();
    }
}
