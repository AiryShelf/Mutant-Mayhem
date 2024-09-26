using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Aug_StructureIntegrity_New", menuName = "Augmentations/Aug_StructureIntegrity")]
public class Aug_StructureIntegrity : AugmentationBaseSO
{
    public float lvlMultIncrement = 0.1f;

    public override void ApplyAugmentation(AugManager augManager, int level)
    {
        StructureStats structureStats = FindObjectOfType<Player>().stats.structureStats;
        float integrityMult = structureStats.structureMaxHealthMult;
        float totalMult = integrityMult + (lvlMultIncrement * level);

        structureStats.structureMaxHealthMult = totalMult;
        
        Debug.Log("Aug set structure integrity to " + totalMult);
    }

    public override string GetPositiveDescription(AugManager augManager, int level)
    {
        float integrityMult = 1;
        float totalMult = integrityMult + lvlMultIncrement * level;
        string percentage = GameTools.FactorToPercent(totalMult);
        string description = "Boost structure integrity by " + percentage + " - Access to the latest materials science tech " +
                             "allows us to engineer our structures to withstand greater forces";
        return description;
    }

    public override string GetNegativeDescription(AugManager augManager, int level)
    {
        float integrityMult = 1;
        float totalMult = integrityMult + lvlMultIncrement * -level;
        string percentage = GameTools.FactorToPercent(totalMult);
        string description = "Reduce structure integrity by " + percentage + " - Using lower quality materials allows us to " +
                             "allocate the saved resources towards a loan on Research Points";
        return description;
    }

    public override string GetNeutralDescription(AugManager augManager, int level)
    {
        return "Raise or lower the level to adjust the integrity of all structures";
    }
}
