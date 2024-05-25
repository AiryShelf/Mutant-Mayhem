using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAggroCheck : MonoBehaviour
{
    public GameObject PlayerTarget { get; set; }
    public EnemyBase _enemyBase;

    private void Awake()
    {
        PlayerTarget = GameObject.FindGameObjectWithTag("Player");
        _enemyBase = GetComponentInParent<EnemyBase>();
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("AITriggers"))
        {
            Debug.Log("collision detected");
            _enemyBase.SetAggroStatus(true);
        }
        // could do another check for turrets, so that enemyBase can decide player over turret
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        if (other.gameObject.layer == LayerMask.GetMask("AITriggers"))
        {
            _enemyBase.SetAggroStatus(false);
        }
    }
}
