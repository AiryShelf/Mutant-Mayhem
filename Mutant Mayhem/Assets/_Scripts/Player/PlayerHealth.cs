using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class PlayerHealth : Health
{
    [SerializeField] Player player;
    public float healthRegenPerSec = 0.5f;

    void Start()
    {
        StartCoroutine(HealthRegen());
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
        Debug.Log("PLAYER IS DEAD");
        player.isDead = true;
        myRb.freezeRotation = false;
        // Random flip 
        int sign = Random.Range(0, 2) * 2 - 1; // Randomly 1 or -1
        myRb.AddTorque(sign * deathTorque);
    } 
}
