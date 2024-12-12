using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Planet_New", menuName = "Game/Planet")]
public class Planet : ScriptableObject
{
    public string planetName;
    public string description;
    public GameObject icon;
    public GameObject propertyPrefab;
    public List<PlanetPropertySO> properties;
    public Mission mission;

    [Header("Research Points:")]
    public int basePoints = 0;
    public int pointsPerWave = 20;
    public float growthControlFactor = 0.05f;
    public int difficultyAdjustHard = 10; // These two are depricated
    public int difficultyAdjustEasy = -10; // " "

    [Header("WaveController Settings:")]
    [Header("Wave Properties")]
    public float timeBetweenWavesBase = 90; // Base amount of day-time
    public float wavesTillAddBase = 1; 
    public float subwaveDelayMultStart = 1f;
    public int spawnRadiusBuffer = 16;
    [Header("Enemy Multipliers")]
    public int batchMultiplierStart = 5; // Starting batch multiplier for each Subwave
    public float damageMultiplier = 1;
    public float healthMultiplier = 1;
    public float speedMultiplier = 1;
    public float sizeMultiplier = 1; // Smaller is harder
}
