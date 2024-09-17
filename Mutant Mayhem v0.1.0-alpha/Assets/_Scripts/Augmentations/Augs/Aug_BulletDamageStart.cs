using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Aug_BulletDamageStart_New", menuName = "Augmentations/Aug_BulletDamageStart")]
public class Aug_BulletDamageStart : AugmentationBaseSO
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
            if (gun.gunType == GunType.Bullet)
            {
                //GunSO ogGun = playerShooter._gunListSource[playerShooter._gunListSource.IndexOf(gun)];
                gun.damage *= damageMult;
            }
        }

        // Increase turretGuns start damage
        foreach (TurretGunSO turretGun in TurretManager.Instance.turretGunList)
        {
            if (turretGun.gunType == GunType.Bullet)
            {
                turretGun.damage *= damageMult;
            }
        }
    }
}
