using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDrone : Drone
{
    public Shooter shooter;

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
        shooter.hasTarget = true;

        while (true)
        {
            Vector3 dir = targetTrans.position - rb.transform.position;
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
