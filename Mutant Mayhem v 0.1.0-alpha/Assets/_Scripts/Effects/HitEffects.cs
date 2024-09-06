using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffects : MonoBehaviour
{
    [SerializeField] ParticleSystem bulletHitEffect;
    [SerializeField] ParticleSystem meleeHitEffect;
    [SerializeField] float EffectsDestroyTime = 60f;

    public void PlayBulletHitEffect(Vector2 hitPos, Vector2 hitDir)
    {
        transform.position = hitPos;
        float angle = Mathf.Atan2(hitDir.y, hitDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + 180);

        bulletHitEffect.Play();
    }

    public void PlayMeleeHitEffect(Vector2 hitPos, Vector2 hitDir)
    {
        transform.position = hitPos;
        float angle = Mathf.Atan2(hitDir.y, hitDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + 180);

        meleeHitEffect.Play();
    }

    public void DestroyAfterSeconds()
    {
        Destroy(gameObject, EffectsDestroyTime);
    }
}

