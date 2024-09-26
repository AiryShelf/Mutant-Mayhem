using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Aug_MoveSpeed_New", menuName = "Augmentations/Aug_MoveSpeed")]
public class Aug_MoveSpeed : AugmentationBaseSO
{
    public float speedMult = 1;
    public float lvlMultIncrement = 0.1f;

    public override void ApplyAugmentation(AugManager augManager, int level)
    {
        float totalMult = speedMult + lvlMultIncrement * level;
        
        Player player = FindObjectOfType<Player>();
        player.stats.moveSpeed *= totalMult;
        player.stats.strafeSpeed *= totalMult;
        player.stats.lookSpeed *= totalMult;

        Debug.Log($"Aug applied {totalMult} multiplier to moveSpeed, strafeSpeed, and lookSpeed");
    }

    public override string GetPositiveDescription(AugManager augManager, int level)
    {
        float totalMult = speedMult + lvlMultIncrement * level;
        string percentage = GameTools.FactorToPercent(totalMult);
        string description = "Adds " + percentage + " to forward movement, strafe, and rotation speed at start.  " +
                             "Improvements to exosuit actuator and power deliver systems allow for faster movement";
        return description;
    }

    public override string GetNegativeDescription(AugManager augManager, int level)
    {
        float totalMult = speedMult + lvlMultIncrement * -level;
        string percentage = GameTools.FactorToPercent(totalMult);
        string description = "Reduces forward movement, strafe, and rotation speed by " + percentage +" at start.  " +
                             "Less focus on optimizing exosuit movement allows for more research processing, granting some RP";
        return description;
    }

    public override string GetNeutralDescription(AugManager augManager, int level)
    {
        return "Raise or lower the level to adjust your foward movement, strafe, and rotation speeds";
    }
}