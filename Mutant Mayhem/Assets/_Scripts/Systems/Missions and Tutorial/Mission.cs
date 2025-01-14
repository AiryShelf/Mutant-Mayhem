using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Mission_New", menuName = "Game/Missions/Mission")]
public class Mission : ScriptableObject
{
    public string missionTitle;
    public string toPassText;
    public List<Objective> objectives;
    public bool isTutorial = false;
}
