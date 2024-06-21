using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string soundName;
    [Header("AudioSource Settings")]
    public AudioClip clip;
    public bool loop;
    [Range(0f, 1f)]
    public float volume = 1;
    [Range(-3f, 3f)]
    public float pitch = 1;
    [Range(0, 1)]
    public float pitchRandRange = 0;
    [Range(0, 1)]
    public float spatialBlend = 0.8f;
    [Range(0f, 5f)]
    public float dopplerLevel = 1;
    public float minDistance = 1;
    public float maxDistance = 50;

}
