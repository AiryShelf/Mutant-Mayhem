using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShootTrigger : MonoBehaviour
{
    public EnemyBase _enemyBase;

    int triggerColliderID = -1;

    void Awake()
    {
        _enemyBase = GetComponentInParent<EnemyBase>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (other.CompareTag("TargetForEnemyTrigger"))
        {
            _enemyBase.SetShootDistanceBool(true);
            triggerColliderID = other.GetInstanceID();
        }
    }

    void OnTriggerExit2D(Collider2D other) 
    {
        if (other.GetInstanceID() == triggerColliderID)
        {
            _enemyBase.SetShootDistanceBool(false);
            triggerColliderID = -1;
        }
        
    }
}
