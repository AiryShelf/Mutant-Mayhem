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

    MessagePanel messagePanel;
    
    protected override void Start()
    {
        messagePanel = FindObjectOfType<MessagePanel>();

        target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        target.z = 0; // Ensure the Z position is zero for 2D.

        // Calculate the distance to the target
        Vector3 dir = target - transform.position;
        targetDist = dir.magnitude;

        // Set the bullet's velocity to move straight out of the gun
        myRb.velocity = transform.right * speed;

        // Start the coroutine to check the distance traveled
        StartCoroutine(CheckDistanceTravelled());
    }

    protected override void CheckCollisions()
    {
        // Do nothing
    }

    protected override void Hit(Collider2D otherCollider, Vector2 point)
    {
        tileManager.ModifyHealthAt(point, damage);

        effectsHandler.DestroyAfterSeconds();

        gameObject.SetActive(false);
        Destroy(gameObject);
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
                tileManager.ModifyHealthAt(transform.position, -damage);
                tileManager.RepairEffectAt(transform.position);
                BuildingSystem.PlayerCredits -= repairCost;
            }
            else
            {
                messagePanel.ShowMessage("Not enough Credits to repair!", Color.red);
            }
        }

        effectsHandler.DestroyAfterSeconds();

        Destroy(gameObject);
    }
}
