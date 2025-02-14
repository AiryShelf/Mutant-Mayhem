using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerProfile
{
    public string profileName;
    public int researchPoints;
    public int playthroughs;
    public int maxWaveSurvived;
    public bool isTutorialEnabled;
    public DifficultyLevel difficultyLevel;
    public bool isStandardWASD;
    public bool isSpacebarEnabled;
    public bool isFastJoystickAimEnabled;
    public float joystickCursorSpeed;
    public List<string> completedPlanets = new List<string>();

    public PlayerProfile(string profileName, DifficultyLevel difficulty)
    {
        this.profileName = profileName;
        researchPoints = 0;
        playthroughs = 0;
        maxWaveSurvived = 0;
        isTutorialEnabled = true;
        difficultyLevel = difficulty;
        isStandardWASD = true;
        isSpacebarEnabled = true;
        isFastJoystickAimEnabled = false;
        joystickCursorSpeed = 600;
    }
}