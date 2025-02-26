using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Task_Move : Task
{
    [SerializeField] float timeToMove = 4;
    float moveTimer;
    Player player;

    void Start()
    {
        player = FindObjectOfType<Player>();
        moveTimer = timeToMove;
        UpdateProgressText();
    }

    void FixedUpdate()
    {
        if (isComplete) 
            return;

        if (player.rawInput.magnitude > 0)
        {
            moveTimer -= Time.fixedDeltaTime;
            moveTimer = Mathf.Clamp(moveTimer, 0, float.MaxValue);
        }
        

        progress = (timeToMove - moveTimer) / timeToMove;
        if (progress >= 1)
            SetTaskComplete();

        UpdateProgressText();
    }
}

