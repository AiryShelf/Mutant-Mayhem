using System;
using System.Collections;
using UnityEngine;

public class Task_PowerBalance : Task
{
    public int minimumPowerBalance = 0;

    void Start()
    {
        UpdateProgressText();
        StartCoroutine(CheckIntermittently());
    }

    void CheckComplete()
    {
        progress = PowerManager.Instance.powerBalance >= minimumPowerBalance ?
                   1 :
                   (float)PowerManager.Instance.powerBalance / minimumPowerBalance;

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
