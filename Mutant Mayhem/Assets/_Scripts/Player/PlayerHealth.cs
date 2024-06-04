using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class PlayerHealth : Health
{
    [SerializeField] Player player;
    public PlayerStats playerStats;

    void Start()
    {
        playerStats = player.stats;
        StartCoroutine(HealthRegen());
        maxHealth = playerStats.healthMax;
        health = playerStats.healthMax;
    }

    public override void SetMaxHealth(float value)
    {
        playerStats.healthMax = value;
        maxHealth = value;
    }

    IEnumerator HealthRegen()
    {
        while (true)
        {
            // This is here to update healthMax after an upgrade
            maxHealth = playerStats.healthMax;

            yield return new WaitForSeconds(1);

            // Regenerate
            if (health < playerStats.healthMax)
            {
                health += playerStats.healthRegen;
            }
        }
    }

    public override void Die()
    {
        Debug.Log("PLAYER IS DEAD");
        player.isDead = true;
        myRb.freezeRotation = false;
        // Random flip 
        int sign = Random.Range(0, 2) * 2 - 1; // Randomly 1 or -1
        myRb.AddTorque(sign * deathTorque);
    } 
}
