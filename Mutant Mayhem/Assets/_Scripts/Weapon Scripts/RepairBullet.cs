using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairBullet : Bullet
{
    [Header("Repair Bullet Settings")]
    [SerializeField] float speed = 10;
    [SerializeField] int repairCost = 5;
    [SerializeField] BulletEffectsHandler effectsHandler;

    Vector2 target;
    float targetDist;
    
    public override void Fly()
    {
        SFXManager.Instance.PlaySoundFollow(shootSound, transform);

        target = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // If distance to target is above max, set target to max distance
        float targetDist = Vector2.Distance((Vector2)transform.position, target);
        float maxTravelDistance = destroyTime * speed;
        if (targetDist > maxTravelDistance)
        {
            //Vector3 normalizedDir = targetDir.normalized;
            target = transform.position + transform.right * maxTravelDistance;
            targetDist = maxTravelDistance;
        }
        
        rb.velocity = transform.right * speed;

        // Start the coroutine to check the distance traveled
        StartCoroutine(CheckDistanceTravelled(targetDist));
        
    }

    protected override void CheckCollisions()
    {
        // Do nothing
    }

    protected override void Hit(Collider2D otherCollider, Vector2 point)
    {
        // Do nothing
    }

    private IEnumerator CheckDistanceTravelled(float travelDistance)
    {
        Vector3 startPosition = transform.position;
        Vector2 hitDir = (target - (Vector2)startPosition).normalized;

        float distanceTravelled = 0;
        while (distanceTravelled < travelDistance)
        {
            distanceTravelled = Vector3.Distance(startPosition, transform.position);
            yield return new WaitForEndOfFrame(); // Wait for the next frame
        }

        // Lock to target pos to avoid missses
        transform.position = target;

        // Modify tile health 
        if (!tileManager.CheckTileFullHealth(transform.position))
        {
            if (BuildingSystem.PlayerCredits >= repairCost)
            {
                Vector2 pos = transform.position;
                if (tileManager.IsTileBlueprint(pos))
                    ConstructionManager.Instance.BuildBlueprint(pos, -damage, hitDir);
                else
                    tileManager.ModifyHealthAt(pos, -damage, 2, hitDir);
                ParticleManager.Instance.PlayRepairEffect(pos, transform.right);
                BuildingSystem.PlayerCredits -= repairCost;

                SFXManager.Instance.PlaySoundAt(hitSound, pos);
                Debug.Log("Ran repair code");
            }
            else
            {
                MessagePanel.PulseMessage("Not enough Credits to repair!", Color.red);
            }
        }

        //effectsHandler.DestroyAfterSeconds();

        PoolManager.Instance.ReturnToPool(objectPoolName, gameObject);
    }
}
