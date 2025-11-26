using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlanetStatModifier
{
    PlayerMoveSpeed,
    EnemyMoveSpeed,
    BuildCost,
    LaserDamage,
    BulletDamage,
    ExplosionDamage,
    RepairGunDamage,
    LaserRange,
    BulletRange,
    ThrowRange,
    CreditsMult,
    SupportSensors,
    EnemyDamage,
    EnemyHealth,
    EnemySize,
    StructureIntegrity,
    PlayerDrag,
    PlayerHealth,
}

[System.Serializable]
public class StatModifierEntry
{
    public string modifierUiName;
    public bool isDebuff = true;
    public PlanetStatModifier statModifier;
    public float multiplier;
}

[CreateAssetMenu(fileName = "PlanetProperty", menuName = "Game/Planets/Planet Property")]
public class PlanetPropertySO : ScriptableObject
{
    public string propertyName;
    public List<StatModifierEntry> statModifierEntries;
}
