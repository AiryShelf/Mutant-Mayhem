using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAggroTrigger : MonoBehaviour
{
    public GameObject PlayerTarget { get; set; }
    public EnemyBase _enemyBase;

    private void Awake()
    {
        PlayerTarget = GameObject.FindGameObjectWithTag("Player");
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        //Debug.Log("enemy AiTrigger enter");

        // Can raycast here for line-of-sight check

        // Layer# 13 - "AiTriggers"
        if (other.gameObject.layer == 13)
        {   
            if (other.CompareTag("PlayerBulletTrigger"))
            {
                _enemyBase.IsShotAggroed = true;
            }
            else if (other.CompareTag("PlayerTrigger"))
            {
                //Debug.Log("Aggro triggered");
                _enemyBase.SetAggroToPlayerStatus(true);
            }
            //else if (other.CompareTag("TurretTrigger"))
            {
                // Do Turret logic.
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        if (other.gameObject.layer == 13)
        {
            if (other.CompareTag("PlayerTrigger"))
            {
                //Debug.Log("Aggro Un-triggered");
                _enemyBase.SetAggroToPlayerStatus(false);
            }
        }
    }
}
