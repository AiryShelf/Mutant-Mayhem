using System;
using System.Collections;
using UnityEngine;

public class Task_Aim : Task
{
    [SerializeField] float distToAim;
    float distanceAimed = 0;
    Vector2 cursorPreviousPos;

    bool initialized;

    void Start()
    {
        UpdateProgressText();
        StartCoroutine(Initialize());
    }

    IEnumerator Initialize()
    {
        yield return new WaitForSecondsRealtime(2);

        initialized = true;
        cursorPreviousPos = CursorManager.Instance.GetCustomCursorWorldPos();
    }

    void FixedUpdate()
    {
        if (isComplete || !initialized) 
            return;

        distanceAimed += Vector2.Distance(cursorPreviousPos, CursorManager.Instance.GetCustomCursorWorldPos());
        progress = distanceAimed / distToAim;
        //progress = ((float)StatsCounterPlayer.ShotsFiredByPlayer - timesUsedAtStart) / shotsToFire;
        if (progress >= 1)
            SetTaskComplete();

        UpdateProgressText();
        cursorPreviousPos = CursorManager.Instance.GetCustomCursorWorldPos();
    }
}
