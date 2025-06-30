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
    public int lastPlanetVisited;
    public int playthroughs;
    public int maxWaveSurvived;
    public DifficultyLevel difficultyLevel;

    [Header("Options Settings")]
    public int qualityLevel;
    public bool isStandardWASD;
    public bool isSpacebarEnabled;
    public bool isFastJoystickAimEnabled;
    public float joystickCursorSpeed;
    public bool virtualAimJoystickDisabled;

    public PlayerProfile(string profileName, DifficultyLevel difficulty)
    {
        this.profileName = profileName;
        researchPoints = 0;
        lastPlanetVisited = 1; // Default to Tsorbia
        playthroughs = 0;
        maxWaveSurvived = 0;
        qualityLevel = -1;
        difficultyLevel = difficulty;
        isStandardWASD = true;
        isSpacebarEnabled = true;
        isFastJoystickAimEnabled = false;
        joystickCursorSpeed = 800;
        virtualAimJoystickDisabled = true;
    }
}