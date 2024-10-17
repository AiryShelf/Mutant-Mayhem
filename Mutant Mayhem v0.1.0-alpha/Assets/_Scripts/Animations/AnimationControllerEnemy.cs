using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationControllerEnemy : MonoBehaviour
{
    public float animSpeedFactor = 1f;
    [SerializeField] float switchToRunBuffer = 1.1f;
    [SerializeField] float maxAnimSpeed = 10;

    public Animator myAnimator;
    EnemyBase enemy;
    Rigidbody2D enemyRb;
    float baseSpeed;

    void Start()
    {
        enemy = GetComponent<EnemyBase>();
        myAnimator = GetComponentInChildren<Animator>();

        if (enemy != null)
        {
            baseSpeed = enemy.moveSpeedBase;
            enemyRb = enemy.GetComponent<Rigidbody2D>();
        }
    }

    void FixedUpdate()
    {
        UpdateEnemy();
    }

    void UpdateEnemy()
    {
        if (enemy != null)
        {
            float speed = enemyRb.velocity.magnitude;

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
