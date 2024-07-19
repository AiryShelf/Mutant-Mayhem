using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public float detectionRange = 10f;
    [HideInInspector] public float rotationSpeed;
    public float expansionSpeed = 1f;
    [SerializeField] float expansionDelay = 0.1f;
    [SerializeField] float randScanPauseMinTime = 0.5f;
    [SerializeField] float randScanPauseTime = 2f;
    [SerializeField] float startShootAngle = 45f;
    public CircleCollider2D detectionCollider;
    public Shooter shooter;
    TurretGunSO turretGunSO;
    Transform target;
    bool hasTarget;
    float detectionRangeSqrd;
    Coroutine searchRoutine;

    void Start()
    {
        detectionCollider.radius = 0f;
        detectionRangeSqrd = detectionRange * detectionRange;
        if (shooter.currentGunSO is TurretGunSO _turretGunSO)
        {
            turretGunSO = _turretGunSO;
        }

        rotationSpeed = turretGunSO.rotationSpeed;
    }

    void FixedUpdate()
    {
        if (hasTarget)
        {
            TrackTarget();
        }
        else
        {
            if (searchRoutine == null)
                searchRoutine = StartCoroutine(SearchForTarget());
        }
    }

    void TrackTarget()
    {
        if (target == null || (transform.position - target.position).sqrMagnitude > detectionRangeSqrd)
        {
            // Target is dead or out of range
            hasTarget = false;
            shooter.hasTarget = false;
            detectionCollider.radius = 0f;
            return;
        }
        else
        {
            RotateTowardsTarget();
        }
    }

    IEnumerator SearchForTarget()
    {
        StartCoroutine(RandomScanning());

        while (!hasTarget)
        {
            // Expand the detection radius to find the next target
            detectionCollider.radius += expansionSpeed;
            if (detectionCollider.radius > detectionRange)
            {
                detectionCollider.radius = detectionRange; // Cap the radius at the max detection range
            }
            yield return new WaitForSeconds(expansionDelay);
        }
    }

    void RotateTowardsTarget()
    {
        Vector3 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 
                                                      rotationSpeed * Time.fixedDeltaTime);
        if (Quaternion.Angle(transform.rotation, targetRotation) <= startShootAngle)
            shooter.hasTarget = true;
        else
            shooter.hasTarget = false;
    }

    IEnumerator RandomScanning()
    {
        while (!hasTarget)
        {
            // Pick a random angle
            float randomAngle = Random.Range(0f, 360f);
            Quaternion randomRotation = Quaternion.Euler(0, 0, randomAngle);

            // Rotate towards the random angle at 1/3 rotation speed
            while (Quaternion.Angle(transform.rotation, randomRotation) > 0.01f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, randomRotation, 
                                                              rotationSpeed/3 * Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
                if (hasTarget)
                    yield break;
            }

            // Wait for a random time
            float randomWaitTime = Random.Range(randScanPauseMinTime, randScanPauseTime);
            yield return new WaitForSeconds(randomWaitTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasTarget && other.CompareTag("Enemy"))
        {
            //Debug.Log("Enemy entered trigger");
            target = other.transform;
            hasTarget = true;
            if (searchRoutine != null)
                StopCoroutine(searchRoutine);
            searchRoutine = null;
            detectionCollider.radius = 0f; // Reset the detection radius
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionCollider.radius);
    }
}