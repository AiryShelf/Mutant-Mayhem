using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Rendering.Universal;

public class MeleeControllerPlayer : MonoBehaviour
{
    [SerializeField] float damage;
    [SerializeField] float knockback;
    public float meleeAttackRate = 0.5f;
    [SerializeField] float meleeStaminaUse = 15f;
    [SerializeField] SpriteRenderer swordSR;
    public SwordController swordController;
    [SerializeField] PlayerShooter playerShooter;
    [SerializeField] AnimationControllerPlayer animationControllerPlayer;
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

    public void MeleeColliderToggle(bool on)
    {
        if (on)
        {
            swordController.polyCollider.enabled = true;
        }
        else
        {
            swordController.polyCollider.enabled = false;
        }
    }

    public void UseStamina()
    {
        myStamina.ModifyStamina(-meleeStaminaUse);
    }

    public void Hit(Collider2D other, Vector2 point)
    {
        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                //enemy.IsHit();
                health.Knockback((Vector2)health.transform.position - point, knockback);
                health.MeleeHitEffect(point, transform.right);
                health.ModifyHealth(-damage);
            }
        }
    }

}
