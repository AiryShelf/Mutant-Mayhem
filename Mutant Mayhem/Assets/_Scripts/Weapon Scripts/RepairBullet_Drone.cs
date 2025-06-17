using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepairBullet_Drone : RepairBullet
{
    // DEPRECATED
    public override void Fly()
    {
        AudioManager.Instance.PlaySoundFollow(shootSound, transform);

        target = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // If distance to target is above max, set target to max distance
        float targetDist = Vector2.Distance(transform.position, transform.right * 10);
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
}
