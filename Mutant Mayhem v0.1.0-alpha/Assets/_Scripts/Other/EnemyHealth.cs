using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class EnemyHealth : Health
{
    [SerializeField] string corpsePoolName;
    EnemyBase enemyBase;

    protected override void Awake()
    {
        myRb = GetComponent<Rigidbody2D>();

        enemyBase = GetComponent<EnemyBase>();
        if (enemyBase == null)
        {
            Debug.LogError("EnemyBase not found by EnemyHealth on Awake");
        }
    }

    public override void Die()
    {
        StopAllCoroutines();

        if (!string.IsNullOrEmpty(corpsePoolName))
        {
            hasDied = true;
            // Create corpse, pass scale and color
            GameObject corpse = PoolManager.Instance.GetFromPool(corpsePoolName);
            corpse.transform.position = transform.position;
            corpse.transform.rotation = transform.rotation;
            corpse.transform.localScale = transform.localScale;
            corpse.GetComponentInChildren<SpriteRenderer>().color = 
                                                GetComponent<SpriteRenderer>().color;

            // Pass physics to corpse      **  Depricate for improved performance?  **  Currently testing with non-simulated corpses
            //Rigidbody2D corpseRb = corpsePrefab.GetComponent<Rigidbody2D>();
            //corpseRb.velocity = myRb.velocity;
            //corpseRb.angularVelocity = myRb.angularVelocity;
            //corpseRb.mass = myRb.mass * 2;
        }

        // Increment Drop Counter
        int value = Mathf.FloorToInt(maxHealth / healthToCreditsDivisor * SettingsManager.Instance.CreditsMult);
        EnemyCounter.PickupCounter += value;

        // Increment Drop Threshold
        if (EnemyCounter.PickupDropThreshold < value * 1.5f)
            EnemyCounter.PickupDropThreshold = Mathf.FloorToInt(value * 1.5f);
        
        // Drop Pickup
        if (EnemyCounter.PickupCounter >= EnemyCounter.PickupDropThreshold || EnemyCounter.EnemyCount <= 1)
            DropPickup(EnemyCounter.PickupCounter);
        
        EnemyCounter.EnemyCount--; 

        enemyBase.Die();
    }

    void DropPickup(int value)
    {
        GameObject obj = PoolManager.Instance.GetFromPool("Pickup");
        Pickup pickup = obj.GetComponent<Pickup>();
        pickup.transform.position = transform.position;
        pickup.pickupData.credits = value;

        EnemyCounter.PickupCounter = 0;
    }
}
