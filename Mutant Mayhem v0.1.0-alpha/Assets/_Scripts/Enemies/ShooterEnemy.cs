using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterEnemy : MonoBehaviour
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
            GameObject obj = PoolManager.Instance.GetFromPool(currentGun.bulletPoolName);
            obj.transform.position = gun.transform.position;
            obj.transform.rotation = gun.transform.rotation;

            Bullet bullet = obj.GetComponent<Bullet>();
            bullet.objectPoolName = currentGun.bulletPoolName;
            
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            Vector2 dir = ApplyAccuracy(transform.right);
            bullet.velocity = dir * currentGun.bulletSpeed;

            bullet.Fly();

            StartCoroutine(KeepBulletTrail(obj, currentGun.bulletLifeTime));
            StartCoroutine(MuzzleFlash());

            yield return new WaitForSeconds(currentGun.shootSpeed);
        }
    }

    // OLD CODE!  Use BulletEffectsHandler  
    
    IEnumerator KeepBulletTrail(GameObject bullet, float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        yield return new WaitForEndOfFrame();
        
        if (bullet != null)
        {
            BulletEffectsHandler bulletFX = bullet.GetComponent<BulletEffectsHandler>();
            if (bulletFX == null)
                yield break;

            bulletFX.transform.parent = null;
            //bulletFX.DestroyAfterSeconds();
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
