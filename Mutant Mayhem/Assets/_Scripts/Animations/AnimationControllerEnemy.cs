using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationControllerEnemy : MonoBehaviour
{
    public float animSpeedFactor = 1f;
    public float switchToRunBuffer = 1.1f;
    public float maxAnimSpeed = 10;

    [SerializeField] Animator myAnimator;
    EnemyBase enemy;
    protected Rigidbody2D myRb;
    protected float baseSpeed;

    protected virtual void Start()
    {
        enemy = GetComponent<EnemyBase>();
        myAnimator = GetComponentInChildren<Animator>();

        if (enemy != null)
        {
            baseSpeed = enemy.moveSpeedBase;
            myRb = enemy.GetComponent<Rigidbody2D>();
        }
    }

    protected void FixedUpdate()
    {
        UpdateEnemy();
    }

    protected virtual void UpdateEnemy()
    {
        if (enemy != null)
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
            float animSpeed = speed * animSpeedFactor;
            animSpeed = Mathf.Clamp(animSpeed, 0, maxAnimSpeed);
            myAnimator.speed = animSpeed;
        }
    }
}
