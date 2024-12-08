using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyHealth : Health
{
    [SerializeField] string corpsePoolName;
    EnemyBase enemyBase;

    TileManager tileManager;

    protected override void Awake()
    {
        base.Awake();
        
        enemyBase = GetComponent<EnemyBase>();
        if (enemyBase == null)
        {
            Debug.LogError("EnemyBase not found by EnemyHealth on Awake");
        }

        tileManager = FindObjectOfType<TileManager>();
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
        int deathValue = Mathf.FloorToInt(maxHealth / healthToCreditsDivisor * SettingsManager.Instance.CreditsMult);
        EnemyCounter.PickupCounter += deathValue;

        // Increment Drop Threshold
        if (EnemyCounter.PickupDropThreshold < deathValue)
            EnemyCounter.PickupDropThreshold = Mathf.FloorToInt(deathValue);
        
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

        pickup.tileManager = tileManager;
        pickup.RepositionIfNecessary();

        EnemyCounter.PickupCounter = 0;
    }
}
