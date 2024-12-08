using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Health : MonoBehaviour
{   
    public float startMaxHealth;
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float healthToCreditsDivisor = 1;
    public float deathTorque = 20;
    [SerializeField] SoundSO painSound;
    [SerializeField] float painSoundCooldown= 0.3f;
    [SerializeField] protected Color textFlyHealthGainColor;
    [SerializeField] protected Color textFlyHealthLossColor;
    [SerializeField] protected float textFlyAlphaMax = 0.25f;
    float lastPainSoundTime;

    protected float health;
    protected Rigidbody2D myRb;
    public bool hasDied;

    protected virtual void Awake()
    {   
        myRb = GetComponent<Rigidbody2D>();

        maxHealth = startMaxHealth;
        health = maxHealth;
    }

    public float GetHealth()
    {
        return health;
    }

    public void SetHealth(float value)
    {
        health = value;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public virtual void SetMaxHealth(float value)
    {
        maxHealth = value;
        //Debug.Log($"MaxHealth was set to {value}");
    }

    public virtual void ModifyHealth(float value, float textPulseScaleMax, Vector2 textDir, GameObject damageDealer)
    {
        //Debug.Log($"Modifying {health} health by {value}.  Max health: {maxHealth}");
        float healthChange = value;
        float healthStart = health;
        health += value;
        if (health > maxHealth)
        {
            health = maxHealth;
            healthChange = health - healthStart;
        }

        TextFly textFly = PoolManager.Instance.GetFromPool("TextFlyWorld_Health").GetComponent<TextFly>();
        textFly.transform.position = transform.position;
        if (healthChange < 0)
        {
            float angle = Random.Range(-30f, 30f) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(
                textDir.x * Mathf.Cos(angle) - textDir.y * Mathf.Sin(angle),
                textDir.x * Mathf.Sin(angle) + textDir.y * Mathf.Cos(angle)
            ).normalized;

            textFly.Initialize(Mathf.Abs(healthChange).ToString("#0"), textFlyHealthLossColor, textFlyAlphaMax, dir, true, textPulseScaleMax);
            PlayPainSound();
        }
        else
        {
            float angle = (Random.Range(-45f, 45f) + 90) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;

            textFly.Initialize("+" + healthChange.ToString("#0"), textFlyHealthGainColor, textFlyAlphaMax, dir, true, textPulseScaleMax);
        }

        // Stats counting
        if (damageDealer != null)
        {
            if (this.CompareTag("Enemy"))
                StatsCounterPlayer.DamageToEnemies -= value;
        }
        
        // Die
        if (health <= 0 && !hasDied)
        {
            if (damageDealer != null)
            {
                // Structure layer 13
                if (damageDealer.layer == 13)
                    StatsCounterPlayer.EnemiesKilledByTurrets++;
                else if (damageDealer.CompareTag("Player") || damageDealer.CompareTag("PlayerExplosion") || damageDealer.layer == 8)
                    StatsCounterPlayer.EnemiesKilledByPlayer++;
            }

            Die();
            return;
        }
    }

    protected void PlayPainSound()
    {
        if (painSound != null)
        {
            if (Time.time - lastPainSoundTime >= painSoundCooldown)
            {
                SFXManager.Instance.PlaySoundAt(painSound, transform.position);
                lastPainSoundTime = Time.time;
            }
        }
    }

    public void Knockback(Vector2 dir, float knockback)
    {
        myRb.AddForce(dir * knockback, ForceMode2D.Impulse);
    }

    public virtual void Die() { }
}
