using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour, IPowerConsumer, ITileObjectExplodable
{
    public int TurretManagerGunIndex;
    public float rotationSpeed;
    public float expansionDelay;
    [SerializeField] float expansionDist = 0.5f;
    [SerializeField] float randScanPauseMinTime = 0.5f;
    [SerializeField] float randScanPauseMaxTime = 2f;
    [SerializeField] float startShootAngle = 45f;
    public CircleCollider2D detectionCollider;
    public Shooter shooter;
    [SerializeField] GameObject pointLights; // For power on/off
    [SerializeField] GameObject flashLight;
    [SerializeField] RangeCircle rangeCircle;

    public string explosionPoolName;

    public void Explode()
    {
        if (!string.IsNullOrEmpty(explosionPoolName))
        {
            GameObject explosion = PoolManager.Instance.GetFromPool(explosionPoolName);
            explosion.transform.position = transform.position;
        }
    }

    TurretGunSO myGun;
    Transform target;
    bool hasTarget;
    float detectionRangeSqrd;
    Coroutine searchRoutine;
    Coroutine scanRoutine;

    bool initialized = false;
    bool hasPower = true;

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
        myGun = TurretManager.Instance.turretGunList[TurretManagerGunIndex];
        turretShooter.gunList[0] = myGun;
        turretShooter.SwitchGuns(0);
        
        // Initialize gun
        foreach (TurretGunSO managerGun in TurretManager.Instance.turretGunList)
        {
            if (managerGun == myGun)
            {
                myGun.damage = managerGun.damage;
                myGun.knockback = managerGun.knockback;
                myGun.clipSize = managerGun.clipSize;
                myGun.chargeDelay = managerGun.chargeDelay;
                myGun.shootSpeed = managerGun.shootSpeed;
                myGun.accuracy = managerGun.accuracy;
                myGun.bulletSpeed = managerGun.bulletSpeed;
                myGun.bulletLifeTime = managerGun.bulletLifeTime;
                myGun.rotationSpeed = managerGun.rotationSpeed;
                myGun.detectRange = managerGun.detectRange;
                myGun.expansionDelay = managerGun.expansionDelay;
                myGun.reloadSpeed = managerGun.reloadSpeed;
                break;
            }
        }

        // Initialize turret structure
        myGun.rotationSpeed += UpgradeManager.Instance.structureStatsUpgLevels[StructureStatsUpgrade.TurretTracking] 
                                   * myGun.rotSpeedUpgAmt;
        myGun.detectRange += UpgradeManager.Instance.structureStatsUpgLevels[StructureStatsUpgrade.TurretTracking] 
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
        rangeCircle.radius = Mathf.Sqrt(detectionRangeSqrd);

        initialized = true;
    }

    public void UpdateStructure()
    {
        if (shooter.currentGunSO is TurretGunSO turretGunSO)
        {
            detectionRangeSqrd = turretGunSO.detectRange * turretGunSO.detectRange;
            expansionDelay = turretGunSO.expansionDelay;
            rotationSpeed = turretGunSO.rotationSpeed;
            rangeCircle.radius = turretGunSO.detectRange;
        }
        else 
            Debug.Log("Non-turret gun found in TurretShooter");
    }

    public void PowerOn()
    {
        hasPower = true;
        if (pointLights != null)
            pointLights.SetActive(true);
        if (flashLight != null)
            flashLight.SetActive(true);

        Debug.Log("Turret: PowerOn ran");
    }

    public void PowerOff()
    {
        StopAllCoroutines();
        searchRoutine = null;
        scanRoutine = null;
        
        if (pointLights != null)
            pointLights.SetActive(false);
        if (flashLight != null)
            flashLight.SetActive(false);

        hasPower = false;
        hasTarget = false;
        shooter.hasTarget = false;
        detectionCollider.radius = 0f;
        Debug.Log("Turret: PowerOff ran");
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
            if (detectionCollider.radius >= myGun.detectRange)
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