using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Planet_New", menuName = "Game/Planets/Planet")]
public class PlanetSO : ScriptableObject
{
    public string typeOfBody = "Planet/Moon";
    public string bodyName;
    public string description;
    public List<PlanetPropertySO> properties;
    public Tile terrainTile;
    public List<PlanetSO> prerequisitePlanets;
    public MissionSO mission;
    public DialogueSO planetDialogue;
    public int nightToSurvive = 10;
    public Color globalLightColor = Color.white;
    public float globalLightIntensity = 0.02f;
    public GameObject highRezPlanetPrefab;
    

    [Header("Research Points:")]
    public bool isTutorialPlanet = false;
    public int pointsPerWave = 20;
    public float growthControlFactor = 0.05f;

    [Header("WaveController Settings:")]
    [Header("Wave Properties")]
    public WaveSOBase waveSOBase;
    public int maxIndexToSelectAtStart = 1;
    public float wavesTillAddIndex = 1;
    public int creditsPerWave = 150; // Additive bonus (waveIndex*creditsPerWave)
    public float timeBetweenWavesBase = 90; // Base amount of day-time
    public float batchMultiplierStart = 5; // Starting batch multiplier for each Subwave
    public float subwaveDelayMultStart = 1f; // Time between subwaves
    public int spawnRadiusBuffer = 16;
    public int batchMultGrowthTime = 2;
    public int damageMultGrowthTime = 5;
    public int attackDelayMultGrowthTime = 60;
    public int healthMultGrowthTime = 20;
    public int speedMultGrowthTime = 20;
    public int sizeMultGrowthTime = 40;
    public int subwaveDelayMultGrowthTime = 40;

    [Header("Enemy Multipliers")]
    public float damageMultiplier = 1;
    public float healthMultiplier = 1;
    public float speedMultiplier = 1;
    public float sizeMultiplier = 1; // Smaller is harder

    [Header("Mutations Settings")]
    public float mutationChance = 0.1f;
    public float addMutationChancePerWave = 0.02f;
    public float mutationChanceMax = 0.3f;
    public float mutationIntensity = 0.1f; // Range of randomization for scale
    public float addMutationIntensityPerWave = 0.01f;
    public float subwaveSpawnFactor = 0.5f; // Mult for number to spawn each subwave
}
