using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AugmentationBaseSO : ScriptableObject
{
    public string augmentationName;
    public Sprite uiImage;
    public int cost; // Research points required
    public int lvlCostIncrement;
    public float lvlCostIncrementMult = 1;
    public int refund; // Only need if has neg levels
    public int lvlRefundIncrement;
    public float lvlRefundIncrementMult = 1;
    public int maxLvl;
    public int minLvl;
    public AugmentationFamily family;
    public AugmentationType type = AugmentationType.Other;
    
    public abstract void ApplyAugmentation(AugManager augManager, int level);
    public abstract string GetPositiveDescription(AugManager augManager, int level);
    public abstract string GetNegativeDescription(AugManager augManager, int level);
    public abstract string GetNeutralDescription(AugManager augManager, int level);
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

// This is used to resolve potential conflicts between 2 or more augs 
// augs by preventing selecting 2 of the same type, or control edge cases
public enum AugmentationType
{
    Other,
    MaxAugs,
}
