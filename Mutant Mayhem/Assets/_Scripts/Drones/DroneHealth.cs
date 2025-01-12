using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneHealth : Health
{
    Drone drone;

    protected override void Awake()
    {
        base.Awake();

        drone = GetComponent<Drone>();
        if (drone == null)
        {
            Debug.LogError("DroneHealth: Drone not found on Awake");
        }
    }

    public override void ModifyHealth(float value, float textPulseScaleMax, Vector2 textDir, GameObject damageDealer)
    {
        base.ModifyHealth(value, textPulseScaleMax, textDir, damageDealer);

        if (health <= 0 && !hasDied)
        {
            Die();
            return;
        }
    }

    public override void Die()
    {
        hasDied = true;
        drone.Die();
    }

}
