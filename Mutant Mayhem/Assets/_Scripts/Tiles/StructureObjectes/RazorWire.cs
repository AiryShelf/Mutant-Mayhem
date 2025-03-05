using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RazorWire : MonoBehaviour
{
    public float slowFactor = 0.3f;
    public float damageInterval = 1f;
    public float damageAmount = 7f;
    private Dictionary<Collider2D, Coroutine> enemyDamageCoroutines = new Dictionary<Collider2D, Coroutine>();

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object is an enemy.
        if (other.CompareTag("Enemy"))
        {
            // **Change:** Assume enemy has an EnemyMovement component with ApplySlow method.
            EnemyBase enemyBase = other.GetComponent<EnemyBase>();
            if (enemyBase != null)
            {
                //Debug.Log("Applying SlowFactor");
                enemyBase.ApplySlowFactor(slowFactor);
            }
            
            // Start the damage coroutine if it’s not already running on this enemy.
            if (!enemyDamageCoroutines.ContainsKey(other))
            {
                Coroutine coroutine = StartCoroutine(DamageOverTime(other.GetComponent<Health>()));
                enemyDamageCoroutines.Add(other, coroutine);
            }
        }
        // Check if the object is the player.
        else if (other.CompareTag("Player"))
        {
            // **Change:** Assume the player has a PlayerMovement component with an ApplySlow method.
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.ApplySlowFactor(slowFactor);
            }

            if (!enemyDamageCoroutines.ContainsKey(other))
            {
                Coroutine coroutine = StartCoroutine(DamageOverTime(other.GetComponent<Health>()));
                enemyDamageCoroutines.Add(other, coroutine);
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // **Change:** Remove the slow effect from the enemy.
            EnemyBase enemyBase = other.GetComponent<EnemyBase>();
            if (enemyBase != null)
            {
                enemyBase.RemoveSlowFactor(slowFactor);
            }
            
            // Stop the damage coroutine if it’s running.
            if (enemyDamageCoroutines.ContainsKey(other))
            {
                StopCoroutine(enemyDamageCoroutines[other]);
                enemyDamageCoroutines.Remove(other);
            }
        }
        else if (other.CompareTag("Player"))
        {
            // **Change:** Remove the slow effect from the player.
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.RemoveSlowFactor(slowFactor);
            }
        }
    }

    IEnumerator DamageOverTime(Health health)
    {
        while (true)
        {
            if (health != null && health.gameObject.activeInHierarchy)
            {
                health.ModifyHealth(-damageAmount, 1, GameTools.GetRandomDirection(), gameObject);
            }
            else break;

            yield return new WaitForSeconds(damageInterval);
        }
    }
}
