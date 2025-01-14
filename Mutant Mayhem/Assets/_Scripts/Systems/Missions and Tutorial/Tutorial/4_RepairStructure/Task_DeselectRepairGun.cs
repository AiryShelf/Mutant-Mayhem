using System;
using System.Collections;
using UnityEngine;

public class Task_DeselectRepairGun : Task
{
    [SerializeField] int repairGunIndex;
    PlayerShooter playerShooter;
    bool wasSelected = false;

    void Start()
    {
        Player player = FindObjectOfType<Player>();
        playerShooter = player.stats.playerShooter;
        if (playerShooter == null)
        {
            Debug.LogError("Objective_DeselctRepairGun could not find the Player's shooter");
        }

        UpdateProgressText();
    }

    void FixedUpdate()
    {
        if (isComplete) 
            return;

        if (playerShooter.currentGunIndex == repairGunIndex)
        {
            wasSelected = true;
        }
        else if (wasSelected && playerShooter.currentGunIndex != repairGunIndex)
        {
            progress = 1;
            SetTaskComplete();
        }

        UpdateProgressText();
    }
}
