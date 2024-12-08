using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Task_RepairTheCube : Task
{
    QCubeHealth cubeHealth;
    float healthAtStart;

    void Start()
    {
        cubeHealth = FindObjectOfType<QCubeHealth>();
        if (cubeHealth == null)
        {
            Debug.LogError("Objective could not find QCubeHealth");
            return;
        }

        healthAtStart = cubeHealth.GetHealth();
        if (healthAtStart == cubeHealth.GetMaxHealth())
        {
            cubeHealth.ModifyHealth(-1, 1, Vector2.one, null);
            healthAtStart = cubeHealth.GetHealth();
        }

        UpdateProgressText();
    }

    void FixedUpdate()
    {
        if (isComplete) 
            return;


        if (cubeHealth.GetHealth() > healthAtStart)
        {
            progress = 1;
            SetTaskComplete();
        }

        UpdateProgressText();

        healthAtStart = cubeHealth.GetHealth();
    }
}
