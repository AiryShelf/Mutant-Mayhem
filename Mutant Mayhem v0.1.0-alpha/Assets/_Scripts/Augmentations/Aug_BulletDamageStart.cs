using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Aug_BulletDamageStart_New", 
                 menuName = "Augmentations/Aug_BulletDamageStart")]
public class Aug_BulletDamageStart : AugmentationBaseSO
{
    public float damageMultStart;
    public float lvlMultIncrement = 0.15f;

    public override void ApplyAugmentation(AugManager augManager, int level)
    {
        float totalDamageMult = damageMultStart + (lvlMultIncrement * level);

        // Increase Player guns start damage
        Player player = FindObjectOfType<Player>();
        PlayerShooter playerShooter = player.GetComponent<PlayerShooter>();
        foreach (GunSO gun in playerShooter.gunList)
        {
            if (gun.gunType == GunType.Bullet)
            {
                gun.damage *= totalDamageMult;
                Debug.Log("Aug multiplied player bullet gun damage by " + totalDamageMult);
            }
        }

        // Increase turretGuns start damage
        foreach (TurretGunSO turretGun in TurretManager.Instance.turretGunList)
        {
            if (turretGun.gunType == GunType.Bullet)
            {
                turretGun.damage *= totalDamageMult;
                Debug.Log("Aug multiplied turret bullet gun damage by " + totalDamageMult);
            }
        }
    }

    public override string GetPositiveDescription(AugManager augManager, int level)
    {
        float totalDamageMult = damageMultStart + (lvlMultIncrement * level);
        string percentage = GameTools.FactorToPercent(totalDamageMult);
        string description = "Increases starting damage of all bullet based weapons by " + percentage +
                             " - Focus RP into balistics to develop stronger starting weaponry";
        return description;
    }

    public override string GetNegativeDescription(AugManager augManager, int level)
    {
        float totalDamageMult = damageMultStart + (lvlMultIncrement * level);
        string percentage = GameTools.FactorToPercent(totalDamageMult);
        string description = "Reduces starting damage of all bullet based weapons by " + percentage + 
                             " - Aquire RP by diverting processing focus from balistics into research.";
        return description;
        
    }
}
