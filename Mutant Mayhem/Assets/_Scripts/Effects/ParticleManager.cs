using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance;

    [SerializeField] ParticleSystem repairHit;
    [SerializeField] ParticleSystem bulletBlood;
    [SerializeField] ParticleSystem bloodSpray;
    [SerializeField] ParticleSystem bulletBloodFling;
    [SerializeField] ParticleSystem bulletBloodEntry;
    [SerializeField] ParticleSystem bulletHitWall;
    [SerializeField] ParticleSystem bulletHole; // Depricate?

    [SerializeField] ParticleSystem meleeBlood;
    [SerializeField] ParticleSystem meleeHitWall;

    [SerializeField] ParticleSystem laserHit;
    [SerializeField] ParticleSystem laserSplash;
    [SerializeField] ParticleSystem bulletHit;
    [SerializeField] ParticleSystem bulletSplash;

    [SerializeField] ParticleSystem casingLaser;
    [SerializeField] ParticleSystem casingLaserDrip;
    [SerializeField] ParticleSystem casingBullet_SMG;
    [SerializeField] ParticleSystem casingBullet_Rifle;
    [SerializeField] ParticleSystem casingRepair;
    [SerializeField] ParticleSystem casingRepairDrip;

    [SerializeField] GameObject casingPrefab_SMG;
    [SerializeField] GameObject casingPrefab_Rifle;
    [SerializeField] ParticleSystem clip_SMG;
    [SerializeField] ParticleSystem clip_Turret_Rifle;

    ParticleSystem.Particle[] particlesBuffer;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneUnloaded += ClearParticles;
    }

    void OnDisable()
    {
        SceneManager.sceneUnloaded -= ClearParticles;
    }

    void ClearParticles(Scene current)
    {
        ClearAllChildrenParticleSystems();
    }

    public void ClearAllChildrenParticleSystems()
    {
        ParticleSystem[] pSystems = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in pSystems)
        {
            ps.Clear();
        }
    }

    public void ClearBulletHolesInBounds(Bounds worldBounds)
    {
        if (bulletHole == null) return;

        var main = bulletHole.main;
        if (main.simulationSpace != ParticleSystemSimulationSpace.World)
        {
            Debug.LogWarning("BulletHoleManager: bulletHoleSystem should use World simulation space for ClearInBounds to work correctly.");
        }

        int max = main.maxParticles;
        if (particlesBuffer == null || particlesBuffer.Length < max)
            particlesBuffer = new ParticleSystem.Particle[max];

        int count = bulletHole.GetParticles(particlesBuffer);

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = particlesBuffer[i].position; // world-space if simulationSpace = World

            if (worldBounds.Contains(pos))
            {
                // Kill particle
                particlesBuffer[i].remainingLifetime = 0f;
            }
        }

        bulletHole.SetParticles(particlesBuffer, count);
    }

    void SetPositionAndRotation(ParticleSystem ps, Vector2 pos, Vector2 hitDir)
    {
        ps.transform.position = pos;
        float angle = Mathf.Atan2(hitDir.y, hitDir.x) * Mathf.Rad2Deg;
        ps.transform.rotation = Quaternion.Euler(0, 0, angle + 180);
    }

    void SetPositionAndRotation(ParticleSystem ps, Vector2 pos, Quaternion rot)
    {
        ps.transform.position = pos;
        ps.transform.rotation = rot;
    }

    public void PlayHitEffectByName(string methodName, Vector2 hitPos, Vector2 hitDir)
    {
        //Debug.Log("PlayHitEffectByName called");
        // Get the method by name using reflection
        MethodInfo method = this.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (method != null)
        {
            // Check if the method has the correct signature
            method.Invoke(this, new object[] { hitPos, hitDir });
        }
        else
        {
            Debug.LogError($"ParticleManager: No method found with name: {methodName}");
        }
    }

    public void PlayCasingEffectByName(string methodName, Transform ejectorTrans, Quaternion rot, bool isElevated)
    {
        //Debug.Log("PlayCasingEffectByName called");
        // Get the method by name using reflection
        MethodInfo method = this.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (method != null)
        {
            // Check if the method has the correct signature
            method.Invoke(this, new object[] { ejectorTrans, rot, isElevated });
        }
        else
        {
            Debug.LogError($"ParticleManager: No method found with name: {methodName}, or incorrect signature");
        }
    }

    #region Bullet Hits

    public void PlayRepairEffect(Vector2 hitPos, Vector2 hitDir)
    {
        SetPositionAndRotation(repairHit, hitPos, hitDir);
        repairHit.Emit(5);
    }

    public void PlayBulletHitEffect(GunType gunType, Vector2 hitPos, Vector2 hitDir)
    {
        switch (gunType)
        {
            case GunType.Laser:
                SetPositionAndRotation(laserHit, hitPos, hitDir);
                laserHit.Emit(10);
                SetPositionAndRotation(laserSplash, hitPos, hitDir);
                laserSplash.Emit(10);
            break;
            case GunType.Bullet:
                SetPositionAndRotation(bulletHit, hitPos, hitDir);
                bulletHit.Emit(10);
                SetPositionAndRotation(bulletSplash, hitPos, hitDir);
                bulletSplash.Emit(10);
            break;
        }
    }

    public void PlayBulletHitWall(Vector2 hitPos, Vector2 hitDir)
    {
        SetPositionAndRotation(bulletHitWall, hitPos, hitDir);
        bulletHitWall.Emit(10);

        SetPositionAndRotation(bulletHole, hitPos, hitDir);
        bulletHole.Emit(1);
    }

    public void PlayBulletBlood(Vector2 hitPos, Vector2 hitDir)
    {
        if (QualityManager.CurrentQualityLevel == QualityLevel.VeryLow) return;
        
        SetPositionAndRotation(bulletBlood, hitPos, hitDir);
        bulletBlood.Emit(6);
        SetPositionAndRotation(bloodSpray, hitPos, hitDir);
        bloodSpray.Emit(20);
        SetPositionAndRotation(bulletBloodFling, hitPos, hitDir);
        bulletBloodFling.Emit(7);
        SetPositionAndRotation(bulletBloodEntry, hitPos, hitDir);
        bulletBloodEntry.Emit(4);
    }

    #endregion

    #region Casings / Clips

    public void PlayRepairCasing_Gun(Transform ejectorTrans, Quaternion rot, bool isElevated)
    {
        SetPositionAndRotation(casingRepair, ejectorTrans.position, rot);
        casingRepair.Emit(6);
        SetPositionAndRotation(casingRepairDrip, ejectorTrans.position, rot);
        //int amount = Random.Range(1, 4);
        casingRepairDrip.Emit(1);
    }

    public void PlayLaserCasing_Pistol(Transform ejectorTrans, Quaternion rot, bool isElevated)
    {
        SetPositionAndRotation(casingLaser, ejectorTrans.position, rot);
        casingLaser.Emit(6);
        SetPositionAndRotation(casingLaserDrip, ejectorTrans.position, rot);
        //int amount = Random.Range(1, 3);
        casingLaserDrip.Emit(1);
        //Debug.Log("PlayerLaserCasing_Pistol ran");
    }

    public void PlayBulletCasingFly_SMG(Transform ejectorTrans, Quaternion rot, bool isElevated)
    {
        GameObject casingObj = PoolManager.Instance.GetFromPool("Casing_SMG");
        casingObj.transform.position = ejectorTrans.position;

        BulletCasingFly casingFly = casingObj.GetComponent<BulletCasingFly>();
        if (casingFly != null)
        {
            casingFly.casingTrans = ejectorTrans;
            casingFly.StartFly();
        }
        else 
            Debug.LogError("CasingFly component not found for " + casingPrefab_SMG + " in ParticleManager");

        // If elevated, all shells go over walls                
        if (isElevated)
        {
            casingObj.layer = LayerMask.NameToLayer("Default");
        }
    }

    public void PlayBulletCasingFly_Rifle(Transform ejectorTrans, Quaternion rot, bool isElevated)
    {
        GameObject casingObj = PoolManager.Instance.GetFromPool("Casing_GunTurret");
        casingObj.transform.position = ejectorTrans.position;

        BulletCasingFly casingFly = casingObj.GetComponent<BulletCasingFly>();
        if (casingFly != null)
        {
            casingFly.casingTrans = ejectorTrans;
            casingFly.StartFly();
        }
        else 
            Debug.LogError("CasingFly component not found for " + casingPrefab_Rifle + " in ParticleManager");

        // If elevated, all shells go over walls                
        if (isElevated)
        {
            casingObj.layer = LayerMask.NameToLayer("Default");
        }
    }

    public void PlayBulletCasing_SMG(Transform ejectorTrans, Quaternion rot, bool isElevated)
    {
        var emitParams = new ParticleSystem.EmitParams();
        emitParams.position = ejectorTrans.position;
        emitParams.rotation = -rot.eulerAngles.z;
        
        casingBullet_SMG.Emit(emitParams, 1);
    }

    public void PlayBulletCasing_Rifle(Transform ejectorTrans, Quaternion rot, bool isElevated)
    {
        var emitParams = new ParticleSystem.EmitParams();
        emitParams.position = ejectorTrans.position;
        emitParams.rotation = -rot.eulerAngles.z;
        
        casingBullet_SMG.Emit(emitParams, 1);
    }

    public void PlayClip_SMG(Transform ejectorTrans, Quaternion rot, bool isElevated)
    {
        var emitParams = new ParticleSystem.EmitParams();
        emitParams.position = ejectorTrans.position;
        emitParams.rotation = -rot.eulerAngles.z;

        clip_SMG.Emit(emitParams, 1);
    }

    public void PlayClip_Turret_Rifle(Transform ejectorTrans, Quaternion rot, bool isElevated)
    {
        var emitParams = new ParticleSystem.EmitParams();
        emitParams.position = ejectorTrans.position;
        emitParams.rotation = -rot.eulerAngles.z;

        clip_Turret_Rifle.Emit(emitParams, 1);
    }

    #endregion

    #region MeleeHits

    public void PlayMeleeHitWall(Vector2 hitPos, Vector2 hitDir)
    {
        SetPositionAndRotation(meleeHitWall, hitPos, hitDir);
        meleeHitWall.Emit(10);
    }

    public void PlayMeleeBlood(Vector2 hitPos, Vector2 hitDir)
    {
        if (QualityManager.CurrentQualityLevel == QualityLevel.VeryLow) return;

        //Debug.Log("MeleeBlood played");
        SetPositionAndRotation(meleeBlood, hitPos, hitDir);
        meleeBlood.Emit(11);
        SetPositionAndRotation(bloodSpray, hitPos, hitDir);
        bloodSpray.Emit(20);
    }

    #endregion
}
