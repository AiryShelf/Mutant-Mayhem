using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Task_SkipDay : Task
{
    WaveController waveController;

    void Start()
    {
        waveController = FindObjectOfType<WaveController>();

        UpdateProgressText();
    }

    void FixedUpdate()
    {
        if (isComplete) 
            return;

        if (waveController.isNight == true)
        {
            progress = 1;
            SetTaskComplete();
        }

        UpdateProgressText();
    }
}
