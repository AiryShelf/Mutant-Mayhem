using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerProfile
{
    [Header("Profile Stats")]
    public string profileName;
    public int researchPoints;
    public List<string> completedPlanets = new List<string>();
    public List<PlanetIndexEntry> planetIndexReachedList = new List<PlanetIndexEntry>();
    [System.NonSerialized]
    public Dictionary<string, int> planetsMaxIndexReached;
    public int lastPlanetVisited;
    public int playthroughs;
    public int totalNightsSurvived;
    public DifficultyLevel difficultyLevel;
    public int maxAugLevels;

    [Header("Options Settings")]
    public int qualityLevel;
    public bool isStandardWASD;
    public bool isSpacebarEnabled;
    public bool isFastJoystickAimEnabled;
    public float joystickCursorSpeed;
    public float joystickAccelSpeed;
    public bool virtualAimJoystickDisabled;

    public PlayerProfile(string profileName, DifficultyLevel difficulty)
    {
        this.profileName = profileName;
        researchPoints = 0;
        lastPlanetVisited = 1; // Default to Tsorbia
        playthroughs = 0;
        qualityLevel = -1;
        difficultyLevel = difficulty;
        maxAugLevels = 12;
        isStandardWASD = true;
        isSpacebarEnabled = true;
        isFastJoystickAimEnabled = false;
        joystickCursorSpeed = 1500f;
        joystickAccelSpeed = 3000;
        virtualAimJoystickDisabled = true;
    }

    public bool IsProfileUpToDate()
    {
        // Ensure list exists
        if (planetIndexReachedList == null)
            planetIndexReachedList = new List<PlanetIndexEntry>();

        bool upgraded = false;

        if (maxAugLevels == 0)
        {
            int augLevels = 12;
            if (completedPlanets != null)
                augLevels += completedPlanets.Count;

            maxAugLevels = augLevels;
            upgraded = true;
        }

        return !upgraded;
    }

    [System.Serializable]
    public class PlanetIndexEntry
    {
        public string planetName;
        public int maxIndexReached;
    }

    public void EnsurePlanetIndexLookup()
    {
        if (planetsMaxIndexReached != null)
            return;

        planetsMaxIndexReached = new Dictionary<string, int>();

        if (planetIndexReachedList == null)
            planetIndexReachedList = new List<PlanetIndexEntry>();

        foreach (var entry in planetIndexReachedList)
        {
            if (!string.IsNullOrEmpty(entry.planetName))
                planetsMaxIndexReached[entry.planetName] = entry.maxIndexReached;
        }
    }

    public int GetPlanetMaxIndex(string planetName)
    {
        EnsurePlanetIndexLookup();
        return planetsMaxIndexReached.TryGetValue(planetName, out int value) ? value : 0;
    }

    public void SetPlanetMaxIndex(string planetName, int index)
    {
        EnsurePlanetIndexLookup();

        if (planetsMaxIndexReached.TryGetValue(planetName, out int existing) && existing >= index)
            return;

        planetsMaxIndexReached[planetName] = index;

        // Keep list in sync for serialization
        var entry = planetIndexReachedList.Find(e => e.planetName == planetName);
        if (entry != null)
        {
            entry.maxIndexReached = index;
        }
        else
        {
            planetIndexReachedList.Add(new PlanetIndexEntry
            {
                planetName = planetName,
                maxIndexReached = index
            });
        }
    }
}