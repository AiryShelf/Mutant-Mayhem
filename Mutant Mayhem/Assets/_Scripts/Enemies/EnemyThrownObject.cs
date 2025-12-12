using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyThrownObject : Bullet
{
    Vector2 startScale;
    public float startDamage;
    public float accuracyRadius = 0.6f;

    protected override void Awake()
    {
        base.Awake();
        startScale = transform.localScale;
        startDamage = damage;
    }

    /// <summary>
    /// Throws this object along a smooth parabola from startPos to endPos.
    /// Travel time is computed from *arc length* / bulletSpeed, then marched
    /// in FixedUpdate-sized steps so we land exactly on endPos.
    /// </summary>
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
            ReturnToPool();
            yield break;
        }

        Vector2 start2 = new Vector2(startPos.x, startPos.y);

        // Apply accuracy spread (world units) before max-range clamping so the full path is consistent.
        Vector2 end2 = new Vector2(endPos.x, endPos.y);
        if (accuracyRadius > 0f)
        {
            Vector2 offset = Random.insideUnitCircle * accuracyRadius;
            end2 += offset;
            endPos = new Vector3(end2.x, end2.y, endPos.z);
        }

        Vector2 toEnd = end2 - start2;

        // clamp by maxRange
        float straightDistSqr = toEnd.sqrMagnitude;
        if (straightDistSqr > maxRange * maxRange)
        {
            float invMag = 1f / Mathf.Sqrt(straightDistSqr);
            Vector2 dir = toEnd * invMag;

            end2 = start2 + dir * maxRange;
            endPos = new Vector3(end2.x, end2.y, endPos.z);
        }

        // Compute arc length of the *curved* path, not the straight chord
        float arcLength = GameTools.EstimateParabolaArcLength(start2, end2, curveHeight);

        // Duration from arc length
        float travelDuration = arcLength / Mathf.Max(0.0001f, bulletSpeed);

        float dt = Time.fixedDeltaTime;
        int steps = Mathf.Max(1, Mathf.CeilToInt(travelDuration / dt));
        float invSteps = 1f / steps;

        float z = transform.position.z;

        // Track previous position for rotation
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

            // face velocity
            Vector3 dir = currentPos - prevPos;
            if (dir.sqrMagnitude > 0.0001f)
                transform.right = dir.normalized;

            speed = dir / dt;
            prevPos = currentPos;

            // bell-curve scale (unchanged)
            float bell = 4f * t * (1f - t);
            float scale = Mathf.Lerp(1f, peakScaleMult, bell);
            transform.localScale = startScale * scale;

            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(new Vector3(endPos.x, endPos.y, z));
        rb.velocity = speed;
        transform.localScale = startScale;

        ReturnToPool();
    }
    
    void ReturnToPool()
    {
        damage = startDamage;
        PoolManager.Instance.ReturnToPool(objectPoolName, gameObject);
    }
}
