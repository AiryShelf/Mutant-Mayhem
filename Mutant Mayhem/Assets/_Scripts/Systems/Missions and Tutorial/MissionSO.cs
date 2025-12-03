using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Mission_New", menuName = "Game/Missions/Mission")]
public class MissionSO : ScriptableObject
{
    public string missionTitle;
    public string toPassText;
    public List<ObjectiveSO> objectives;
    public bool isTutorial = false;
    public int researchPointsReward = 0;
}
