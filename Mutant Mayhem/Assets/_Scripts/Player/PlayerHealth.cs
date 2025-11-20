using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : Health
{
    [SerializeField] Player player;
    public float healthRegenPerSec = 0.5f;
    [SerializeField] float hitAccuracyLoss = 3f;

    [Header("Camera Shake On Hit")]
    [SerializeField] float minShakeIntensity = 0.1f;
    [SerializeField] float maxShakeIntensity = 0.8f;
    [SerializeField] float minShakeDuration = 0.08f;
    [SerializeField] float maxShakeDuration = 0.25f;
    public event Action<float> OnPlayerHealthChanged;
    public event Action<float> OnPlayerMaxHealthChanged;

    void Start()
    {
        StartCoroutine(HealthRegen());
    }

    public override void ModifyHealth(float value, float textPulseScaleMax, Vector2 hitDir, GameObject damageDealer)
    {
        if (player.IsDead)
            return;

        base.ModifyHealth(value, textPulseScaleMax, hitDir, damageDealer);

        // Stats counting
        if (value < 0)
        {
            // Screen shake based on damage ratio
            float damageAmount = -value; // value is negative for damage
            float damageRatio = Mathf.Clamp01(damageAmount / (health / 4f));

            float shakeIntensity = Mathf.Lerp(minShakeIntensity, maxShakeIntensity, damageRatio);
            float shakeDuration = Mathf.Lerp(minShakeDuration, maxShakeDuration, damageRatio);

            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(shakeIntensity, shakeDuration);
            }

            StatsCounterPlayer.DamageToPlayer -= healthChange;
            player.playerShooter.currentAccuracy += hitAccuracyLoss * player.stats.weaponHandling;
        }
        
        if (health <= 0 && !player.IsDead)
        {
            AnalyticsManager.Instance.TrackPlayerDeath(damageDealer.name);
            Die();
            return;
        }

        OnPlayerHealthChanged.Invoke(health);
    }

    public override void SetMaxHealth(float value)
    {
        maxHealth = value;
        OnPlayerMaxHealthChanged.Invoke(health);
    }

    IEnumerator HealthRegen()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);

            // Regenerate
            if (health < maxHealth)
            {
                ModifyHealth(healthRegenPerSec, 1, Vector2.one, gameObject);
            }
        }
    }

    public override void Die()
    {
        StopAllCoroutines();

        //Debug.Log("PLAYER IS DEAD");
        hasDied = true;
        player.IsDead = true;
        myRb.freezeRotation = false;
        // Random flip 
        int sign = UnityEngine.Random.Range(0, 2) * 2 - 1; // Randomly 1 or -1
        myRb.AddTorque(sign * deathTorque);
    } 
}
