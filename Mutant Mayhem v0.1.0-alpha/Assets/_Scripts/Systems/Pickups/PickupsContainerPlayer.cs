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
        
        Vector2 impulseDir = transform.position - col.transform.position;
        ToggleUnloading(true);
        StartCoroutine(UnloadPickups(impulseDir));
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

    IEnumerator UnloadPickups(Vector2 impulseDir)
    {
        
        while (container.Count > 0)
        {
            Pickup pickup = container[0];
            pickup.unloading = true;
            pickup.gameObject.SetActive(true);

            // Impulse to left or right
            int left = Random.Range(0,2);
            Vector2 adjustedImpulseDir;
            if (left == 1)
            {
                adjustedImpulseDir = new Vector2(-impulseDir.y, impulseDir.x);
            }
            else
            {
                adjustedImpulseDir = new Vector2(impulseDir.y, -impulseDir.x);
            }
            // Id like impulse dir to be adjusted to 90 degrees left or right.  
            pickup.GetComponent<Rigidbody2D>().AddForce(adjustedImpulseDir * unloadForce, ForceMode2D.Impulse);
            pickup.transform.SetParent(null);
            container.RemoveAt(0);

            yield return new WaitForSeconds(unloadDelay);
        }
    }
}
