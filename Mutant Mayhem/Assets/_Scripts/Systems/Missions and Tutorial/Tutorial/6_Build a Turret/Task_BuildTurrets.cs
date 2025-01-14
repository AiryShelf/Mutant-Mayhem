using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Task_BuildTurrets : Task
{
    [SerializeField] int numberToBuild;
    int numberBuilt;

    void Start()
    {
        UpdateProgressText();
    }

    void FixedUpdate()
    {
        if (isComplete) 
            return;

        numberBuilt = StatsCounterPlayer.TurretsBuilt;
        progress = (float)numberBuilt / numberToBuild;
        if (progress >= 1)
            SetTaskComplete();

        UpdateProgressText();
    }
}
