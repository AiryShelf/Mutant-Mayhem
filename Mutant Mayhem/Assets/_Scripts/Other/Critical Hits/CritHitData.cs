using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Critical Hit Data")]
public class CritHitData : ScriptableObject
{
    public float critChance;
    public float critMultiplier;
}
