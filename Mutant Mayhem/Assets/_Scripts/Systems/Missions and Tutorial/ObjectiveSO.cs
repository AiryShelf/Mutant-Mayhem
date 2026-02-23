using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Objective_New", menuName = "Game/Missions/Objective")]
public class ObjectiveSO : ScriptableObject
{
    public string objectiveTitle;
    public bool autoShowInfoPanel = false;
    public Sprite infoImageSprite;
    public Sprite inforImageSprite2;
    [TextArea(3, 10)]
    public string objectiveDescription;
    [TextArea(3, 10)]
    public string objectiveDescriptionGamepad;
    [TextArea(3, 10)]
    public string objectiveDescriptionMobile;
    public List<GameObject> taskPrefabs = new List<GameObject>();
    public ConversationData startConversation;
    public ConversationData endConversation;
    public float timeToShowCompletionUI = 3f;
}
