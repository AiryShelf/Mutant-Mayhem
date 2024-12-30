using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Planet_New", menuName = "Game/Planet")]
public class PlanetSO : ScriptableObject
{
    public string typeOfBody = "Planet/Moon";
    public string bodyName;
    public string description;
    public List<PlanetPropertySO> properties;
    public Tile terrainTile;
    public Mission mission;
    public GameObject highRezPlanetPrefab;
    public List<PlanetSO> prerequisitePlanets;

    [Header("Research Points:")]
    public int basePoints = 0;
    public int pointsPerWave = 20;
    public float growthControlFactor = 0.05f;
    //public int difficultyAdjustHard = 10; // These two are tentatively depricated
    //public int difficultyAdjustEasy = -10; //               " "

    [Header("WaveController Settings:")]
    [Header("Wave Properties")]
    public float timeBetweenWavesBase = 90; // Base amount of day-time
    public float wavesTillAddBase = 1; 
    public int batchMultiplierStart = 5; // Starting batch multiplier for each Subwave
    public float subwaveDelayMultStart = 1f; // Time between subwaves
    public int spawnRadiusBuffer = 16;

    [Header("Enemy Multipliers")]
    public float damageMultiplier = 1;
    public float healthMultiplier = 1;
    public float speedMultiplier = 1;
    public float sizeMultiplier = 1; // Smaller is harder
}
