using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDrone : Drone
{
    public Shooter shooter;
    public float attackRange = 5f;

    internal Transform targetTrans;

    internal IEnumerator Attack()
    {
        shooter.hasTarget = true;
        yield return null;
    }
}
