using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ShooterTurret : MonoBehaviour
{
    public List<GunSO> gunList;
    public GunSO currentGun;
    public GameObject currentMuzzleFlash;
    Coroutine shootingCoroutine;
    [HideInInspector] public bool isShooting;
    Transform gun;
    bool waitToShoot;

    void Awake()
    {
        gun = transform.Find("Gun");
        SwitchGuns(0);
        //currentGun = gunList[0];
        //currentMuzzleFlash = Instantiate(currentGun.muzzleFlashPrefab, gun.transform);
    }

    void Start()
    {
        
    }

    void Update()
    {
        Shoot();
    }

    public void SwitchGuns(int i)
    {
        currentGun = gunList[i];
        if (currentMuzzleFlash != null)
            Destroy(currentMuzzleFlash);
        currentMuzzleFlash = Instantiate(gunList[i].muzzleFlashPrefab, gun.transform);
        currentMuzzleFlash.SetActive(false);
    }

    public void Shoot()
    {
        if (isShooting && shootingCoroutine == null && !waitToShoot)
        {
            shootingCoroutine = StartCoroutine(ShootContinuously());
            StartCoroutine(WaitToShoot());
            waitToShoot = true;
        }
        else if (!isShooting && shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }
    }

    IEnumerator WaitToShoot()
    {
        yield return new WaitForSeconds(currentGun.shootSpeed);
        waitToShoot = false;
    }

    IEnumerator ShootContinuously()
    {
        while (true)
        {
            GameObject bullet = Instantiate(currentGun.bulletPrefab, gun.transform.position, transform.rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            Vector2 dir = ApplyAccuracy(transform.right);
            rb.velocity = dir * currentGun.bulletSpeed;
            StartCoroutine(DestroyBullet(bullet, currentGun.bulletLifeTime));
            Destroy(bullet, currentGun.bulletLifeTime);

            StartCoroutine(MuzzleFlash());

            yield return new WaitForSeconds(currentGun.shootSpeed);
        }
    }

    IEnumerator DestroyBullet(GameObject bullet, float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        if (bullet != null)
        {
            BulletEffectsHandler trails = bullet.GetComponent<BulletEffectsHandler>();
            trails.transform.parent = null;
            trails.DestroyAfterSeconds();
        }
        
    }

    Vector2 ApplyAccuracy(Vector2 dir)
    {
        // Vector to radians to degrees
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        // Implement accuracy randomness
        angle += Random.Range(-currentGun.accuracy, currentGun.accuracy);
        // Convert back to radians to vector
        float radians = angle * Mathf.Deg2Rad;
        dir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

        return dir;
    }

    IEnumerator MuzzleFlash()
    {
        currentMuzzleFlash.SetActive(true);
        yield return new WaitForSeconds(currentGun.muzzleFlashTime);
        currentMuzzleFlash.SetActive(false);
        //currentGun.muzzleFlash.enabled = false;
    }
}
