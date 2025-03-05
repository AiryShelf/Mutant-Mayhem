using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RazorWire : MonoBehaviour
{
    public float slowFactor = 0.3f;
    public float damageInterval = 1f;
    public float damageAmount = 7f;
    [SerializeField] float distBeforeDamage = 0.2f;
    private Dictionary<Collider2D, Coroutine> enemyDamageCoroutines = new Dictionary<Collider2D, Coroutine>();

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
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
        else if (other.CompareTag("Player"))
        {
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
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.RemoveSlowFactor(slowFactor);
            }

            if (enemyDamageCoroutines.ContainsKey(other))
            {
                StopCoroutine(enemyDamageCoroutines[other]);
                enemyDamageCoroutines.Remove(other);
            }
        }
    }

    IEnumerator DamageOverTime(Health health)
    {
        Vector2 lastPosition = Vector2.zero;

        while (true)
        {
            if (health != null && health.gameObject.activeInHierarchy)
            {
                Vector3 currentPosition = health.gameObject.transform.position;
                if (Vector3.Distance(lastPosition, currentPosition) >= distBeforeDamage)
                {
                    // Apply damage only if sufficient movement has occurred.
                    health.ModifyHealth(-damageAmount, 1, GameTools.GetRandomDirection(), gameObject);
                    TileManager.Instance.ModifyHealthAt(transform.position, -damageAmount, 1, GameTools.GetRandomDirection());
                    lastPosition = currentPosition;
                }
            }
            else break;

            yield return new WaitForSeconds(damageInterval);
        }
    }
}
