using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Objective_New", menuName = "Game/Missions/Objective")]
public class ObjectiveSO : ScriptableObject
{
    public string objectiveTitle;
    [TextArea(3, 10)]
    public string objectiveDescription;
    public List<GameObject> taskPrefabs = new List<GameObject>();
    public ConversationData startConversation;
    public ConversationData endConversation;
    public float timeToShowCompletionUI = 3f;
}
