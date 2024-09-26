using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class RepairBullet : Bullet
{
    [Header("Repair Bullet Settings")]
    [SerializeField] float speed = 10;
    [SerializeField] int repairCost = 5;

    Vector3 target;
    float targetDist;
    
    protected override void Start()
    {
        target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        target.z = 0; // Ensure the Z position is zero for 2D.

        // Calculate the distance to the target
        Vector3 dir = target - transform.position;
        targetDist = dir.magnitude;
        targetDist = Mathf.Clamp(targetDist, 0, destroyTime * speed);

        // Set the bullet's velocity to move straight out of the gun
        rb.velocity = transform.right * speed;

        // Start the coroutine to check the distance traveled
        StartCoroutine(CheckDistanceTravelled());
    }

    protected override void CheckCollisions()
    {
        // Do nothing
    }

    protected override void Hit(Collider2D otherCollider, Vector2 point)
    {
        // Do nothing
    }

    private IEnumerator CheckDistanceTravelled()
    {
        Vector3 startPosition = transform.position;

        while (Vector3.Distance(startPosition, transform.position) < targetDist)
        {
            yield return new WaitForEndOfFrame(); // Wait for the next frame
        }

        // Modify tile health 
        if (!tileManager.CheckTileFullHealth(transform.position))
        {
            if (BuildingSystem.PlayerCredits >= repairCost)
            {
                Vector2 pos = transform.position;
                tileManager.ModifyHealthAt(pos, -damage);
                tileManager.RepairEffectAt(pos);
                BuildingSystem.PlayerCredits -= repairCost;

                AudioManager.Instance.PlaySoundAt(hitSound, pos);
                Debug.Log("Ran repair code");
            }
            else
            {
                MessagePanel.PulseMessage("Not enough Credits to repair!", Color.red);
            }
        }

        effectsHandler.DestroyAfterSeconds();

        Destroy(gameObject);
    }
}
