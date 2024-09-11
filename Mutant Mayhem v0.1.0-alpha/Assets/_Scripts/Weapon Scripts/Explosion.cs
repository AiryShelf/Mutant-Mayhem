using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Explosion : MonoBehaviour
{
    [SerializeField] SoundSO explosionSound;
    
    [Header("Explosion Settings")]
    [SerializeField] float force;
    [SerializeField] float radius;
    [SerializeField] float damage;
    [SerializeField] WindZone wind;
    [SerializeField] float windTime;

    void Start()
    {
        // CAN ADD WINDZONE COROUTINE TO CAUSE PRESSURE EFFECT

        AudioManager.Instance.PlaySoundAt(explosionSound, transform.position);

        // Find objects in range
        Vector2 pos = transform.position;
        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(pos, radius, 
                                        LayerMask.GetMask("Enemies", "Player", "Corpses"));
        // Loop through findings
        foreach (Collider2D collider in objectsInRange)
        {
            // Apply force
            Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = pos - rb.position;
                float dist = dir.magnitude;
                dir.Normalize();
                rb.AddForce(-dir * (force / dist), ForceMode2D.Impulse);

                // Apply Enemy Damage and AITrigger
                EnemyBase enemy = collider.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.ModifyHealth(-damage / dist, gameObject);
                    enemy.StartFreeze();
                    enemy.SetAggroStatus(true);
                    enemy.EnemyChaseSOBaseInstance.StartSprint();

                    StatsCounterPlayer.TotalDamageByPlayerExplosions += damage / dist;
                }
                else
                {
                    // Apply Player damage
                    Player player = collider.GetComponent<Player>();
                    if (player != null)
                    {
                        Health pHealth = player.GetComponent<Health>();
                        pHealth.ModifyHealth(-damage/2 / dist, gameObject);                   
                        StatsCounterPlayer.TotalDamageByPlayerExplosions += damage / dist;
                    }
                }

                
            }           
        }
    }
}
