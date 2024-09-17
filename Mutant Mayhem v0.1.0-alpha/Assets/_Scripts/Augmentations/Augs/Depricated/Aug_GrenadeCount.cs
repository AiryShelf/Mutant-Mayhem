using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Aug_GrenadeCount_New", menuName = "Augmentations/Aug_GrenadeCount")]
public class Aug_GrenadesCount : AugmentationBaseSO
{
    public int amountToStartWith;

    public override void ApplyAugmentation(AugManager augManager, int level)
    {
        // Depricated
    }
}
