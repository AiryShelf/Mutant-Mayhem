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

    // Constructor for easy creation
    public PlayerProfile(string profileName)
    {
        this.profileName = profileName;
        researchPoints = 0;
        playthroughs = 0;
        maxWaveReached = 0;
    }
}