using System;
using System.Collections;
using UnityEngine;

public class Task_SellGuts : Task
{
    [SerializeField] int valueToSell;
    int creditsAtStart;

    void Start()
    {
        UpdateProgressText();

        creditsAtStart = (int)BuildingSystem.PlayerCredits;
    }

    void FixedUpdate()
    {
        if (isComplete) 
            return;

        float difference = BuildingSystem.PlayerCredits - creditsAtStart;
        progress = difference / valueToSell;
        if (progress >= 1)
            SetTaskComplete();

        UpdateProgressText();
    }
}
