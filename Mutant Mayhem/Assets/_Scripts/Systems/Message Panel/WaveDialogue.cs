using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveDialogue
{
    public int waveIndex;
    public bool playOnWaveStart = true; // False plays on wave end
    public ConversationSO conversation;
}
