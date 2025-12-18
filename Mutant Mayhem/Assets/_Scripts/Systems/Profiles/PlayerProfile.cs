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
    public List<PlanetMissionsEntry> completedMissions = new List<PlanetMissionsEntry>();
    public List<PlanetIndexEntry> planetIndexReachedList = new List<PlanetIndexEntry>();
    [System.NonSerialized]
    public Dictionary<string, int> planetsMaxIndexReached;
    public int lastPlanetVisited;
    public int playthroughs;
    public int totalNightsSurvived;
    public DifficultyLevel difficultyLevel;
    public bool completedTutorial = false;

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
        completedTutorial = false;
        isStandardWASD = true;
        isSpacebarEnabled = true;
        isFastJoystickAimEnabled = false;
        joystickCursorSpeed = 1500f;
        joystickAccelSpeed = 3000;
        virtualAimJoystickDisabled = true;
    }

    public bool IsProfileUpToDate()
    {
        // Check if tutorial completed, fix list
        if (!completedTutorial)
        {
            if (completedPlanets.Contains("Tutorial"))
                completedTutorial = true;

            completedPlanets.Remove("Tutorial");
        }

        // Check cursor speed and accel is within range
        if (joystickCursorSpeed < CursorManager.Instance.cursorSpeedMin ||
            joystickCursorSpeed > CursorManager.Instance.cursorSpeedMax)
        {
            joystickCursorSpeed = 1500f;
        }
        if (joystickAccelSpeed < CursorManager.Instance.cursorAccelMin ||
            joystickAccelSpeed > CursorManager.Instance.cursorAccelMax)
        {
            joystickAccelSpeed = 3000f;
        }
        
        // Ensure lists exist
        if (planetIndexReachedList == null)
            planetIndexReachedList = new List<PlanetIndexEntry>();
        if (completedMissions == null)
            completedMissions = new List<PlanetMissionsEntry>();

        bool upgraded = false;

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

    public bool AddCompletedMission(string planetName, string missionTitle, int researchPointsReward)
    {
        // Find or create new empty entry for planet
        PlanetMissionsEntry planetEntry = completedMissions.Find(entry => entry.planetName == planetName);
        if (planetEntry == null)
        {
            planetEntry = new PlanetMissionsEntry
            {
                planetName = planetName,
                completedMissions = new List<string>()
            };
            completedMissions.Add(planetEntry);
        }

        // Add mission if not already completed
        if (!planetEntry.completedMissions.Contains(missionTitle))
        {
            planetEntry.completedMissions.Add(missionTitle);
            researchPoints += researchPointsReward;
            ProfileManager.Instance.SaveCurrentProfile();
            return true;
        }

        return false;
    }
}

[System.Serializable]
public class PlanetMissionsEntry
{
    public string planetName;
    public List<string> completedMissions;
}