using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Aug_MaxAugs_New", menuName = "Augmentations/Aug_MaxAugs")]
public class Aug_MaxAugs : AugmentationBaseSO
{
    public int lvlAddIncrement = 2;
    public int lvlNegIncrement = 1;

    public override void ApplyAugmentation(AugManager augManager, int level)
    {
        // This is applied by the +/- buttons
        //augManager.maxAugs -= lvlAddIncrement * level;

        Debug.Log("Max Augs is " + augManager.maxAugs);
    }

    public override string GetPositiveDescription(AugManager augManager, int level)
    {
        float totalToAdd = lvlAddIncrement * level;
        string description = "Increase Max Aug Levels - Trade RP for Quantum Cloud " +
                             "Services to boost your Augmentation capacity by " + totalToAdd;
        return description;
    }

    public override string GetNegativeDescription(AugManager augManager, int level)
    {
        float totalToAdd = Mathf.Abs(lvlNegIncrement * level);
        string description = "Reduces Max Aug Levels - Loan " + totalToAdd + " of your Augmentation " +
                             "capacity units in exchange for RP";
        return description; 
    }

    public override string GetNeutralDescription(AugManager augManager, int level)
    {
        return "Raise or lower the level to adjust how many augmenation levels you can apply";
    }
}
