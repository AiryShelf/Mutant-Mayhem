using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAggroTrigger : MonoBehaviour
{
    public GameObject PlayerTarget { get; set; }
    public EnemyBase _enemyBase;
    [SerializeField] float waitTimeBetweenHits = 2f;
    int triggerColliderID = -1;
    bool recentlyHit;

    private void Awake()
    {
        PlayerTarget = GameObject.FindGameObjectWithTag("Player");
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        // This object should be on AiTriggers layer

        // Can raycast here for line-of-sight check?  Would require constant checking to
        // 'see' the target again...

        // Return if already has a target
        if (_enemyBase.EnemyChaseSOBaseInstance.targetTransform != null)
            return;

        if (other.CompareTag("PlayerBulletTrigger"))
        {
            if (!recentlyHit)
            {
                recentlyHit = true;
                _enemyBase.IsShotAggroed = true;
                AiTrigger trigger = other.GetComponent<AiTrigger>();
                _enemyBase.EnemyChaseSOBaseInstance.targetPos = trigger.origin.transform.position;
                StartCoroutine(WaitBetweenHits());
            }
        }
        else if (other.CompareTag("PlayerTrigger"))
        {
            //Debug.Log("Aggro triggered");
            _enemyBase.SetAggroStatus(true);
            _enemyBase.EnemyChaseSOBaseInstance.targetTransform = other.transform;
            triggerColliderID = other.GetInstanceID();
        }
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        //if (other.GetInstanceID() != triggerColliderID)
            //return;
        // This object should be on AiTriggers layer
        if (other.CompareTag("PlayerTrigger") && triggerColliderID == other.GetInstanceID())
        {
            //Debug.Log("Aggro Un-triggered");
            _enemyBase.SetAggroStatus(false);
            triggerColliderID = -1;
            _enemyBase.EnemyChaseSOBaseInstance.targetTransform = null;
        }
    }

    IEnumerator WaitBetweenHits()
    {
        yield return new WaitForSeconds(waitTimeBetweenHits);
        recentlyHit = false;
    }
}
