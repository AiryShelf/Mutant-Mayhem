using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Aug_LaserDamageStart_New", menuName = "Augmentations/Aug_LaserDamageStart")]
public class Aug_LaserDamageStart : AugmentationBaseSO
{
    public float damageMult;
    public float lvlMultIncrement = 0.15f;

    public override void ApplyAugmentation(AugManager augManager, int level)
    {
        float totalDamageMult = damageMult + (lvlMultIncrement * level);

        Debug.Log("LaserDamageStart attempting to apply upgrades");

        // Increase Player guns start damage
        Player player = FindObjectOfType<Player>();
        PlayerShooter playerShooter = player.GetComponent<PlayerShooter>();
        foreach (GunSO gun in playerShooter.gunList)
        {
            if (gun.gunType == GunType.Laser)
            {
                //GunSO ogGun = playerShooter._gunListSource[playerShooter._gunListSource.IndexOf(gun)];
                gun.damage *= totalDamageMult;
                Debug.Log("Aug multiplied player laser gun damage by " + totalDamageMult);
            }
        }

        // Increase turretGuns start damage
        foreach (TurretGunSO turretGun in TurretManager.Instance.turretGunList)
        {
            if (turretGun.gunType == GunType.Laser)
            {
                turretGun.damage *= totalDamageMult;
                Debug.Log("Aug multiplied turret laser gun damage by " + totalDamageMult);
            }
        }

        // Increase melee start damage
        player.stats.meleeDamage *= totalDamageMult;
    }

    public override string GetPositiveDescription(AugManager augManager, int level)
    {
        float totalDamageMult = damageMult + (lvlMultIncrement * level);
        string percentage = GameTools.FactorToPercent(totalDamageMult);
        string description = "Increases starting damage of all laser based weapons by " + percentage + 
                             " - Focus RP into photonics to develop stronger starting laser weaponry";
        return description;
    }

    public override string GetNegativeDescription(AugManager augManager, int level)
    {
        float totalDamageMult = damageMult + (lvlMultIncrement * level);
        string percentage = GameTools.FactorToPercent(totalDamageMult);
        string description = "Reduces starting damage of all laser based weapons by " + percentage + 
                             " - Aquire RP by diverting processing focus from lasers into research";
        return description;
    }
}
