using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Aug_CreditsMult_New", menuName = "Augmentations/Aug_CreditsMult")]
public class Aug_CreditsMult : AugmentationBaseSO
{
    public float creditsMult;
    public float lvlMultIncrement = 0.1f;

    public override void ApplyAugmentation(AugManager augManager, int level)
    {
        SettingsManager.Instance.CreditsMult = creditsMult;
    }
}
