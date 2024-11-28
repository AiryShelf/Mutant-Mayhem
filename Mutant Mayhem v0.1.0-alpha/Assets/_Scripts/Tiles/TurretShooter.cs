using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretShooter : Shooter
{
    [HideInInspector] public Player player;

    protected override void Fire()
    {
        // Use bullet ammo
        /*
        if (currentGunSO.gunType == GunType.Bullet)
        {
            if (player.stats.playerShooter.gunsAmmo[1] <= 0)
            {
                isReloading = true;
                return;
            }

            player.stats.playerShooter.gunsAmmo[1]--;
        }
        */
                
        
        base.Fire();

        StatsCounterPlayer.ShotsFiredByTurrets++;
    }

    protected override void Reload()
    {
        reloadRoutine = StartCoroutine(ReloadRoutine());
    }

    protected override IEnumerator ReloadRoutine()
    {
        // Wait for ammo to use
        while (player.stats.playerShooter.gunsAmmo[1] <= 0)
            yield return new WaitForSeconds(0.2f);

        DropClip();

        // Use bullet ammo
        int reloadAmount = Mathf.Clamp(player.stats.playerShooter.gunsAmmo[1], 0, clipSize);
        player.stats.playerShooter.gunsAmmo[1] -= reloadAmount;

        while (isReloading)
        {
            yield return new WaitForSeconds(0.1f);
            reloadTimer -= 0.1f;

            if (reloadTimer <= 0)
            {
                isReloading = false;
                gunsAmmoInClips[currentGunIndex] = reloadAmount;
                reloadTimer = TurretReloadTime;
                reloadRoutine = null;
            }
        }
    }
}
