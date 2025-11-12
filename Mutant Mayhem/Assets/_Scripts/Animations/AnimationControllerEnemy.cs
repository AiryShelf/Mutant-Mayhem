using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationControllerEnemy : MonoBehaviour
{
    public float animSpeedFactor = 1f;
    public float switchToRunBuffer = 1.1f;
    public float maxAnimSpeed = 10;

    public Animator myAnimator;
    SpriteRenderer mySR;
    EnemyBase enemyBase;
    protected Rigidbody2D myRb;

    protected virtual void Start()
    {
        enemyBase = GetComponent<EnemyBase>();
        myAnimator = GetComponentInChildren<Animator>();
        mySR = myAnimator.GetComponent<SpriteRenderer>();

        if (enemyBase != null)
        {
            myRb = enemyBase.GetComponent<Rigidbody2D>();
        }
    }

    protected void FixedUpdate()
    {
        UpdateEnemy();
    }

    protected virtual void UpdateEnemy()
    {
        if (enemyBase != null)
        {
            float sprSpeed = myRb.velocity.sqrMagnitude;

            if (GameTools.AnimatorHasParameter(myAnimator, "isRunning"))
            {
                if (sprSpeed > switchToRunBuffer * switchToRunBuffer)
                {
                    myAnimator.SetBool("isRunning", true);
                }
                else
                {
                    myAnimator.SetBool("isRunning", false);
                }
            }

            float animSpeed = Mathf.Sqrt(sprSpeed) * animSpeedFactor;
            animSpeed = Mathf.Clamp(animSpeed, 0, maxAnimSpeed);
            myAnimator.speed = animSpeed;

            if (GameTools.AnimatorHasParameter(myAnimator, "isJumping"))
            {
                if (myAnimator.GetBool("isSitting") || myAnimator.GetBool("isJumping"))
                {
                    myAnimator.speed = 1;
                }
            }
        }
    }

    public virtual void SetSitAnimation(bool isSitting)
    {
        // Check if animator has parameter
        if (GameTools.AnimatorHasParameter(myAnimator, "isSitting"))
        {
            myAnimator.SetBool("isSitting", isSitting);
        }
    }

    public virtual void SetJumpAnimation(bool isJumping)
    {
        if (GameTools.AnimatorHasParameter(myAnimator, "isJumping"))
        {
            myAnimator.SetBool("isJumping", isJumping);
        }
        SetSpriteLayerToFlying(isJumping);
    }

    public virtual void SetSpriteLayerToFlying(bool isFlying)
    {
        if (isFlying)
        {
            mySR.sortingLayerName = "FireParticles";
        }
        else
        {
            mySR.sortingLayerName = "Enemy";
        }
    }
}
