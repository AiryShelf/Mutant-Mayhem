using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AugmentationBaseSO : ScriptableObject
{
    public string augmentationName;
    [TextArea(3,10)]
    public string description;
    public Sprite uiImage;
    public int cost; // Research points required
    public AugmentationFamily family;
    public AugmentationType type = AugmentationType.Other;
    public int lvlCostIncrement;
    public float lvlCostIncrementMult = 1;
    public int maxLvl;

    
    public abstract void ApplyAugmentation(AugManager augManager, int level);
}

public enum AugmentationFamily
{
    Consumable,
    Exosuit,
    Structure,
    Mothership,
    LaserTech,
    BulletTech,
    ExplosiveTech,
    Drone
}

// This is used to prevent having a boost and drain of the same Aug type
public enum AugmentationType
{
    Other,
    CreditsMult,
    LaserDamageMult,
    BulletDamageMult,
}
