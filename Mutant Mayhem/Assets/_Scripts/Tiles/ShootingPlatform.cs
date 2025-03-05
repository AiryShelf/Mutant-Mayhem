using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ShootingPlatform : MonoBehaviour
{
    PlayerShooter playerShooter;

    void Start()
    {
        playerShooter = FindObjectOfType<PlayerShooter>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug.Log("Player entered platform");
            playerShooter.SetElevated(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug.Log("Player exitted platform");
            playerShooter.SetElevated(false);
        }
    }
}
