using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : Health
{
    [SerializeField] Player player;
    public float healthRegenPerSec = 0.5f;

    void Start()
    {
        StartCoroutine(HealthRegen());
    }

    public override void ModifyHealth(float value, GameObject other)
    {
        if (!hasDied)
            PlayPainSound(value);

        health += value;
        if (health > maxHealth)
            health = maxHealth;

        // Stats counting
        // Layer# 8 - PlayerProjectiles.  player, enemy
        if (value < 0)
        {
            StatsCounterPlayer.DamageToPlayer -= value;
        }
        
        // Die
        if (health <= 0 && !hasDied)
        {
            Die();
            return;
        }
    }

    public override void SetMaxHealth(float value)
    {
        maxHealth = value;
    }

    IEnumerator HealthRegen()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            // Regenerate
            if (health < maxHealth)
            {
                health += healthRegenPerSec;
            }
        }
    }

    public override void Die()
    {
        //Debug.Log("PLAYER IS DEAD");
        hasDied = true;
        player.isDead = true;
        myRb.freezeRotation = false;
        // Random flip 
        int sign = Random.Range(0, 2) * 2 - 1; // Randomly 1 or -1
        myRb.AddTorque(sign * deathTorque);
    } 
}
