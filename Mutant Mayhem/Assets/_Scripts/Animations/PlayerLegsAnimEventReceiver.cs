using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLegsAnimEventReceiver : MonoBehaviour
{
    [SerializeField] Player player;

    void OnFootStep()
    {
        // Need check for ground type
        AudioManager.instance.PlaySoundAt(player.walkGrassSound, transform.position);
    }
}
