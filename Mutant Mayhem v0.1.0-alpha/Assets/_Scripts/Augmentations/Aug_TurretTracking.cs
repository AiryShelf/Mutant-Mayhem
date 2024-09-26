using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Aug_TurretTracking_New", menuName = "Augmentations/Aug_TurretTracking")]
public class Aug_TurretTracking : AugmentationBaseSO
{
    float multStart = 1;
    public float lvlMultIncrement = 0.15f;

    public override void ApplyAugmentation(AugManager augManager, int level)
    {
        float totalMult = multStart + (lvlMultIncrement * level);

        // Increase turretGuns tracking and rotation
        foreach (TurretGunSO turretGun in TurretManager.Instance.turretGunList)
        {
            if (totalMult >= 1)
            {
                turretGun.rotationSpeed *= totalMult;
                turretGun.detectRange *= totalMult;
                turretGun.expansionDelay *= 2 - totalMult;
            }
            else if (totalMult < 1)
            {
                turretGun.rotationSpeed *= totalMult;
                turretGun.detectRange *= totalMult;
            }
            Debug.Log("Aug multiplied turret rotation, detection range, by " + totalMult + " and expansion delay by " + (2 - totalMult));
        }
    }

    public override string GetPositiveDescription(AugManager augManager, int level)
    {
        float totalMult = multStart + (lvlMultIncrement * level);
        string percentage = GameTools.FactorToPercent(totalMult);
        string description = percentage + " boost to turret rotation speed, sensor range, and detection speed" +
                             " - Advanced servos combined with ultra-precise quantum sensors allows turrets to track targets more effectively";
        return description;
    }

    public override string GetNegativeDescription(AugManager augManager, int level)
    {
        float totalMult = multStart + (lvlMultIncrement * level);
        string percentage = GameTools.FactorToPercent(totalMult);
        string description = "Lose " + percentage + " of turret rotation speed, sensor range, and detection speed" +
                             " - Cheap parts and less processing requirements allow aquisition of additional RP";
        return description;
    }

    public override string GetNeutralDescription(AugManager augManager, int level)
    {
        return "Raise or lower the level to adjust turret rotation sppeed, sensor range, and detection speed";
    }
}
