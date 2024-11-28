using System;
using System.Collections;
using UnityEngine;

public class Task_SelectGun : Task
{
    [SerializeField] int playerGunIndex;
    Player player;

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

        if (player.stats.playerShooter.currentGunIndex == playerGunIndex)
        {
            progress = 1;
            SetTaskComplete();
        }

        UpdateProgressText();
    }
}
