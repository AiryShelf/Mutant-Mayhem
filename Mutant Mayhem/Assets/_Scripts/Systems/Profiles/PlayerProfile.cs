using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerProfile
{
    public string profileName;
    public int researchPoints;
    public int playthroughs;
    public int maxWaveReached;
    public bool isTutorialEnabled;
    public DifficultyLevel difficultyLevel;
    public bool isStandardWASD;
    public bool isSpacebarEnabled;

    public PlayerProfile(string profileName, DifficultyLevel difficulty)
    {
        this.profileName = profileName;
        researchPoints = 0;
        playthroughs = 0;
        maxWaveReached = 0;
        isTutorialEnabled = true;
        difficultyLevel = difficulty;
        isStandardWASD = true;
        isSpacebarEnabled = true;
    }
}