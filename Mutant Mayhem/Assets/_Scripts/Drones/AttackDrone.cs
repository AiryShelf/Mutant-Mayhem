using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDrone : Drone
{
    public Shooter shooter;
    [SerializeField] int toleranceAngle = 30;

    internal Transform targetTrans;
    float attackRange;

    public override void Initialize()
    {
        base.Initialize();
        attackRange = shooter.currentGunSO.bulletLifeTime *
                      shooter.currentGunSO.bulletSpeed * 0.75f;
        minJobDist = attackRange;
    }

    internal IEnumerator Attack()
    {
        while (true)
        {
            Vector3 forward = rb.transform.right.normalized;
            Vector3 dir = targetTrans.position - rb.transform.position;
            float dot = Vector3.Dot(forward, dir.normalized);
            float cosThreshold = Mathf.Cos(toleranceAngle * Mathf.Deg2Rad);
            if (dot >= cosThreshold)
                shooter.hasTarget = true;
            else
                shooter.hasTarget = false;

            if (dir.sqrMagnitude > Mathf.Pow(attackRange, 2))
                MoveTowards(targetTrans.position, 1);
            RotateTowards(targetTrans.position);
            yield return new WaitForFixedUpdate();
        }
    }

    protected override void SetNewAction(System.Func<IEnumerator> coroutineMethod)
    {
        shooter.hasTarget = false;
        base.SetNewAction(coroutineMethod);
    }
}
