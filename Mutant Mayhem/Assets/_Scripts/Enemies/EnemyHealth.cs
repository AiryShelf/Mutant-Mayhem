using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : Health
{
    [SerializeField] string corpsePoolName;
    [SerializeField] protected float healthToCreditsDivisor = 1;
    EnemyBase enemyBase;

    TileManager tileManager;

    protected override void Awake()
    {
        base.Awake();
        
        enemyBase = GetComponent<EnemyBase>();
        if (enemyBase == null)
        {
            Debug.LogError("EnemyHealth: EnemyBase not found on Awake");
        }

        tileManager = FindObjectOfType<TileManager>();
    }

    public override void ModifyHealth(float value, float textPulseScaleMax, Vector2 textDir, GameObject damageDealer)
    {
        base.ModifyHealth(value, textPulseScaleMax, textDir, damageDealer);

        // Stats counting
        if (damageDealer != null)
        {
            if (this.CompareTag("Enemy"))
                StatsCounterPlayer.DamageToEnemies -= healthChange;
        }
        
        // Die
        if (health <= 0 && !hasDied)
        {
            if (damageDealer != null)
            {
                // Structure layer 13
                if (damageDealer.layer == 13)
                    StatsCounterPlayer.EnemiesKilledByTurrets++;
                else if (damageDealer.CompareTag("Player") || damageDealer.CompareTag("PlayerExplosion") || damageDealer.layer == 8)
                    StatsCounterPlayer.EnemiesKilledByPlayer++;
            }

            Die();
            return;
        }
    }

    public override void Die()
    {
        if (!string.IsNullOrEmpty(corpsePoolName))
        {
            hasDied = true;
            SetCorpse(corpsePoolName);
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
