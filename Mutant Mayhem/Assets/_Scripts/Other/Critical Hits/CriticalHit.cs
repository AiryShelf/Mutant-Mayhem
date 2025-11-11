using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CriticalHit : MonoBehaviour
{
    public CritHitData critHitData;
    public string effectPoolName = "Critical_Hit_FX";

    public (bool, float) RollForCrit(float chanceMult, float damageMult)
    {
        bool isCritical = GameTools.RollForCrit(critHitData.critChance * chanceMult);
        float multiplier = isCritical ? critHitData.critMultiplier * damageMult : 1.0f;
        return (isCritical, multiplier);
    }
}
