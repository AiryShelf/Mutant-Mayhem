using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Aug_CreditsMult_New", menuName = "Augmentations/Aug_CreditsMult")]
public class Aug_CreditsMult : AugmentationBaseSO
{
    public float lvlMultIncrement = 0.1f;

    public override void ApplyAugmentation(AugManager augManager, int level)
    {
        float creditsMult = SettingsManager.Instance.CreditsMult;
        float totalCreditsMult = creditsMult + (lvlMultIncrement * level);

        SettingsManager.Instance.CreditsMult = totalCreditsMult;
        
        Debug.Log("Aug set credits multiplier to " + totalCreditsMult);
    }

    public override string GetPositiveDescription(AugManager augManager, int level)
    {
        float creditsMult = 1;
        float totalCreditsMult = creditsMult + lvlMultIncrement * level;
        string percentage = GameTools.FactorToPercent(totalCreditsMult);
        string description = "Boost Credits income from pickupsby " + percentage + " - Recent insights into Mutant " +
                             "anatomy allows clones to identify prime tissue samples, which sell at a higher price";
        return description;
    }

    public override string GetNegativeDescription(AugManager augManager, int level)
    {
        float creditsMult = 1;
        float totalCreditsMult = creditsMult + lvlMultIncrement * -level;
        string percentage = GameTools.FactorToPercent(totalCreditsMult);
        string description = "Reduce Credits income by " + percentage + " - Using some of the profits from tissue " +
                             "samples, you can get a lease on some free Research Points!";
        return description;  
    }

    public override string GetNeutralDescription(AugManager augManager, int level)
    {
        return "Raise or lower the level to gain more or less credits";
    }
}
