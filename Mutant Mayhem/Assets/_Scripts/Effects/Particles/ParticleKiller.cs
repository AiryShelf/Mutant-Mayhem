using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleKiller : MonoBehaviour
{
    ParticleSystem pS;
    float destroyTime;

    void Start()
    {
        pS = GetComponent<ParticleSystem>();
        destroyTime = pS.main.startLifetime.constantMax;
        Destroy(gameObject, destroyTime);
    }
}
