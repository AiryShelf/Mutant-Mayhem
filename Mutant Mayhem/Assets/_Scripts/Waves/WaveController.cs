using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveController : MonoBehaviour
{
    public int currentWave = 0;
    public float spawnRadius = 60;

    public int batchMultiplier = 1;
    public float damageMultiplier = 1;
    public float healthMultiplier = 1;
    public float spawnSpeedMultiplier = 1;

    public static bool isActive;

    void Start()
    {
        isActive = true;
    }
}
