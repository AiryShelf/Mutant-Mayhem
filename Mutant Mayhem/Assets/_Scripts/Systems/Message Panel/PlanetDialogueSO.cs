using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlanetDialogue", menuName = "Game/Dialogue/PlanetDialogueSO")]
public class PlanetDialogueSO : ScriptableObject
{
    public ConversationSO startConversation;
    public List<WaveDialogue> waveDialogues = new List<WaveDialogue>();
    public ConversationSO missionCompleteConversation;
}
