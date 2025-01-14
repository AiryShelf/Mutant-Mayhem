using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : Health
{
    [SerializeField] Player player;
    public float healthRegenPerSec = 0.5f;
    [SerializeField] float hitAccuracyLoss = 3f;
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

        health += value;
        if (health > maxHealth)
            health = maxHealth;

        // Stats counting
        if (value < 0)
        {
            TextFly textFly = PoolManager.Instance.GetFromPool("TextFlyWorld_Health").GetComponent<TextFly>();
            textFly.transform.position = transform.position;
            float angle = UnityEngine.Random.Range(-45f, 45f) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(
                hitDir.x * Mathf.Cos(angle) - hitDir.y * Mathf.Sin(angle),
                hitDir.x * Mathf.Sin(angle) + hitDir.y * Mathf.Cos(angle)
            ).normalized;

            textFly.Initialize(Mathf.Abs(value).ToString("#0"), textFlyHealthLossColor, textFlyAlphaMax, dir, true, textPulseScaleMax);

            PlayPainSound();
            StatsCounterPlayer.DamageToPlayer -= value;

            player.playerShooter.currentAccuracy += hitAccuracyLoss * player.stats.weaponHandling;
        }
        
        // Die
        if (health <= 0 && !player.IsDead)
        {
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
        //Debug.Log("PLAYER IS DEAD");
        hasDied = true;
        player.IsDead = true;
        myRb.freezeRotation = false;
        // Random flip 
        int sign = UnityEngine.Random.Range(0, 2) * 2 - 1; // Randomly 1 or -1
        myRb.AddTorque(sign * deathTorque);
    } 
}
