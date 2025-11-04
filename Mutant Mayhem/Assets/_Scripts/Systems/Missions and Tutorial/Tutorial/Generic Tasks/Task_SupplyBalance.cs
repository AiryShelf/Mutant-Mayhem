using System;
using System.Collections;
using UnityEngine;

public class Task_SupplyBalance : Task
{
    [Header("Task Settings")]
    public int minimumSupplyBalance = 0;

    void Start()
    {
        UpdateProgressText();
        StartCoroutine(CheckIntermittently());
    }

    void CheckComplete()
    {
        progress = SupplyManager.SupplyBalance >= minimumSupplyBalance ?
                   1 :
                   (float)SupplyManager.SupplyBalance / minimumSupplyBalance;

        if (progress >= 1)
            SetTaskComplete();
        else
            SetTaskIncomplete();

        UpdateProgressText();
    }
    
    IEnumerator CheckIntermittently()
    {
        while (true)
        {
            CheckComplete();
            yield return new WaitForSeconds(2f);
        }
    }
}
