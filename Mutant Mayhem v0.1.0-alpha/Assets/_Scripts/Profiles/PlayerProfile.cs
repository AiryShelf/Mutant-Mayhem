using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerProfile
{
    public string profileName;
    public int researchPoints;
    public int playthroughs;
    public int highestWaveReached;

    // Constructor for easy creation
    public PlayerProfile(string profileName)
    {
        this.profileName = profileName;
        this.researchPoints = 0;
        this.playthroughs = 0;
        this.highestWaveReached = 0;
    }
}