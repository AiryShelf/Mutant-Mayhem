using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletEffectsHandler : MonoBehaviour
{
    [SerializeField] float timeToDestroyParticleTrail;
    [SerializeField] float timeToDestroyLineRenderer;
    [SerializeField] float timeToDestroyAll;
    [SerializeField] ParticleSystem particleTrail;
    [SerializeField] TrailRenderer trailRenderer;

    public void DestroyAfterSeconds()
    {
        transform.parent = null;

        if (particleTrail != null)
        {
            // Bullet particle trail
            ParticleSystem.EmissionModule emitter = particleTrail.emission;
            emitter.enabled = false;
            Destroy(particleTrail.gameObject, timeToDestroyParticleTrail);
        }

        if (trailRenderer != null)
        {
            // Trail Renderer
            trailRenderer.emitting = false;
            Destroy(trailRenderer.gameObject, timeToDestroyLineRenderer);
        }

        // Destroys this game object last
        Destroy(gameObject, timeToDestroyAll);
    }
}
