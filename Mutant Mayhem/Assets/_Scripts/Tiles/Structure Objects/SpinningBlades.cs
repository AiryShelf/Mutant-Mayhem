using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningBlades : MonoBehaviour, ITileObjectExplodable
{
    
    [SerializeField] float rotationForce = 10000f;
    [SerializeField] float maxAngularVelocity = 1000f;
    [SerializeField] float minAngularVelocityToDamage = 100f;
    [SerializeField] float damagePerAngularVel = 0.1f;
    [SerializeField] float damageInterval = 0.5f;
    [SerializeField] float selfDamagePerAngularVel = 0.01f;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] bool allowSpinning = true;

    List<Collider2D> tempColliders = new List<Collider2D>();
    float previousAngularVelocity = 0f;

    public string explosionPoolName;

    public void Explode()
    {
        if (!string.IsNullOrEmpty(explosionPoolName))
        {
            GameObject explosion = PoolManager.Instance.GetFromPool(explosionPoolName);
            explosion.transform.position = transform.position;
        }
    }

    void FixedUpdate()
    {
        // Apply torque and limit angular velocity
        if (allowSpinning && Mathf.Abs(rb.angularVelocity) < maxAngularVelocity)
            rb.AddTorque(rotationForce * Time.fixedDeltaTime);

        previousAngularVelocity = rb.angularVelocity;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (tempColliders.Contains(collision.collider) || Mathf.Abs(previousAngularVelocity) < minAngularVelocityToDamage)
            return;

        // Damage health on collision
        Collider2D otherCollider = collision.collider;
        if (otherCollider.CompareTag("Enemy") || otherCollider.CompareTag("Player"))
        {
            Health health = otherCollider.GetComponent<Health>();
            if (health != null)
            {
                float damageToApply = damagePerAngularVel * Mathf.Abs(previousAngularVelocity);
                // Get ratio of damage from min to max angular velocity for TextFly scaling
                float damageScale = 1 + Mathf.Clamp01(Mathf.Abs(previousAngularVelocity) / maxAngularVelocity);
                Vector2 hitDir = otherCollider.transform.position - transform.position;
                health.ModifyHealth(-damageToApply, damageScale, hitDir, otherCollider.gameObject);

                // Self damage
                float selfDamage = selfDamagePerAngularVel * Mathf.Abs(previousAngularVelocity);
                TileManager.Instance.ModifyHealthAt(transform.position, -selfDamage, 1f, -hitDir);

                // Add to temp list to avoid multiple damage instances in short time
                tempColliders.Add(otherCollider);
                StartCoroutine(RemoveColliderFromTempList(otherCollider, damageInterval));
            }
        }

        //previousAngularVelocity = rb.angularVelocity;
    }

    IEnumerator RemoveColliderFromTempList(Collider2D collider, float delay)
    {
        yield return new WaitForSeconds(delay);
        tempColliders.Remove(collider);
    }
}
