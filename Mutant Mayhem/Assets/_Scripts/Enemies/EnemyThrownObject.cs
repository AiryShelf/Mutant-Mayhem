using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyThrownObject : Bullet
{
    Vector2 startScale;

    protected override void Awake()
    {
        base.Awake();
        startScale = transform.localScale;
    }

    /// <summary>
    /// Throws this object along a smooth parabola from startPos to endPos.
    /// Travel time is computed from distance / bulletSpeed, then marched
    /// in FixedUpdate-sized steps so we land exactly on endPos.
    /// </summary>
    /// <param name="startPos">World start position</param>
    /// <param name="endPos">World end position</param>
    /// <param name="bulletSpeed">Units per second (must be > 0)</param>
    /// <param name="curveHeight">Peak arc height added to the mid-point</param>
    /// <param name="peakScaleMult">Scale multiplier at the arc peak (t≈0.5)</param>
    /// <param name="maxRange">Maximum distance the object can be thrown</param>
    public IEnumerator ThrowTowardsTarget(
        Vector3 startPos,
        Vector3 endPos,
        float bulletSpeed,
        float curveHeight = 2f,
        float peakScaleMult = 1.2f,
        float maxRange = 10f)
    {
        if (bulletSpeed <= 0f)
        {
            transform.position = endPos;
            PoolManager.Instance.ReturnToPool(objectPoolName, gameObject);
            yield break;
        }
        
        float distance = Vector2.Distance(new Vector2(startPos.x, startPos.y),
                                                 new Vector2(endPos.x, endPos.y));
        distance = Mathf.Min(distance, maxRange);

        float travelDuration = distance / Mathf.Max(0.0001f, bulletSpeed);

        float dt = Time.fixedDeltaTime;
        int steps = Mathf.Max(1, Mathf.CeilToInt(travelDuration / dt));
        float invSteps = 1f / steps;

        float z = transform.position.z;

        // --- NEW: track previous position for rotation ---
        Vector3 prevPos = startPos;
        Vector2 speed = Vector2.one;

        for (int i = 1; i <= steps; i++)
        {
            float t = i * invSteps;

            float x = Mathf.Lerp(startPos.x, endPos.x, t);
            float y = Mathf.Lerp(startPos.y, endPos.y, t);
            y += curveHeight * (t - t * t);

            Vector3 currentPos = new Vector3(x, y, z);
            rb.MovePosition(currentPos);

            // --- NEW: rotate to face velocity ---
            Vector3 dir = currentPos - prevPos;
            if (dir.sqrMagnitude > 0.0001f) // avoid zero-length
                transform.right = dir.normalized;

            speed = dir / dt;
            prevPos = currentPos;

            // Scale size in bell curve
            float bell = 4f * t * (1f - t);
            float scale = Mathf.Lerp(1, peakScaleMult, bell);
            transform.localScale = startScale * scale;

            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(new Vector3(endPos.x, endPos.y, z));
        rb.velocity = speed;
        transform.localScale = startScale;

        PoolManager.Instance.ReturnToPool(objectPoolName, gameObject);
    }
}
