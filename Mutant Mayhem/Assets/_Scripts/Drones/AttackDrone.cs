using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDrone : Drone
{
    [SerializeField] int aimToleranceAngle = 30;
    [SerializeField] float keepDistanceFactor = 0.7f;

    internal Transform targetTrans;
    float attackRange;

    public override void Initialize(TurretGunSO droneGun)
    {
        base.Initialize(droneGun);
        attackRange = shooter.currentGunSO.bulletLifeTime *
                      shooter.currentGunSO.bulletSpeed * 0.9f;
        minJobDist = attackRange;
    }

    public override void RefreshStats()
    {
        attackRange = shooter.currentGunSO.bulletLifeTime *
                      shooter.currentGunSO.bulletSpeed * 0.9f;
        minJobDist = attackRange;
        
        base.RefreshStats();
    }

    internal IEnumerator Attack()
    {
        while (true)
        {
            Vector3 forward = rb.transform.right.normalized;
            Vector3 dir = targetTrans.position - rb.transform.position;
            float dot = Vector3.Dot(forward, dir.normalized);
            float cosThreshold = Mathf.Cos(aimToleranceAngle * Mathf.Deg2Rad);
            shooter.hasTarget = dot >= cosThreshold;

            float sqrDistance = dir.sqrMagnitude;
            float attackRangeSqr = Mathf.Pow(attackRange, 2);
            float keepDistanceSqr = Mathf.Pow(attackRange * keepDistanceFactor, 2);

            if (sqrDistance > attackRangeSqr)
            {
                // Move closer to the target
                MoveTowards(targetTrans.position, 1);
            }
            else if (sqrDistance < keepDistanceSqr)
            {
                // Move away from the target
                Vector3 awayDirection = rb.transform.position - targetTrans.position;
                MoveTowards(rb.transform.position + awayDirection.normalized, 1);
            }

            RotateTowards(targetTrans.position);
            yield return new WaitForFixedUpdate();
        }
    }

    protected override void SetNewAction(System.Func<IEnumerator> coroutineMethod)
    {
        shooter.hasTarget = false;
        base.SetNewAction(coroutineMethod);
    }

    protected override IEnumerator CheckIfJobDone()
    {
        yield return null;

        while (!jobDone)
        {
            if (currentJob == null)
            {
                Debug.Log("AttackDrone: CurrentJob found null");
                SetJobDone();
                yield break;
            }
            if (currentJob.jobType == DroneJobType.None)
            {
                SetJobDone();
                yield break;
            }

            yield return new WaitForSeconds(1);
        }
    }
}
