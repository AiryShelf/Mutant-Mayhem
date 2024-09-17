using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Aug_MaxAugs_New", menuName = "Augmentations/Aug_MaxAugs")]
public class Aug_MaxAugs : AugmentationBaseSO
{
    public int addToMax;
    public int lvlAddIncrement = 1;
    

    public override void ApplyAugmentation(AugManager augManager, int level)
    {
        AugManager.Instance.maxAugs += addToMax;
    }
}
