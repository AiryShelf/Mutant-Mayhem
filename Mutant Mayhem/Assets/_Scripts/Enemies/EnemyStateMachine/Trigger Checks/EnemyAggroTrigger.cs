using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This object should be on AiTriggers layer
public class EnemyAggroTrigger : MonoBehaviour
{
    public EnemyBase _enemyBase;
    [SerializeField] float waitTimeBetweenHits = 2f;
    
    int triggerColliderID = -1;
    bool recentlyHit;

    void OnDisable()
    {
        StopAllCoroutines();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!gameObject.activeInHierarchy)
            return;

        // Return if already has a target
        if (_enemyBase.targetTransform != null)
            return;

        // Can raycast here for line-of-sight check?  Would require constant checking..

        if (other.CompareTag("PlayerBulletTrigger"))
        {
            if (!recentlyHit)
            {
                recentlyHit = true;
                _enemyBase.IsShotAggroed = true;
                AiTrigger trigger = other.GetComponent<AiTrigger>();
                _enemyBase.targetPos = trigger.origin.transform.position;
                StartCoroutine(WaitBetweenHits());
            }
        }
        else if (other.CompareTag("TargetForEnemyTrigger"))
        {
            //Debug.Log("Aggro triggered");
            _enemyBase.SetAggroStatus(true);
            _enemyBase.targetTransform = other.transform;
            triggerColliderID = other.GetInstanceID();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        //if (other.GetInstanceID() != triggerColliderID)
        //return;
        // This object should be on AiTriggers layer
        if (other.CompareTag("TargetForEnemyTrigger") && triggerColliderID == other.GetInstanceID())
        {
            //Debug.Log("Aggro Un-triggered");
            _enemyBase.SetAggroStatus(false);
            triggerColliderID = -1;
            _enemyBase.targetTransform = null;
        }
    }

    IEnumerator WaitBetweenHits()
    {
        yield return new WaitForSeconds(waitTimeBetweenHits);
        recentlyHit = false;
    }
}
