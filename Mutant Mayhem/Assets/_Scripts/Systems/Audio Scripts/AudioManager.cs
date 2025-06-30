using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Mixer")]
    public AudioMixerGroup mixerGroupMusic;
    public AudioMixerGroup mixerGroupSFX;
    public AudioMixerGroup mixerGroupUI;
    public AudioMixerGroup mixerGroupVoice;

    [Header("Snapshots")]
    public AudioMixerSnapshot gameplaySnapshot;
    public AudioMixerSnapshot deathSnapshot;
    public float snapshotFadeTime = 4;

    [Header("Pooling Settings")]
    public int poolSizeMusic = 20;
    private List<AudioSource> sourcesPoolMusic;
    private List<AudioSource> sourcesActivePoolMusic;

    public int poolSizeSFX = 3;
    private List<AudioSource> sourcesPoolSFX;
    private List<AudioSource> sourcesActivePoolSFX;

    public int poolSizeUI = 5;
    private List<AudioSource> sourcesPoolUI;
    private List<AudioSource> sourcesActivePoolUI;

    public int poolSizeVoice = 5;
    private List<AudioSource> sourcesPoolVoice;
    private List<AudioSource> sourcesActivePoolVoice;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        sourcesPoolMusic = CreateAudioSourcePool(mixerGroupMusic, poolSizeMusic);
        sourcesActivePoolMusic = new List<AudioSource>();

        sourcesPoolSFX = CreateAudioSourcePool(mixerGroupSFX, poolSizeSFX);
        sourcesActivePoolSFX = new List<AudioSource>();

        sourcesPoolUI = CreateAudioSourcePool(mixerGroupUI, poolSizeUI);
        sourcesActivePoolUI = new List<AudioSource>();

        sourcesPoolVoice = CreateAudioSourcePool(mixerGroupVoice, poolSizeVoice);
        sourcesActivePoolVoice = new List<AudioSource>();
    }

    public void Initialize()
    {
        //gameplaySnapshot.TransitionTo(0);
    }

    /// <summary>
    /// Plays sound at given position
    /// </summary>
    public AudioSource PlaySoundAt(SoundSO sound, Vector2 pos)
    {
        AudioSource source = GetAvailableAudioSource(sound.soundType);
        StartCoroutine(PlaySoundAtRoutine(sound, pos, source));

        return source;
    }

    /// <summary>
    /// Plays sound following a target
    /// </summary>
    public void PlaySoundFollow(SoundSO sound, Transform target)
    {
        StartCoroutine(PlaySoundFollowRoutine(sound, target));
    }

    public void StopSound(AudioSource source, SoundType soundType)
    {
        if (source != null && source.isPlaying)
        {
            source.Stop();
            ReturnAudioSourceToPool(source, soundType);
        }
    }

    IEnumerator PlaySoundAtRoutine(SoundSO sound, Vector2 pos, AudioSource source)
    {
        if (source == null) { Debug.LogError("No AudioSource!"); yield break; }

        source.transform.position = pos;
        source = ConfigureSource(source, sound);
        source.pitch += Random.Range(-sound.pitchRandRange, sound.pitchRandRange);

        AudioClip startedClip = source.clip;
        source.Play();

        while (source.isPlaying)
            yield return null;

        ReturnAudioSourceToPool(source, sound.soundType);
    }

    private IEnumerator PlaySoundFollowRoutine(SoundSO sound, Transform target)
    {
        AudioSource source = GetAvailableAudioSource(sound.soundType);

        if (source != null)
        {
            source = ConfigureSource(source, sound);
            float rand = Random.Range(-sound.pitchRandRange, sound.pitchRandRange);
            source.pitch += rand;
            source.Play();

            while (source.isPlaying)
            {
                // This 'should' work when adding bullet pooling
                if (target != null && target.gameObject.activeSelf)
                    source.transform.position = target.position;

                yield return null;
            }

            ReturnAudioSourceToPool(source, sound.soundType);
        }
        else
        {
            Debug.LogError("Could not find AudioSource!");
        }
    }

    AudioSource ConfigureSource(AudioSource source, SoundSO sound)
    {
        // Choose random clip from list
        int i = Random.Range(0, sound.clips.Length);
        source.clip = sound.clips[i];

        source.gameObject.SetActive(true);
        //Debug.Log("Configured AudioSource: " + source.name);

        source.loop = sound.loop;
        source.volume = sound.volume;
        source.pitch = sound.pitch;
        source.spatialBlend = sound.spatialBlend;
        source.dopplerLevel = 0;
        source.minDistance = sound.minDistance;
        source.maxDistance = sound.maxDistance;

        return source;
    }

    public void FadeToDeathSnapshot()
    {
        //deathSnapshot.TransitionTo(snapshotFadeTime);
    }

    #region Pool Handling

    private List<AudioSource> CreateAudioSourcePool(AudioMixerGroup mixerGroup, int poolSize)
    {
        List<AudioSource> sourcesPool = new List<AudioSource>();

        for (int i = 0; i < poolSize; i++)
        {
            AudioSource source = CreateAudioSource(i, mixerGroup);
            sourcesPool.Add(source);
        }

        return sourcesPool;
    }

    private AudioSource GetAvailableAudioSource(SoundType type)
    {
        // Setup for sound type
        AudioSource source;
        AudioMixerGroup mixerGroup;
        List<AudioSource> sourcesPool;
        List<AudioSource> sourcesActivePool;
        switch (type)
        {
            case SoundType.Music:
                sourcesPool = sourcesPoolMusic;
                sourcesActivePool = sourcesActivePoolMusic;
                mixerGroup = mixerGroupMusic;
                break;
            case SoundType.SFX:
                sourcesPool = sourcesPoolSFX;
                sourcesActivePool = sourcesActivePoolSFX;
                mixerGroup = mixerGroupSFX;
                break;
            case SoundType.UI:
                sourcesPool = sourcesPoolUI;
                sourcesActivePool = sourcesActivePoolUI;
                mixerGroup = mixerGroupUI;
                break;
            case SoundType.Voice:
                sourcesPool = sourcesPoolVoice;
                sourcesActivePool = sourcesActivePoolVoice;
                mixerGroup = mixerGroupVoice;
                break;
            default:
                sourcesPool = sourcesPoolSFX;
                sourcesActivePool = sourcesActivePoolSFX;
                mixerGroup = mixerGroupSFX;
                break;
        }

        // Get AudioSource or add new
        if (sourcesPool.Count > 0)
        {
            source = sourcesPool[0];
            sourcesPool.RemoveAt(0);
            sourcesActivePool.Add(source);
        }
        else
        {
            Debug.LogWarning("No available " + type + " AudioSources in pool. Adding new source.");
            source = GetNewAudioSource(sourcesActivePool, mixerGroup);
        }

        return source;
    }

    private AudioSource GetNewAudioSource(List<AudioSource> sourcesActivePool, AudioMixerGroup mixerGroup)
    {
        AudioSource source = CreateAudioSource(sourcesActivePoolSFX.Count, mixerGroup);
        sourcesActivePool.Add(source);

        return source;
    }

    private void ReturnAudioSourceToPool(AudioSource source, SoundType type)
    {
        // Setup for sound type
        List<AudioSource> sourcesPool;
        List<AudioSource> sourcesActivePool;
        switch (type)
        {
            case SoundType.Music:
                sourcesPool = sourcesPoolMusic;
                sourcesActivePool = sourcesActivePoolMusic;
                break;
            case SoundType.SFX:
                sourcesPool = sourcesPoolSFX;
                sourcesActivePool = sourcesActivePoolSFX;
                break;
            case SoundType.UI:
                sourcesPool = sourcesPoolUI;
                sourcesActivePool = sourcesActivePoolUI;
                break;
            case SoundType.Voice:
                sourcesPool = sourcesPoolVoice;
                sourcesActivePool = sourcesActivePoolVoice;
                break;
            default:
                sourcesPool = sourcesPoolSFX;
                sourcesActivePool = sourcesActivePoolSFX;
                break;
        }

        // Remove from active, return to pool
        source.gameObject.SetActive(false);
        sourcesActivePool.Remove(source);
        sourcesPool.Insert(0, source);
    }

    AudioSource CreateAudioSource(int i, AudioMixerGroup mixerGroup)
    {
        GameObject sourceObj = new GameObject(mixerGroup + "_AudioSource_" + i);
        AudioSource source = sourceObj.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = mixerGroup;
        source.playOnAwake = false;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        sourceObj.SetActive(false);
        sourceObj.transform.SetParent(transform);

        //Debug.Log("Created " + mixerGroup.name + " AudioSource");
        return source;
    }

    #endregion
}
