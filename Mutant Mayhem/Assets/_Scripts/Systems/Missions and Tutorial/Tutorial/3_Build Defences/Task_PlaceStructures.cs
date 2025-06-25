using System;
using System.Collections;
using UnityEngine;

public class Task_PlaceStructures : Task
{
    [Header("Place Structures")]
    [SerializeField] StructureType structureType;
    [SerializeField] int numberToPlace;
    int numberPlaced;

    void Start()
    {
        UpdateProgressText();
    }

    void FixedUpdate()
    {
        if (isComplete) 
            return;

        numberPlaced = StatsCounterPlayer.GetStructuresPlacedByType(structureType);
        progress = (float)numberPlaced / numberToPlace;
        if (progress >= 1)
            SetTaskComplete();

        UpdateProgressText();
    }
}
