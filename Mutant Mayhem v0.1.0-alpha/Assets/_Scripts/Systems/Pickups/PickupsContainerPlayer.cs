using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupsContainerPlayer : PickupsContainerBase
{
    
    [SerializeField] GravityField myGravity;
    [SerializeField] Collider2D myCollider;
    [SerializeField] float unloadDelay;
    [SerializeField] float unloadForce;

    bool isUnloading = false;

    void Awake()
    {

    }

    public override void AddToContainer(Pickup pickup)
    {
        base.AddToContainer(pickup);

    }

    void OnTriggerEnter2D(Collider2D col)
    {
        PickupsReceiverTrigger trigger = col.GetComponent<PickupsReceiverTrigger>();

        // Must be enter trigger to start unloading
        if (trigger == null || !trigger.isEnterTrigger)
            return;
        
        ToggleUnloading(true);
        StartCoroutine(UnloadPickups(col.transform));
    }

    void OnTriggerExit2D(Collider2D col)
    {
        PickupsReceiverTrigger trigger = col.GetComponent<PickupsReceiverTrigger>();

        // Must be enter trigger to stop unloading
        if (trigger == null)
            return;
        if (!trigger.isEnterTrigger)
        {
            ToggleUnloading(false);
        }
        
        StopAllCoroutines();
    }

    void ToggleUnloading(bool unloading)
    {
        isUnloading = unloading;
        myGravity.enabled = !unloading;
    }

    IEnumerator UnloadPickups(Transform receiverTrans)
    {
        
        while (true)
        {
            if (container.Count > 0)
            {
                Pickup pickup = container[0];
                pickup.unloading = true;
                pickup.gameObject.SetActive(true);

                // Impulse to left or right
                Vector2 receiverDir = transform.position - receiverTrans.position;
                int left = Random.Range(0,2);
                Vector2 impulseDir;
                if (left == 1)
                {
                    impulseDir = new Vector2(-receiverDir.y, receiverDir.x);
                }
                else
                {
                    impulseDir = new Vector2(receiverDir.y, -receiverDir.x);
                }
                  
                pickup.GetComponent<Rigidbody2D>().AddForce(impulseDir * unloadForce, ForceMode2D.Impulse);
                pickup.transform.SetParent(null);
                container.RemoveAt(0);
            }

            yield return new WaitForSeconds(unloadDelay);
        }
    }
}
