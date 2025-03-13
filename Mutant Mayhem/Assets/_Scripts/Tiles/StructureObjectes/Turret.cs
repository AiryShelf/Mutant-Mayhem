using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Turret : MonoBehaviour
{
    public SpriteRenderer reloadImageSr;
    public float detectionRange;
    public float rotationSpeed;
    public float expansionDelay;
    [SerializeField] float expansionDist = 0.5f;
    [SerializeField] float randScanPauseMinTime = 0.5f;
    [SerializeField] float randScanPauseMaxTime = 2f;
    [SerializeField] float startShootAngle = 45f;
    public CircleCollider2D detectionCollider;
    public Shooter shooter;
    [SerializeField] List<Light2D> lights; // For power on/off

    TurretGunSO myGun;
    Transform target;
    bool hasTarget;
    float detectionRangeSqrd;
    Coroutine searchRoutine;
    Coroutine scanRoutine;
    Player player;

    bool initialized = false;
    bool hasPower = true;

    void Start()
    {
        //InitializeTurret();
    }

    void FixedUpdate()
    {
        if (!initialized || !hasPower)
            return;

        // Tracking
        if (hasTarget)
        {
            TrackTarget();
        }
        else
        {
            if (searchRoutine == null)
                searchRoutine = StartCoroutine(SearchForTarget());
        }

        // Reload Image
        if (shooter.gunsAmmoInClips[0] <= 0)
        {
            reloadImageSr.enabled = true;
        }
        else
        {
            reloadImageSr.enabled = false;
        }
    }

    public void InitializeTurret(Player player)
    {
        if (player == null)
        {
            Debug.LogError("Turret: Player is null on Initialize");
            return;
        }

        TurretShooter turretShooter = (TurretShooter)shooter;
        turretShooter.player = player;
        myGun = (TurretGunSO)shooter.gunList[0];
        
        // Initialize gun
        foreach (TurretGunSO turretGun in TurretManager.Instance.turretGunList)
        {
            if (turretGun.uiName == shooter.gunList[0].uiName)
            {
                myGun.damage = turretGun.damage;
                myGun.knockback = turretGun.knockback;
                myGun.clipSize = turretGun.clipSize;
                myGun.chargeDelay = turretGun.chargeDelay;
                myGun.shootSpeed = turretGun.shootSpeed;
                myGun.accuracy = turretGun.accuracy;
                myGun.bulletSpeed = turretGun.bulletSpeed;
                myGun.bulletLifeTime = turretGun.bulletLifeTime;
                myGun.rotationSpeed = turretGun.rotationSpeed;
                myGun.detectRange = turretGun.detectRange;
                myGun.expansionDelay = turretGun.expansionDelay;
                myGun.reloadSpeed = turretGun.reloadSpeed;
                break;
            }
        }

        // Initialize turret structure
        myGun.rotationSpeed += UpgradeManager.Instance.structureStatsUpgLevels[StructureStatsUpgrade.TurretRotSpeed] 
                                   * myGun.rotSpeedUpgAmt;
        myGun.detectRange += UpgradeManager.Instance.structureStatsUpgLevels[StructureStatsUpgrade.TurretSensors] 
                                 * myGun.detectRangeUpgAmt;

        // Initialize detection collider
        if (myGun is TurretGunSO gun)
        {
            detectionRangeSqrd = gun.detectRange * gun.detectRange;
            myGun = gun;
            rotationSpeed = gun.rotationSpeed;
        }
        else
            Debug.Log("Non-turret gun found in TurretShooter");
            
        detectionCollider.radius = 0f;

        initialized = true;
    }

    public void UpdateStructure()
    {
        if (shooter.currentGunSO is TurretGunSO turretGunSO)
        {
            detectionRangeSqrd = turretGunSO.detectRange * turretGunSO.detectRange;
            expansionDelay = turretGunSO.expansionDelay;
            rotationSpeed = turretGunSO.rotationSpeed;
        }
        else 
            Debug.Log("Non-turret gun found in TurretShooter");
    }

    public void PowerOn()
    {
        hasPower = true;
        foreach (var light in lights)
            light.enabled = true;
    }

    public void PowerOff()
    {
        StopAllCoroutines();
        hasPower = false;
        foreach (var light in lights)
            light.enabled = false;
    }

    #region Tracking

    void TrackTarget()
    {
        if (!target.gameObject.activeSelf || (transform.position - target.position).sqrMagnitude > detectionRangeSqrd + 20)
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
        //Debug.Log("Turret search routine ran");
        if (scanRoutine != null)
            StopCoroutine(scanRoutine);
        scanRoutine = StartCoroutine(RandomScanning());

        while (!hasTarget)
        {
            // Expand the detection radius to find the next target, cap at max
            detectionCollider.radius += expansionDist;
            if (detectionCollider.radius > myGun.detectRange)
            {
                detectionCollider.radius = myGun.detectRange;
                yield break;
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
            yield return new WaitForSeconds(2f);

            // Pick a random angle
            float randomAngle = Random.Range(0f, 360f);
            Quaternion randomRotation = Quaternion.Euler(0, 0, randomAngle);

            // Rotate towards the random angle at 1/3 rotation speed
            while (Quaternion.Angle(transform.rotation, randomRotation) > 0.01f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, randomRotation, 
                                                              rotationSpeed/5 * Time.fixedDeltaTime);
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
            detectionCollider.radius = 0f; // Reset the detection radius

            if (searchRoutine != null)
                StopCoroutine(searchRoutine);
            searchRoutine = null;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionCollider.radius);
    }

    #endregion
}