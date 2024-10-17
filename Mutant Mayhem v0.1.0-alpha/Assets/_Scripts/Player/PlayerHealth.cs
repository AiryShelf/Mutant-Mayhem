using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : Health
{
    [SerializeField] Player player;
    public float healthRegenPerSec = 0.5f;
    public event Action<float> OnPlayerHealthChanged;
    public event Action<float> OnPlayerMaxHealthChanged;

    void Start()
    {
        StartCoroutine(HealthRegen());
    }

    public override void ModifyHealth(float value, GameObject damageDealer)
    {
        if (!player.IsDead)
            PlayPainSound(value);

        health += value;
        if (health > maxHealth)
            health = maxHealth;

        // Stats counting
        if (value < 0)
        {
            StatsCounterPlayer.DamageToPlayer -= value;
        }
        
        // Die
        if (health <= 0 && !player.IsDead)
        {
            Die();
            return;
        }

        OnPlayerHealthChanged.Invoke(health);
    }

    public override void SetMaxHealth(float value)
    {
        maxHealth = value;
        OnPlayerMaxHealthChanged.Invoke(health);
    }

    IEnumerator HealthRegen()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            // Regenerate
            if (health < maxHealth)
            {
                ModifyHealth(healthRegenPerSec, gameObject);
            }
        }
    }

    public override void Die()
    {
        //Debug.Log("PLAYER IS DEAD");
        hasDied = true;
        player.IsDead = true;
        myRb.freezeRotation = false;
        // Random flip 
        int sign = UnityEngine.Random.Range(0, 2) * 2 - 1; // Randomly 1 or -1
        myRb.AddTorque(sign * deathTorque);
    } 
}
