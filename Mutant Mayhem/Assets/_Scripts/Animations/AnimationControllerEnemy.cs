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
    protected float baseSpeed;

    protected virtual void Start()
    {
        enemyBase = GetComponent<EnemyBase>();
        myAnimator = GetComponentInChildren<Animator>();
        mySR = myAnimator.GetComponent<SpriteRenderer>();

        if (enemyBase != null)
        {
            baseSpeed = enemyBase.moveSpeedBase;
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
            float speed = myRb.velocity.magnitude;

            if (speed > baseSpeed * switchToRunBuffer)
            {
                myAnimator.SetBool("isRunning", true);
            }
            else
            {
                myAnimator.SetBool("isRunning", false);
            }

            if (myAnimator.GetBool("isSitting") || myAnimator.GetBool("isJumping"))
            {
                myAnimator.speed = 1;
            }
            else
            {
                float animSpeed = speed * animSpeedFactor;
                animSpeed = Mathf.Clamp(animSpeed, 0, maxAnimSpeed);
                myAnimator.speed = animSpeed;
            }

        }
    }

    public virtual void SetSitAnimation(bool isSitting)
    {
        myAnimator.SetBool("isSitting", isSitting);
    }

    public virtual void SetJumpAnimation(bool isJumping)
    {
        myAnimator.SetBool("isJumping", isJumping);
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
