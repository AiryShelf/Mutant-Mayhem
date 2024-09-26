using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Health : MonoBehaviour
{   
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] float healthToCreditsDivisor = 1;
    [SerializeField] HitEffects hitEffectsChild;
    [SerializeField] GameObject corpsePrefab;
    public float deathTorque = 20;
    [SerializeField] SoundSO painSound;
    [SerializeField] float painSoundCooldown= 0.3f;
    float lastPainSoundTime;

    protected float health;
    protected Rigidbody2D myRb;
    protected bool hasDied;

    void Awake()
    {
        myRb = GetComponent<Rigidbody2D>();
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
    }

    public virtual void ModifyHealth(float value, GameObject damageDealer)
    {
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

    public void BulletHitEffect(Vector2 hitPos, Vector2 hitDir)
    {
        hitEffectsChild.PlayBulletHitEffect(hitPos, hitDir);
    }

    public void MeleeHitEffect(Vector2 hitPos, Vector2 hitDir)
    {
        hitEffectsChild.PlayMeleeHitEffect(hitPos, hitDir);
    }

    public void Knockback(Vector2 dir, float knockback)
    {
        myRb.AddForce(dir * knockback, ForceMode2D.Impulse);
    }

    public virtual void Die()
    {
        if (corpsePrefab)
        {
            hasDied = true;
            // Create corpse and pass scale
            corpsePrefab = Instantiate(corpsePrefab, transform.position, transform.rotation);
            corpsePrefab.transform.localScale = transform.localScale;
            // Pass physics to corpse
            Rigidbody2D corpseRb = corpsePrefab.GetComponent<Rigidbody2D>();
            corpseRb.velocity = myRb.velocity;
            corpseRb.angularVelocity = myRb.angularVelocity;
            corpseRb.mass = myRb.mass * 2;
        }
        
        corpsePrefab.GetComponentInChildren<SpriteRenderer>().color = 
                                                GetComponent<SpriteRenderer>().color;
        hitEffectsChild.transform.parent = null;
        hitEffectsChild.DestroyAfterSeconds();

        // Player Credits
        BuildingSystem.PlayerCredits += Mathf.FloorToInt(maxHealth / healthToCreditsDivisor * 
                                        SettingsManager.Instance.CreditsMult);
        
        EnemyCounter.EnemyCount--;
        Destroy(gameObject);   
    }
}
