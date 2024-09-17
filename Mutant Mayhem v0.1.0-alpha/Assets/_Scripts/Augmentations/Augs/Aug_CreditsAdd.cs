using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Aug_CreditsAdd_New", menuName = "Augmentations/Aug_CreditsAdd")]
public class Aug_CreditsAdd : AugmentationBaseSO
{
    public float creditsAdd;
    public float lvlMultIncrement = 2f;

    public override void ApplyAugmentation(AugManager augManager, int level)
    {
        SettingsManager.Instance.CreditsMult *= creditsAdd;
    }
}
