using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Explosion : MonoBehaviour
{
    
    [SerializeField] float force;
    [SerializeField] float radius;
    [SerializeField] float damage;
    [SerializeField] WindZone wind;
    [SerializeField] float windTime;

    void Start()
    {
        // CAN ADD WINDZONE COROUTINE TO CAUSE PRESSURE EFFECT

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

                // Apply effects
                EnemyBase enemy = collider.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.ModifyHealth(-damage / dist);
                    enemy.StartFreeze();
                    enemy.SetAggroToPlayerStatus(true);
                    enemy.EnemyChaseSOBaseInstance.StartSprint();
                }
            }           
        }
    }
}
