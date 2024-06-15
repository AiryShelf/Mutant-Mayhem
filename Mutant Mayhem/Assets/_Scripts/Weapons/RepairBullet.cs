using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class RepairBullet : Bullet
{
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
        /*
        StartCoroutine(LerpToTarget());
        myRb.velocity = Vector2.zero;

        // Set target
        target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = target - transform.position;
        float targetDist = dir.magnitude;
        target = transform.right * targetDist;
        target.z = 0;

        // Calculate the rotation to face the target
        dir = dir.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        */
    }

    protected override void CheckCollisions()
    {
        // Do nothing
    }

    protected override void Hit(Collider2D otherCollider, Vector2 point)
    {
        tileManager.ModifyHealthAt(point, damage);

        // Delay bullet trail destroy
        if (bulletTrail != null)
        {
            bulletTrail.DestroyAfterSeconds();
            bulletTrail.transform.parent = null;
        }

        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    IEnumerator LerpToTarget()
    {
        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            // Move towards the target at a constant speed
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }

        // Ensure final position is set to target's position
        transform.position = target;

        Hit(null, target);   
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

        Destroy(gameObject);
    }
}
