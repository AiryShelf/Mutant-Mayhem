using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SongSO_New", menuName = "Audio/SongSO")]
public class SongSO : ScriptableObject
{
    public string title;
    public string artist;
    public string genre;
    public AudioClip audioClip;

    public int numberOfLoops;
    [Range(0, 1)]
    public float volume = 1;

    public float GetTrackLength()
    {
        return audioClip != null ? audioClip.length : 0f;
    }
}
