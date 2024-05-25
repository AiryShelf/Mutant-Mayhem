using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShootCheck : MonoBehaviour
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
        if (other.gameObject == PlayerTarget)
        {
            _enemyBase.SetShootDistanceBool(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        if (other.gameObject == PlayerTarget)
        {
            _enemyBase.SetShootDistanceBool(false);
        }
    }
}
