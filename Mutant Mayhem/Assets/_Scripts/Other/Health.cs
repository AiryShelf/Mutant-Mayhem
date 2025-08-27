using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float startMaxHealth;
    [SerializeField] protected float maxHealth = 100f;
    public float deathTorque = 20;
    public SoundSO painSound;
    [SerializeField] float painSoundCooldown = 0.3f;
    [SerializeField] protected Color textFlyHealthGainColor;
    [SerializeField] protected Color textFlyHealthLossColor;
    [SerializeField] protected float textFlyAlphaMax = 0.8f;
    float lastPainSoundTime;

    protected float health;
    protected float healthChange;
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
        healthChange = value;
        float healthStart = health;
        health += value;
        if (health > maxHealth)
        {
            health = maxHealth;
            healthChange = health - healthStart;
        }

        if (healthChange < 0)
        {
            SendTextFly(textDir, textFlyHealthLossColor, textPulseScaleMax);
            PlayPainSound();
        }
        else
        {
            float angle = (Random.Range(-45f, 45f) + 90) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;

            //SendTextFly(textDir, textFlyHealthGainColor, textPulseScaleMax);
        }
    }

    protected void PlayPainSound()
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

    public void Knockback(Vector2 dir, float knockback)
    {
        myRb.AddForce(dir * knockback, ForceMode2D.Impulse);
    }

    protected void SendTextFly(Vector2 textDir, Color color, float scaleMax)
    {
        TextFly textFly = PoolManager.Instance.GetFromPool("TextFlyWorld_Health").GetComponent<TextFly>();
        textFly.transform.position = transform.position;

        float angle = Random.Range(-30f, 30f) * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(
            textDir.x * Mathf.Cos(angle) - textDir.y * Mathf.Sin(angle),
            textDir.x * Mathf.Sin(angle) + textDir.y * Mathf.Cos(angle)
        ).normalized;

        textFly.Initialize(Mathf.Abs(healthChange).ToString("#0"), color,
                           textFlyAlphaMax, dir, true, scaleMax);
    }

    #region Corpse and Die

    protected void SetBasicCorpse(string poolName, Color color)
    {
        // Pass scale and position
        GameObject corpseObj = PoolManager.Instance.GetFromPool(poolName);
        corpseObj.transform.position = transform.position;
        corpseObj.transform.rotation = transform.rotation;
        corpseObj.transform.localScale = transform.localScale * 0.9f; // Scale down a bit

        var corpse = corpseObj.GetComponent<CorpseController>();
        if (corpse != null)
        {
            // Pass list of possible corpse sprites.  This is for enemies, drones, and other simple corpses
            if (corpse is BasicCorpseController basicCorpse)
            {
                if (this is EnemyHealth enemyHealth)
                    basicCorpse.corpseSprites = enemyHealth.corpseSpritesForEnemy;
                else if (this is DroneHealth droneHealth)
                    basicCorpse.corpseSprites = droneHealth.corpseSpritesForDrone;
                else
                    Debug.LogError("Acceptable Health component not found on object, cannot set corpse sprites.");

                basicCorpse.SetSpriteAndColor(color);
            }

            corpse.corpsePoolName = poolName;
        }
    }

    protected void SetMutantCorpse(string poolName)
    {
        // Pass scale and position
        GameObject corpse = PoolManager.Instance.GetFromPool(poolName);
        corpse.transform.position = transform.position;
        corpse.transform.rotation = transform.rotation;
        corpse.transform.localScale = transform.localScale * 0.9f; // Scale down a bit

        var mutantCorpse = corpse.GetComponent<MutantCorpseController>();
        if (mutantCorpse != null)
        {
            // Apply genome to corpse
            mutantCorpse.ApplyGenome(GetComponent<EnemyMutant>().individual.genome);

            mutantCorpse.corpsePoolName = poolName;
        }
    }

    public virtual void Die() { }
    
    #endregion
}
