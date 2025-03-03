using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Task_Sprint : Task
{
    [SerializeField] float timeToSprint = 4;
    float sprintTimer;

    void Start()
    {
        sprintTimer = timeToSprint;
        UpdateProgressText();
    }

    void FixedUpdate()
    {
        if (isComplete) 
            return;
        
        sprintTimer = timeToSprint - StatsCounterPlayer.TimeSprintingPlayer;

        progress = (timeToSprint - sprintTimer) / timeToSprint;
        if (progress >= 1)
            SetTaskComplete();

        UpdateProgressText();
    }
}

