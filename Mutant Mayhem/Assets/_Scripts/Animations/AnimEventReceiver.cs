using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnimEventReceiver : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] PlayerShooter playerShooter;
    [SerializeField] Animator bodyAnim;
    [SerializeField] AnimationControllerPlayer animControllerPlayer;
    [SerializeField] MeleeControllerPlayer meleeControllerPlayer;

    void OnPlayerDie()
    {
        animControllerPlayer.DeathAnimEnd();
        playerShooter.DropGun();
        
    }

    void OnSwitchGunsStart()
    {
        animControllerPlayer.SwitchGunsAnimationPlaying(true);
    }

    void OnSwitchGuns()
    {
        playerShooter.SwitchGuns(bodyAnim.GetInteger("gunIndex"));
    }

    void OnSwitchGunsEnd()
    {
        animControllerPlayer.SwitchGunsAnimationPlaying(false);
    }

    void OnReloadStart()
    {
        animControllerPlayer.ReloadAnimationPlaying(true);
    }

    void OnDropClip()
    {
        playerShooter.DropClip();
    }

    void OnReloaded()
    {
        playerShooter.Reload();
    }

    void OnReloadEnd()
    {
        animControllerPlayer.ReloadAnimationPlaying(false);
    }

    void OnAnimIdle()
    {
        playerShooter.StopAiming();
        animControllerPlayer.MeleeAnimationPlaying(false);
        animControllerPlayer.ThrowAnimationPlaying(false);
    }

    void OnAnimAimed()
    {
        playerShooter.StartAiming();
        animControllerPlayer.MeleeAnimationPlaying(false);
        animControllerPlayer.ThrowAnimationPlaying(false);
    }

    void OnMeleeAttackStart()
    {
        animControllerPlayer.MeleeAnimationPlaying(true);
        meleeControllerPlayer.MeleeColliderToggle(true);
    }

    void OnMeleeAttackStamina()
    {
        meleeControllerPlayer.UseStamina();
    }

    void OnMeleeAttackEnd()
    {
        meleeControllerPlayer.MeleeColliderToggle(false);
    }

    void OnThrowStart()
    {
        animControllerPlayer.ThrowAnimationPlaying(true);
    }

    void OnThrowGrab()
    {
        player.OnThrowGrab();
    }

    void OnThrowFly()
    {
        player.OnThrowFly();
    }
}
