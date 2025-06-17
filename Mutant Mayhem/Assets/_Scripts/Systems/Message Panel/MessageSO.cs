using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Message", menuName = "Game/Dialogue/MessageSO")]
public class MessageSO : ScriptableObject
{
    public string speakerName;
    public Color speakerNameColor = Color.green;
    public RuntimeAnimatorController portraitAnimatorController;
    public RuntimeAnimatorController backgroundAnimatorController;

    [TextArea] public string messageText;
    public SoundSO voiceClip;

    public float messageStartDelay = 0.5f;
    public float messageEndDelay = 3f;
}
