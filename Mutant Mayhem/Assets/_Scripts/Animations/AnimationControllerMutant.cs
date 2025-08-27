using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationControllerMutant : AnimationControllerEnemy
{
    public SpriteRenderer bodySR;
    public Animator leftLegAnimator;
    SpriteRenderer leftLegSR;
    public Animator rightLegAnimator;
    SpriteRenderer rightLegSR;
    EnemyMutant myMutant;

    protected override void Start()
    {
        myMutant = GetComponent<EnemyMutant>();
        leftLegSR = leftLegAnimator.GetComponent<SpriteRenderer>();
        rightLegSR = rightLegAnimator.GetComponent<SpriteRenderer>();

        if (myMutant != null)
        {
            baseSpeed = myMutant.moveSpeedBase;
            myRb = myMutant.GetRigidbody();
        }
    }

    protected override void UpdateEnemy()
    {
        if (myMutant != null)
        {
            float speed = myRb.velocity.magnitude;

            if (GameTools.AnimatorHasParameter(leftLegAnimator, "isRunning"))
            {
                if (speed > baseSpeed * switchToRunBuffer)
                {
                    leftLegAnimator.SetBool("isRunning", true);
                    rightLegAnimator.SetBool("isRunning", true);
                }
                else
                {
                    leftLegAnimator.SetBool("isRunning", false);
                    rightLegAnimator.SetBool("isRunning", false);
                }
            }

            if (GameTools.AnimatorHasParameter(leftLegAnimator, "isJumping"))
            {
                if (leftLegAnimator.GetBool("isSitting") || leftLegAnimator.GetBool("isJumping"))
                {
                    leftLegAnimator.speed = 1;
                    rightLegAnimator.speed = 1;
                }
                else
                {
                    float animSpeed = speed * animSpeedFactor;
                    animSpeed = Mathf.Clamp(animSpeed, 0, maxAnimSpeed);
                    leftLegAnimator.speed = animSpeed;
                    rightLegAnimator.speed = animSpeed;
                }
            }
        }
    }

    public override void SetSitAnimation(bool isSitting)
    {
        leftLegAnimator.SetBool("isSitting", isSitting);
        rightLegAnimator.SetBool("isSitting", isSitting);
    }

    public override void SetJumpAnimation(bool isJumping)
    {
        leftLegAnimator.SetBool("isJumping", isJumping);
        rightLegAnimator.SetBool("isJumping", isJumping);
        SetSpriteLayerToFlying(isJumping);
    }

    public override void SetSpriteLayerToFlying(bool isFlying)
    {
        if (isFlying)
        {
            bodySR.sortingLayerName = "FireParticles";
            leftLegSR.sortingLayerName = "FireParticles";
            rightLegSR.sortingLayerName = "FireParticles";
        }
        else
        {
            bodySR.sortingLayerName = "Enemy";
            leftLegSR.sortingLayerName = "Enemy";
            rightLegSR.sortingLayerName = "Enemy";
        }
    }
}
