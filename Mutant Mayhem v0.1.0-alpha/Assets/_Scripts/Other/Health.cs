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

    public virtual void ModifyHealth(float value, GameObject damageDealer)
    {
        //Debug.Log($"Modifying {health} health by {value}.  Max health: {maxHealth}");
        PlayPainSound(value);

        health += value;
        if (health > maxHealth)
            health = maxHealth;

        // Stats counting
        // Layer# 8 - PlayerProjectiles.  player, enemy
        if (damageDealer != null)
        {
            if (damageDealer.layer == 8)
            {
                StatsCounterPlayer.EnemyDamageByPlayerProjectiles -= value;
                StatsCounterPlayer.ShotsHitByPlayer++;
                //Debug.Log("Player prjectile damage: " + value);
            }
            else if (this.CompareTag("Enemy"))
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

    protected void PlayPainSound(float value)
    {
        if (value < 0)
        {
            if (painSound != null)
            {
                if (Time.time - lastPainSoundTime >= painSoundCooldown)
                {
                    AudioManager.Instance.PlaySoundAt(painSound, transform.position);
                    lastPainSoundTime = Time.time;
                }
            }
        }
    }

    public void Knockback(Vector2 dir, float knockback)
    {
        myRb.AddForce(dir * knockback, ForceMode2D.Impulse);
    }

    public virtual void Die() { }
}
