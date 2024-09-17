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
        // Increase Player guns start damage
        Player player = FindObjectOfType<Player>();
        PlayerShooter playerShooter = player.GetComponent<PlayerShooter>();
        foreach (GunSO gun in playerShooter.gunList)
        {
            if (gun.gunType == GunType.Laser)
            {
                //GunSO ogGun = playerShooter._gunListSource[playerShooter._gunListSource.IndexOf(gun)];
                gun.damage *= damageMult;
            }
        }

        // Increase turretGuns start damage
        foreach (TurretGunSO turretGun in TurretManager.Instance.turretGunList)
        {
            if (turretGun.gunType == GunType.Laser)
            {
                turretGun.damage *= damageMult;
            }
        }

        // Increase melee start damage
        player.stats.meleeDamage *= damageMult;
    }
}
