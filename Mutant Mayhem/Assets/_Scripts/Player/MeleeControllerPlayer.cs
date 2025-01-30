using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Rendering.Universal;

public class MeleeControllerPlayer : MonoBehaviour
{
    public PlayerStats stats;
    
    [SerializeField] float meleeStaminaUse = 15f;
    [SerializeField] float meleeAccuracyLoss = 6f;
    [Range(0, 1)]
    [SerializeField] float damageVariance = 0.15f;
    [SerializeField] SpriteRenderer swordSR;
    public SwordController swordController;
    public PlayerShooter playerShooter;
    [SerializeField] AnimationControllerPlayer animationControllerPlayer;
    [SerializeField] SoundSO swordSwingSound;
    [SerializeField] SoundSO swordHitSound;
    [SerializeField] float meleeSoundCooldown = 0.1f;
    float lastMeleeSoundTime;
    public Stamina myStamina;
    CriticalHit criticalHit;
    
    void Start()
    {
        swordSR.enabled = false;
        criticalHit = GetComponent<CriticalHit>();
    }

    void Update()
    {
        if (meleeStaminaUse <= myStamina.GetStamina())
        {
            animationControllerPlayer.bodyAnim.SetBool("hasMeleeStamina", true);
        }
        else
        {
            animationControllerPlayer.bodyAnim.SetBool("hasMeleeStamina", false);
        }
    }

    public void PlayMeleeSwingSound()
    {
        SFXManager.Instance.PlaySoundAt(swordSwingSound, transform.position);
    }

    public void SwordControllerToggle(bool on)
    {
        if (on)
        {
            swordSR.enabled = true;
            swordController.gameObject.SetActive(true);
        }
        else
        {
            swordSR.enabled = false;
            swordController.gameObject.SetActive(false);
        }
    }

    public void UseStaminaAndAccuracy()
    {
        myStamina.ModifyStamina(-meleeStaminaUse);
        StatsCounterPlayer.MeleeAttacksByPlayer++;

        stats.playerShooter.currentAccuracy += meleeAccuracyLoss * playerShooter.playerStats.weaponHandling;
    }

    public void Hit(Collider2D other, Vector2 point)
    {
        // Half critical chance for player melee
        bool isCritical = false;
        float critMult = 1;
        if (criticalHit != null)
            (isCritical, critMult) = criticalHit.RollForCrit(playerShooter.playerStats.criticalHitChanceMult / 2,
                                                             playerShooter.playerStats.criticalHitDamageMult);
        
        float damage = stats.meleeDamage;
        damage *= 1 + Random.Range(-damageVariance, damageVariance);
        damage *= critMult;

        if (Time.time - lastMeleeSoundTime >= meleeSoundCooldown)
        {
            SFXManager.Instance.PlaySoundAt(swordHitSound, point);
            lastMeleeSoundTime = Time.time;
        }

        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            Vector2 hitDir = (Vector2)enemy.transform.position - point;
            enemy.Knockback(hitDir, stats.knockback);
            enemy.StartFreeze();

            float damageScale = damage / stats.meleeDamage;
            enemy.ModifyHealth(-damage, damageScale, hitDir, gameObject);

            // Should play a melee hit effect, like laser sword lightning here *****
            ParticleManager.Instance.PlayMeleeBlood(point, transform.right);

            StatsCounterPlayer.MeleeDamageByPlayer += damage;
            StatsCounterPlayer.MeleeHitsByPlayer++;
        }
        else
        {
            Debug.LogError("EnemyBase not found on melee hit from player");
        }
    }

}
