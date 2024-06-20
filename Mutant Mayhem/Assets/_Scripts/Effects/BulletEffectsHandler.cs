using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletEffectsHandler : MonoBehaviour
{
    [SerializeField] float timeToDestroyParticleTrail;
    [SerializeField] float timeToDestroyLineRenderer;
    [SerializeField] float timeToDestroyAll;
    [SerializeField] ParticleSystem bulletHitEffect;
    [SerializeField] ParticleSystem bulletHitEffectSplash;
    [SerializeField] ParticleSystem particleTrail;
    [SerializeField] TrailRenderer trailRenderer;
    [SerializeField] int amountOfParticles = 10;
    [SerializeField] int amountOfParticlesSplash = 10;

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

    public void PlayBulletHitEffectAt(Vector2 pos)
    {
        if (bulletHitEffect != null)
        {
            bulletHitEffect.transform.position = pos;
            bulletHitEffectSplash.transform.position = pos;
            bulletHitEffect.Emit(amountOfParticles);
            bulletHitEffectSplash.Emit(amountOfParticlesSplash);
        }
    }
}
