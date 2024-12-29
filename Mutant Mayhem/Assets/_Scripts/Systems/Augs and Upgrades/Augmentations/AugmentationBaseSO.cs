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
    
    public abstract void ApplyAugmentation(AugManager augManager, int level);
    public abstract string GetPositiveDescription(AugManager augManager, int level);
    public abstract string GetNegativeDescription(AugManager augManager, int level);
    public abstract string GetNeutralDescription(AugManager augManager, int level);
}