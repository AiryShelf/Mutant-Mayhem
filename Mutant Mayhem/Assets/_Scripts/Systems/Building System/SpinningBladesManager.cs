using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningBladesManager : MonoBehaviour
{
    public static SpinningBladesManager Instance;

    public float upgradeMultPerLevel = 0.1f;
    public float multTracker = 1f;

    [Header("Spinning Blades Settings")]
    public float rotationForce = 37500f;
    public float maxAngularVelocity = 1200f;
    public float minAngularVelocityToDamage = 300f;
    public float damagePerAngularVel = 0.0833f;
    public float damageInterval = 0.5f;
    public float selfDamagePerAngularVel = 0.01f;

    [Header("Shard Explosion")]
    public bool enableShardExplosion = true;
    public string shardBulletPoolName = "BulletSpinningBlade_Shard";
    [SerializeField, Min(1)] public int shardCount = 6;
    public float minAngularVelForShards = 600f;
    public float shardMinSpeed = 4f;
    public float shardMaxSpeed = 12f;
    public float shardMinDamage = 20f;
    public float shardMaxDamage = 60f;
    public float shardLifeTime = 0.8f;
    public float shardKnockback = 50f;
    [SerializeField, Range(0f, 360f)] public float shardStartAngleRandomOffset = 90f;
    [SerializeField, Range(0f, 30f)] public float shardAngleJitter = 10f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpgradeStats()
    {
        multTracker *= 1 + upgradeMultPerLevel;
        rotationForce *= 1 + upgradeMultPerLevel;
        maxAngularVelocity *= 1 + upgradeMultPerLevel;
        //damagePerAngularVel *= 1 + upgradeMultPerLevel;

        // Shard stats
        shardCount++;
        shardMinSpeed *= 1 + upgradeMultPerLevel / 2;
        shardMaxSpeed *= 1 + upgradeMultPerLevel / 2;
        shardMinDamage *= 1 + upgradeMultPerLevel;
        shardMaxDamage *= 1 + upgradeMultPerLevel;
        shardKnockback *= 1 + upgradeMultPerLevel;
    }
}
