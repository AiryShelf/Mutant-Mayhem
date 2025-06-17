using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Conversation", menuName = "Game/Dialogue/ConversationSO")]
public class ConversationSO : ScriptableObject
{
    public List<MessageSO> messages = new List<MessageSO>();
}
