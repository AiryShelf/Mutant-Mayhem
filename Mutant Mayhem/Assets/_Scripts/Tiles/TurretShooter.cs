using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretShooter : Shooter
{
    protected override void Fire()
    {
        base.Fire();

        StatsCounterPlayer.ShotsFiredByTurrets++;
    }
}
