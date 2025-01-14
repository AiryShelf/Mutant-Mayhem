using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Task_UseGrenade : Task
{
    [SerializeField] int timesToUse;
    int timesUsedAtStart;

    void Start()
    {
        Player player = FindObjectOfType<Player>();
        player.stats.grenadeAmmo++;
        UpdateProgressText();

        //timesUsedAtStart = StatsCounterPlayer.GrenadesThrownByPlayer;
    }

    void FixedUpdate()
    {
        if (isComplete) 
            return;

        progress = (float)StatsCounterPlayer.GrenadesThrownByPlayer / timesToUse;
        if (progress >= 1)
            SetTaskComplete();

        UpdateProgressText();
    }
}
