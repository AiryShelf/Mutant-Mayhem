using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Aug_MoveSpeed_New", menuName = "Augmentations/Aug_MoveSpeed")]
public class Aug_MoveSpeed : AugmentationBaseSO
{
    public float speedMult;
    public float lvlMultIncrement = 2f;

    public override void ApplyAugmentation(AugManager augManager, int level)
    {
        float totalCreditsAdd = speedMult + (lvlMultIncrement * (level - 1));
        
        BuildingSystem.PlayerCredits += totalCreditsAdd;

        Debug.Log("Aug added " + totalCreditsAdd + " Credits");
    }

    public override string GetPositiveDescription(AugManager augManager, int level)
    {
        float totalCreditsAdd = speedMult + (lvlMultIncrement * (level - 1));
        string value = totalCreditsAdd.ToString("N0");
        string description = "Adds " + value +" Credits at start - Loan RP to a customer's " +
                             "research project in exchange for Credits";
        return description;
    }

    public override string GetNegativeDescription(AugManager augManager, int level)
    {
        float totalCreditsMult = speedMult + (lvlMultIncrement * -(level + 1));
        string percentage = GameTools.FactorToPercent(totalCreditsMult);
        string description = "Reduce Credits income by " + percentage + " - Using some of the profits from tissue " +
                             "samples, you can get a lease on some free Knowledge!";
        return description;
    }
}