using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Task_BuildWalls : Task
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

        numberBuilt = StatsCounterPlayer.WallsBuilt;
        progress = (float)numberBuilt / numberToBuild;
        if (progress >= 1)
            SetTaskComplete();

        UpdateProgressText();
    }
}
