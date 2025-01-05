using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ShootingPlatform : MonoBehaviour
{
    Light2D flashlight1;
    Light2D flashlight2;

    PlayerShooter playerShooter;

    void Start()
    {
        playerShooter = FindObjectOfType<PlayerShooter>();
        flashlight1 = playerShooter.flashlight1;
        flashlight2 = playerShooter.flashlight2;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered platform");
            playerShooter.isElevated = true;
            playerShooter.gunSights.isElevated = true;
            flashlight1.shadowsEnabled = false;
            flashlight2.shadowsEnabled = false;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exitted platform");
            playerShooter.isElevated = false;
            playerShooter.gunSights.isElevated = false;
            flashlight1.shadowsEnabled = true;
            flashlight2.shadowsEnabled = true;
        }
    }
}
