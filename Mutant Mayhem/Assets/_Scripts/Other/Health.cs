using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{   
    [SerializeField] protected float maxHealth = 100f;
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

    public void ModifyHealth(float value, GameObject other)
    {
        health += value;

        // Stats counting
        // Layer# 8 - PlayerProjectiles, player, enemy
        if (other.layer == 8)
            StatsCounterPlayer.EnemyDamageByPlayerProjectiles -= value;
        else if (this.tag == "Enemy")
            StatsCounterPlayer.DamageToEnemies -= value;
        else if (this.tag == "Player")
            StatsCounterPlayer.DamageToPlayer -= value;
        
        if (health <= 0 && !hasDied)
        {
            // Structure layer 13
            if (other.layer == 13)
                StatsCounterPlayer.EnemiesKilledByTurrets++;
            else if (other.tag == "Player" || other.tag == "PlayerExplosion" || other.layer == 8)
                StatsCounterPlayer.EnemiesKilledByPlayer++;

            Die();
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
        
        WaveSpawner.EnemyCount--;
        BuildingSystem.PlayerCredits += Mathf.Floor(maxHealth / 10);
        Destroy(gameObject);
        
           
    }
}
