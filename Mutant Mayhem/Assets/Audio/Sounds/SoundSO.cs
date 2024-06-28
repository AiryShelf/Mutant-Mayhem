using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum SoundType
{
    Music,
    SFX,
    UI,
}

[CreateAssetMenu(fileName = "NewSound", menuName = "SoundSO")]
public class SoundSO :ScriptableObject
{
    public string soundName;
    public SoundType soundType = SoundType.SFX;

    [Header("AudioSource Settings")]
    public AudioMixerGroup mixerGroup;
    public AudioClip[] clips;
    public bool loop;
    [Range(0f, 1f)]
    public float volume = 1;
    [Range(-3f, 3f)]
    public float pitch = 1;
    [Range(0, 1)]
    public float pitchRandRange = 0.2f;
    [Range(0, 1)]
    public float spatialBlend = 1f;
    public float minDistance = 20;
    public float maxDistance = 100;

}
