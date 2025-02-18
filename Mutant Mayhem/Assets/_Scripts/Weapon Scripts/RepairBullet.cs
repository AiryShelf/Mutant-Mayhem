using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RepairBullet : Bullet
{
    [Header("Repair Bullet Settings")]
    [SerializeField] protected float speed = 10;
    [SerializeField] BulletEffectsHandler effectsHandler;

    protected Vector2 target;
    
    public override void Fly()
    {
        SFXManager.Instance.PlaySoundFollow(shootSound, transform);

        //if (InputController.LastUsedDevice == Gamepad.current)
            target = CursorManager.Instance.GetCustomCursorWorldPos();
        //else 
            //target = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

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

    protected IEnumerator CheckDistanceTravelled(float travelDistance)
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

        // Build if blueprint
        if (!tileManager.CheckTileFullHealth(transform.position))
        {
            Vector2 pos = transform.position;
            bool isBlueprint = false;
            if (tileManager.IsTileBlueprint(pos))
            {
                isBlueprint = true;
                tileManager.BuildBlueprintAt(pos, -damage, 1.3f, hitDir);
            }
            
            if (!isBlueprint)
            {
                float repairCost = tileManager.GetRepairCostAt(pos, -damage);
                if (BuildingSystem.PlayerCredits >= repairCost)
                {
                    tileManager.ModifyHealthAt(pos, -damage, 1.3f, hitDir);
                    BuildingSystem.PlayerCredits -= repairCost;
                }
                else
                {
                    MessagePanel.PulseMessage("Not enough Credits to repair!", Color.red);
                    PoolManager.Instance.ReturnToPool(objectPoolName, gameObject);
                    yield break;
                }
            }
                
            ParticleManager.Instance.PlayRepairEffect(pos, transform.right);
            SFXManager.Instance.PlaySoundAt(hitSound, pos);
            StatsCounterPlayer.AmountRepairedByPlayer -= damage;
            //Debug.Log("Ran repair code");
        }

        PoolManager.Instance.ReturnToPool(objectPoolName, gameObject);
    }
}
