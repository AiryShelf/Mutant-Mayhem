using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class GravityField : MonoBehaviour
{
    public float attractionRadius = 5f;
    public float attractionStrength = 10f;
    public LayerMask affectedLayers;

    void FixedUpdate()
    {
        // Find all pickups within the gravity field
        Collider2D[] pickupsInRange = Physics2D.OverlapCircleAll(transform.position, attractionRadius, affectedLayers);

        foreach (Collider2D pickup in pickupsInRange)
        {
            Rigidbody2D pickupRb = pickup.GetComponent<Rigidbody2D>();
            if (pickupRb != null)
            {
                // Calculate direction towards the player (center of the gravity field)
                Vector2 pickPos = pickup.transform.position;
                Vector2 myPos = transform.position;
                Vector2 direction = (myPos - pickPos).normalized;
                float distance = Vector2.Distance(myPos, pickPos);

                // Apply a force towards the player
                pickupRb.AddForce(direction * attractionStrength * Time.fixedDeltaTime / distance, ForceMode2D.Force);
            }
        }
    }

    // For visualizing the gravity field in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);
    }
}
