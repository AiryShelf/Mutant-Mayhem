using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Aug_MaxAugs_New", menuName = "Augmentations/Aug_MaxAugs")]
public class Aug_MaxAugs : AugmentationBaseSO
{
    public int lvlAddIncrement = 1;

    public override void ApplyAugmentation(AugManager augManager, int level)
    {
        // This is applied by the +/- buttons
        //augManager.maxAugs -= lvlAddIncrement * level;

        Debug.Log("Max Augs is " + augManager.maxAugs);
    }

    public override string GetPositiveDescription(AugManager augManager, int level)
    {
        float totalToAdd = lvlAddIncrement * level;
        string description = "Increase Max Augs by " + totalToAdd + " - Trade RP for Quantum Cloud " +
                             "Services to boost your Augmentation capacity";
        return description;
    }

    public override string GetNegativeDescription(AugManager augManager, int level)
    {
        float totalToAdd = Mathf.Abs(lvlAddIncrement * level);
        string description = "Reduce Max Augs by " + totalToAdd + " - Loan of your Augmentation " +
                             "capacity units and gain 300 RP";
        return description;
        
    }
}
