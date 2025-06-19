using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlanetDialogue", menuName = "Game/Dialogue/PlanetDialogueSO")]
public class PlanetDialogueSO : ScriptableObject
{
    public ConversationData startConversation;
    public List<WaveDialogue> waveDialogues = new List<WaveDialogue>();
    public ConversationData missionCompleteConversation;
}

[System.Serializable]
public class WaveDialogue
{
    public int waveIndex;
    public bool playOnWaveStart = true;
    public ConversationData conversation;
}

public enum TriggerTime { Start, End }

[System.Serializable]
public class ConversationData
{
    public List<MessageSO> messages;
}
