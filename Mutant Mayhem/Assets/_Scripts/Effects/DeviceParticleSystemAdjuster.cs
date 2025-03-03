using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceParticleSystemAdjuster : MonoBehaviour
{
    [SerializeField] ParticleSystem psToAdjust;
    [SerializeField] float lifetimeFactor;

    void Start()
    {
        if (InputManager.IsMobile())
        {
            var main = psToAdjust.main;
            main.startLifetimeMultiplier = lifetimeFactor;
        } 
    }
}
