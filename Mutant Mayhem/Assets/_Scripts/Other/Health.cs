using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{   
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] float healthToCreditsDivisor = 5;
    [SerializeField] HitEffects hitEffectsChild;
    [SerializeField] GameObject corpsePrefab;
    public float deathTorque = 20;

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

    public virtual void ModifyHealth(float value, GameObject other)
    {
        health += value;
        if (health > maxHealth)
            health = maxHealth;

        // Stats counting
        // Layer# 8 - PlayerProjectiles.  player, enemy
        if (other != null)
        {
            if (other.layer == 8)
            {
                StatsCounterPlayer.EnemyDamageByPlayerProjectiles -= value;
                Debug.Log("Player prjectile damage: " + value);
            }
            else if (this.tag == "Enemy")
                StatsCounterPlayer.DamageToEnemies -= value;
            else if (this.tag == "Player")
            StatsCounterPlayer.DamageToPlayer -= value;
        }
        
        // Die
        if (health <= 0 && !hasDied)
        {
            if (other != null)
            {
                // Structure layer 13
                if (other.layer == 13)
                    StatsCounterPlayer.EnemiesKilledByTurrets++;
                else if (other.tag == "Player" || other.tag == "PlayerExplosion" || other.layer == 8)
                    StatsCounterPlayer.EnemiesKilledByPlayer++;
            }

            Die();
            return;
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
            // Create corpse and pass inhertance
            corpsePrefab = Instantiate(corpsePrefab, transform.position, transform.rotation);
            corpsePrefab.transform.localScale = transform.localScale;
            // Pass physics
            Rigidbody2D corpseRb = corpsePrefab.GetComponent<Rigidbody2D>();
            corpseRb.velocity = myRb.velocity;
            corpseRb.angularVelocity = myRb.angularVelocity;
        }
        
        corpsePrefab.GetComponentInChildren<SpriteRenderer>().color = 
                                                GetComponent<SpriteRenderer>().color;
        hitEffectsChild.transform.parent = null;
        hitEffectsChild.DestroyAfterSeconds();

        // Player Credits
        BuildingSystem.PlayerCredits += Mathf.Floor(maxHealth / healthToCreditsDivisor);
        
        WaveSpawner.EnemyCount--;
        Destroy(gameObject);   
    }
}
