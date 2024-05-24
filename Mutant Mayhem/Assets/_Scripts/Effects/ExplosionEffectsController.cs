using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ExplosionEffectsController : MonoBehaviour
{
    [SerializeField] Light2D explosionFlash;
    [SerializeField] GameObject explosionFlashObject;
    [SerializeField] float flashDisappearRate = 0.1f;
    [SerializeField] float effectDestroyTime = 60;
    
    void Start()
    {
        Destroy(gameObject, effectDestroyTime);
    }

    void FixedUpdate()
    {
        if (explosionFlashObject != null)
        {
            explosionFlash.intensity -= flashDisappearRate;
            if (explosionFlash.intensity <= 0)
            {
                explosionFlash.enabled = false;
                Destroy(explosionFlashObject);
                explosionFlashObject = null;
            }
        }
        
    }
}
