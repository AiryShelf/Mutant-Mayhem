using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "Aug_CreditsAdd_New", menuName = "Augmentations/Aug_CreditsAdd")]
public class Aug_CreditsAdd : AugmentationBaseSO
{
    public float amountToAdd;
    public float lvlAddIncrement = 2f;

    public override void ApplyAugmentation(AugManager augManager, int level)
    {
        float totalCreditsAdd = amountToAdd + (lvlAddIncrement * (level - 1));
        
        BuildingSystem.PlayerCredits += totalCreditsAdd;

        Debug.Log("Aug added " + totalCreditsAdd + " Credits");
    }

    public override string GetPositiveDescription(AugManager augManager, int level)
    {
        float totalCreditsAdd = amountToAdd + (lvlAddIncrement * (level - 1));
        string value = totalCreditsAdd.ToString("N0");
        string description = "Adds " + value +" Credits at start - Loan RP to a customer's " +
                             "research project in exchange for Credits";
        return description;
    }

    public override string GetNegativeDescription(AugManager augManager, int level)
    {
        float totalCreditsCost = amountToAdd + (lvlAddIncrement * (level - 1));
        string value = Mathf.Abs(totalCreditsCost).ToString("N0");
        string description = "Go " + value + " Credits into debt at start - In exchange for " +
                             "a bunch of RP, of course!  WARNING: This can make things very difficult";
        return description;
    }

    public override string GetNeutralDescription(AugManager augManager, int level)
    {
        return "Raise the level to start with more credits";
    }
}
