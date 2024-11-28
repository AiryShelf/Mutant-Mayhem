using System;
using System.Collections;
using UnityEngine;

public class Task_DeselectRepairGun : Task
{
    [SerializeField] int repairGunIndex;
    Player player;
    bool wasSelected = false;

    void Start()
    {
        player = FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogError("Objective_SelectGun could not find the Player");
        }

        UpdateProgressText();
    }

    void FixedUpdate()
    {
        if (isComplete) 
            return;

        if (player.stats.playerShooter.currentGunIndex == repairGunIndex)
        {
            wasSelected = true;
        }
        else if (wasSelected)
        {
            progress = 1;
            SetTaskComplete();
        }

        UpdateProgressText();
    }
}
