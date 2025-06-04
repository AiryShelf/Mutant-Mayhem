using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationControllerMutant : AnimationControllerEnemy
{
    public Animator leftLegAnimator;
    public Animator rightLegAnimator;
    EnemyMutant myMutant;

    protected override void Start()
    {
        myMutant = GetComponent<EnemyMutant>();

        if (myMutant != null)
        {
            baseSpeed = myMutant.moveSpeedBase;
            myRb = myMutant.GetComponent<Rigidbody2D>();
        }
    }

    protected override void UpdateEnemy()
    {
        if (myMutant != null)
        {
            float speed = myRb.velocity.magnitude;

            if (speed > baseSpeed * switchToRunBuffer)
            {
                //leftLegAnimator.SetBool("isRunning", true);
                //rightLegAnimator.SetBool("isRunning", true);
            }
            else
            {
                //leftLegAnimator.SetBool("isRunning", false);
                //rightLegAnimator.SetBool("isRunning", false);
            }
            float animSpeed = speed * animSpeedFactor;
            animSpeed = Mathf.Clamp(animSpeed, 0, maxAnimSpeed);
            leftLegAnimator.speed = animSpeed;
            rightLegAnimator.speed = animSpeed;
        }
    }
}
