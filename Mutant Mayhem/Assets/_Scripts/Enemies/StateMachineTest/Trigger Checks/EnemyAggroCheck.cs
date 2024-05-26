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

    private void OnTriggerStay2D(Collider2D other) 
    {
        Debug.Log("enemy trigger enter");
        if (other.gameObject.layer == LayerMask.NameToLayer("AiTriggers"))
        {               
            if (other.tag == "PlayerTrigger")
            {
                //Debug.Log("Aggro triggered");
                _enemyBase.SetAggroToPlayerStatus(true);
            }
            else if (other.tag == "TurretTrigger")
            {
                // Do Turret logic.
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        if (other.gameObject.layer == LayerMask.GetMask("AITriggers"))
        {
            //Debug.Log("Aggro Un-triggered");
            _enemyBase.SetAggroToPlayerStatus(false);
        }
    }
}
