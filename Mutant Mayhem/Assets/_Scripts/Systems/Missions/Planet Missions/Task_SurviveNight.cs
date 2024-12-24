using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Task_SurviveNight : Task
{
    [Header("Survive Night")]
    [SerializeField] int nightToComplete;
    WaveControllerRandom waveController;

    void Start()
    {
        waveController = FindObjectOfType<WaveControllerRandom>();

        UpdateProgressText();
    }

    void FixedUpdate()
    {
        if (isComplete) 
            return;

        progress = (float)waveController.currentWaveIndex / nightToComplete;

        if (waveController.currentWaveIndex + 1 > nightToComplete)
        {
            progress = 1;
            SetTaskComplete();
        }

        UpdateProgressText();
    }
}
