using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Rendering.Universal;

public class MeleeControllerPlayer : MonoBehaviour
{
    public PlayerStats stats;
    
    [SerializeField] float meleeStaminaUse = 15f;
    [SerializeField] SpriteRenderer swordSR;
    public SwordController swordController;
    public PlayerShooter playerShooter;
    [SerializeField] AnimationControllerPlayer animationControllerPlayer;
    [SerializeField] SoundSO swordSwingSound;
    [SerializeField] SoundSO swordHitSound;
    [SerializeField] float meleeSoundCooldown = 0.1f;
    float lastMeleeSoundTime;
    public Stamina myStamina;
    
    void Start()
    {
        swordSR.enabled = false;
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
        AudioManager.Instance.PlaySoundAt(swordSwingSound, transform.position);
    }

    public void SwordControllerToggle(bool on)
    {
        if (on)
        {
            swordController.gameObject.SetActive(true);
        }
        else
        {
            swordController.gameObject.SetActive(false);
        }
    }

    public void UseStamina()
    {
        myStamina.ModifyStamina(-meleeStaminaUse);
        StatsCounterPlayer.MeleeAttacksByPlayer++;
    }

    public void Hit(Collider2D other, Vector2 point)
    {
        if (Time.time - lastMeleeSoundTime >= meleeSoundCooldown)
        {
            AudioManager.Instance.PlaySoundAt(swordHitSound, point);
            lastMeleeSoundTime = Time.time;
        }

        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy != null)
        {     
            enemy.Knockback((Vector2)enemy.transform.position - point, stats.knockback);
            enemy.MeleeHitEffect(point, transform.right);
            enemy.ModifyHealth(-stats.meleeDamage, gameObject);
            enemy.StartFreeze();

            StatsCounterPlayer.MeleeDamageByPlayer += stats.meleeDamage;
            StatsCounterPlayer.MeleeHitsByPlayer++;
        }
    }

}
