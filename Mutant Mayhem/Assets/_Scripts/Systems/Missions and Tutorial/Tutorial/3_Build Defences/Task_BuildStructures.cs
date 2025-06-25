using System;
using System.Collections;
using UnityEngine;

public class Task_BuildStructures : Task
{
    [Header("Build Structures")]
    [SerializeField] StructureType structureType;
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

        numberBuilt = StatsCounterPlayer.GetStructuresBuiltByType(structureType);
        progress = (float)numberBuilt / numberToBuild;
        if (progress >= 1)
            SetTaskComplete();

        UpdateProgressText();
    }
}
