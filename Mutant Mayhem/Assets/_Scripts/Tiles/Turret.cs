using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [HideInInspector] public float detectionRange;
    [HideInInspector] public float rotationSpeed;
    [HideInInspector] public float expansionDelay;
    [SerializeField] float expansionDist = 0.5f;
    [SerializeField] float randScanPauseMinTime = 0.5f;
    [SerializeField] float randScanPauseMaxTime = 2f;
    [SerializeField] float startShootAngle = 45f;
    public CircleCollider2D detectionCollider;
    public Shooter shooter;
    TurretGunSO turretGun;
    Transform target;
    bool hasTarget;
    float detectionRangeSqrd;
    Coroutine searchRoutine;

    void Start()
    {
        InitializeTurret();
    }

    void FixedUpdate()
    {
        if (hasTarget)
        {
            TrackTarget();
        }
        else
        {
            if (searchRoutine == null)
                searchRoutine = StartCoroutine(SearchForTarget());
        }
    }

    void InitializeTurret()
    {
        turretGun = (TurretGunSO)shooter.gunList[0];
        
        // Initialize gun
        switch (turretGun.gunType)
        {
            case GunType.Laser:
                turretGun.damage += UpgradeManager.Instance.laserUpgLevels[GunStatsUpgrade.GunDamage] * turretGun.damageUpgFactor;
                turretGun.knockback += UpgradeManager.Instance.laserUpgLevels[GunStatsUpgrade.GunKnockback] * turretGun.knockbackUpgAmt;
                turretGun.clipSize += UpgradeManager.Instance.laserUpgLevels[GunStatsUpgrade.ClipSize] * turretGun.clipSizeUpgAmt;
                turretGun.chargeDelay += UpgradeManager.Instance.laserUpgLevels[GunStatsUpgrade.ChargeSpeed] * turretGun.chargeSpeedUpgNegAmt;
                turretGun.shootSpeed += UpgradeManager.Instance.laserUpgLevels[GunStatsUpgrade.ShootSpeed] * turretGun.shootSpeedUpgNegAmt;
                turretGun.accuracy += UpgradeManager.Instance.laserUpgLevels[GunStatsUpgrade.GunAccuracy] * turretGun.accuracyUpgNegAmt;
                turretGun.bulletLifeTime += UpgradeManager.Instance.laserUpgLevels[GunStatsUpgrade.GunRange] * turretGun.bulletRangeUpgAmt;
                break;
            case GunType.Bullet:
                turretGun.damage += UpgradeManager.Instance.bulletUpgLevels[GunStatsUpgrade.GunDamage] * turretGun.damageUpgFactor;
                turretGun.knockback += UpgradeManager.Instance.laserUpgLevels[GunStatsUpgrade.GunKnockback] * turretGun.knockbackUpgAmt;
                turretGun.clipSize += UpgradeManager.Instance.laserUpgLevels[GunStatsUpgrade.ClipSize] * turretGun.clipSizeUpgAmt;
                turretGun.shootSpeed += UpgradeManager.Instance.laserUpgLevels[GunStatsUpgrade.ShootSpeed] * turretGun.shootSpeedUpgNegAmt;
                turretGun.accuracy += UpgradeManager.Instance.laserUpgLevels[GunStatsUpgrade.GunAccuracy] * turretGun.accuracyUpgNegAmt;
                turretGun.bulletLifeTime += UpgradeManager.Instance.laserUpgLevels[GunStatsUpgrade.GunRange] * turretGun.bulletRangeUpgAmt;
                turretGun.reloadSpeed += UpgradeManager.Instance.bulletUpgLevels[GunStatsUpgrade.TurretReloadSpeed] * turretGun.reloadSpeedUpgNegAmt;
                break;
        }

        // Initialize turret structure
        turretGun.rotationSpeed += UpgradeManager.Instance.structureStatsUpgLevels[StructureStatsUpgrade.TurretRotSpeed] 
                                   * turretGun.rotSpeedUpgAmt;
        turretGun.detectRange += UpgradeManager.Instance.structureStatsUpgLevels[StructureStatsUpgrade.TurretSensors] 
                                 * turretGun.detectRangeUpgAmt;

        // Initialize detection collider
        if (shooter.gunList[0] is TurretGunSO gun)
        {
            detectionRangeSqrd = gun.detectRange * gun.detectRange;
            turretGun = gun;
            rotationSpeed = gun.rotationSpeed;
        }
        detectionCollider.radius = 0f;
    }

    public void UpdateSensors()
    {
        if (shooter.currentGunSO is TurretGunSO turretGunSO)
        {
            detectionRangeSqrd = turretGunSO.detectRange * turretGunSO.detectRange;
            expansionDelay = turretGunSO.expansionDelay;
        }
        else 
            Debug.Log("Found a non-turret gun attached to a turret");
    }

    void TrackTarget()
    {
        if (target == null || (transform.position - target.position).sqrMagnitude > detectionRangeSqrd)
        {
            // Target is dead or out of range
            hasTarget = false;
            shooter.hasTarget = false;
            detectionCollider.radius = 0f;
            return;
        }
        else
        {
            RotateTowardsTarget();
        }
    }

    IEnumerator SearchForTarget()
    {
        StartCoroutine(RandomScanning());

        while (!hasTarget)
        {
            // Expand the detection radius to find the next target
            detectionCollider.radius += expansionDist;
            if (detectionCollider.radius > detectionRange)
            {
                detectionCollider.radius = detectionRange; // Cap the radius at the max detection range
            }
            yield return new WaitForSeconds(expansionDelay);
        }
    }

    void RotateTowardsTarget()
    {
        Vector3 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 
                                                      rotationSpeed * Time.fixedDeltaTime);
        if (Quaternion.Angle(transform.rotation, targetRotation) <= startShootAngle)
            shooter.hasTarget = true;
        else
            shooter.hasTarget = false;
    }

    IEnumerator RandomScanning()
    {
        while (!hasTarget)
        {
            // Pick a random angle
            float randomAngle = Random.Range(0f, 360f);
            Quaternion randomRotation = Quaternion.Euler(0, 0, randomAngle);

            // Rotate towards the random angle at 1/3 rotation speed
            while (Quaternion.Angle(transform.rotation, randomRotation) > 0.01f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, randomRotation, 
                                                              rotationSpeed/3 * Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
                if (hasTarget)
                    yield break;
            }

            // Wait for a random time
            float randomWaitTime = Random.Range(randScanPauseMinTime, randScanPauseMaxTime);
            yield return new WaitForSeconds(randomWaitTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasTarget && other.CompareTag("Enemy"))
        {
            //Debug.Log("Enemy entered trigger");
            target = other.transform;
            hasTarget = true;
            if (searchRoutine != null)
                StopCoroutine(searchRoutine);
            searchRoutine = null;
            detectionCollider.radius = 0f; // Reset the detection radius
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionCollider.radius);
    }
}