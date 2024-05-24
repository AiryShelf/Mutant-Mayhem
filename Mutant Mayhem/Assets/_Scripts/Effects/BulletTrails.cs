using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTrails : MonoBehaviour
{
    [SerializeField] float timeToDestroy;

    ParticleSystem bulletTrail;

    private void Awake() 
    {
        bulletTrail = GetComponent<ParticleSystem>();
    }

    public void DestroyAfterSeconds()
    {
        ParticleSystem.EmissionModule emitter = bulletTrail.emission;
        emitter.enabled = false;
        Destroy(gameObject, timeToDestroy);
    }
}
