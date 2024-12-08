using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class PlayerBodyAnimEventReceiver : MonoBehaviour
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
        playerShooter.ReloadPlayer();
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
        meleeControllerPlayer.SwordControllerToggle(true);

        meleeControllerPlayer.PlayMeleeSwingSound();
    }

    void OnMeleeAttackStamina()
    {
        meleeControllerPlayer.UseStaminaAndAccuracy();
    }

    void OnMeleeAttackEnd()
    {
        meleeControllerPlayer.SwordControllerToggle(false);
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

    void OnReloadSound(int index)
    {
        playerShooter.OnReloadSound(index);
    }
}
