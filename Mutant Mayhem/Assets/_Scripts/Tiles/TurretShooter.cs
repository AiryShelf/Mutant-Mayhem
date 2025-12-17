using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretShooter : Shooter
{
    [HideInInspector] public Player player;
    [SerializeField] float recoilDistance = 0.1f;
    Vector3 restLocalPos;
    Coroutine recoilRoutine;

    protected override void Start()
    {
        turretShooter = this;
        base.Start();
        restLocalPos = transform.localPosition;
    }

    protected override void Fire()
    {
        base.Fire();

        StatsCounterPlayer.ShotsFiredByTurrets++;
        if (recoilRoutine != null)
        {
            StopCoroutine(recoilRoutine);
            recoilRoutine = null;
            // Prevent drift when restarting recoil mid-kick.
            transform.localPosition = restLocalPos;
        }

        recoilRoutine = StartCoroutine(RecoilEffect());
    }
    
    IEnumerator RecoilEffect()
    {
        // lastShootDir is typically world-space; localPosition is relative to the parent.
        Vector3 worldShootDir = (Vector3)lastShootDir;
        if (worldShootDir.sqrMagnitude < 0.0001f)
        {
            // Fallback if lastShootDir wasn't set for some reason
            worldShootDir = transform.right;
        }

        Transform parent = transform.parent;
        Vector3 localShootDir = parent != null
            ? parent.InverseTransformDirection(worldShootDir)
            : worldShootDir;

        localShootDir.z = 0f;
        if (localShootDir.sqrMagnitude > 0.0001f)
            localShootDir.Normalize();

        // Move opposite the shot direction
        transform.localPosition = restLocalPos - localShootDir * recoilDistance;

        yield return new WaitForSeconds(0.05f);

        transform.localPosition = restLocalPos;
        recoilRoutine = null;
    }

    protected override void Reload()
    {
        reloadRoutine = StartCoroutine(ReloadRoutine());
    }

    protected override IEnumerator ReloadRoutine()
    {
        if (player == null)
        {
            Debug.LogWarning("Player is null.  It's okay once here, but not excessively");
            yield break;
        }

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
