using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] float health = 100f;
    [SerializeField] HitEffects hitEffectsChild;
    [SerializeField] GameObject corpsePrefab;
    [SerializeField] float deathTorque = 20;

    float maxHealth;
    Rigidbody2D myRb;
    Player player;
    bool hasDied;


    void Awake()
    {
        player = GetComponent<Player>();
        myRb = GetComponent<Rigidbody2D>();
        maxHealth = health;
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

    public void SetMaxHealth(float value)
    {
        maxHealth = value;
    }

    public void ModifyHealth(float value, GameObject obj)
    {
        health += value;

        // Layer# 8 - PlayerProjectiles, player, enemies
        if (obj.layer == 8)
            StatsCounterPlayer.EnemyDamageByPlayerProjectiles -= value;
        if (this.tag == "Player")
            StatsCounterPlayer.DamageToPlayer -= value;
        if (this.tag == "Enemy")
            StatsCounterPlayer.DamageToEnemies -= value;

        if (health <= 0 && !hasDied)
        {
            if (obj.layer == 8 || obj.tag == "PlayerExplosion")
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

    public void Die()
    {
        hasDied = true;
        // Create corpse and pass inhertance
        corpsePrefab = Instantiate(corpsePrefab, transform.position, transform.rotation);
        corpsePrefab.transform.localScale = transform.localScale;
        Rigidbody2D corpseRb = corpsePrefab.GetComponent<Rigidbody2D>();
        corpseRb.velocity = myRb.velocity;
        corpseRb.angularVelocity = myRb.angularVelocity;
        
        if (player == null)
        {
            corpsePrefab.GetComponentInChildren<SpriteRenderer>().color = 
                                                    GetComponent<SpriteRenderer>().color;
            hitEffectsChild.transform.parent = null;
            hitEffectsChild.DestroyAfterSeconds();
            
            EnemySpawner.GLOBAL_enemyCount--;
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("PLAYER IS DEAD");
            player.isDead = true;
            myRb.freezeRotation = false;
            // Random flip 
            int sign = Random.Range(0, 2) * 2 - 1; // Randomly 1 or -1
            myRb.AddTorque(sign * deathTorque);
        }   
    }
}
