using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public int poolSize = 20;
    private List<AudioSource> sourcesPool;
    private List<AudioSource> sourcesActivePool;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeSources();
    }

    /// <summary>
    /// Plays sound at given position
    /// </summary>
    public void PlaySoundAt(Sound sound, Vector2 pos)
    {
        StartCoroutine(PlaySoundAtRoutine(sound, pos));
    }

    /// <summary>
    /// Plays sound following a target
    /// </summary>
    public void PlaySoundFollow(Sound sound, Transform target)
    {
        StartCoroutine(PlaySoundFollowRoutine(sound, target));
    }

    private IEnumerator PlaySoundAtRoutine(Sound sound, Vector2 pos)
    {
        AudioSource source = GetAvailableAudioSource();

        if (source != null)
        {
            source.transform.position = pos;
            source = ConfigureSource(source, sound);
            float rand = Random.Range(-sound.pitchRandRange, sound.pitchRandRange);
            source.pitch += rand;
            source.Play();

            yield return new WaitWhile(() => source.isPlaying);
            ReturnAudioSourceToPool(source);
        }
        else
        {
            Debug.LogError("Could not find AudioSource!");
        }
    }

    private IEnumerator PlaySoundFollowRoutine(Sound sound, Transform target)
    {
        AudioSource source = GetAvailableAudioSource();

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

            ReturnAudioSourceToPool(source);
        }
        else
        {
            Debug.LogError("Could not find AudioSource!");
        }
    }

    AudioSource ConfigureSource(AudioSource source, Sound sound)
    {
        // Choose random clip from list
        int i = Random.Range(0, sound.clips.Length);
        source.clip = sound.clips[i];

        // If new sound list, configure
        if (source.name != sound.soundName)
        {
            source.gameObject.SetActive(true);
            
            source.loop = sound.loop;
            source.volume = sound.volume;
            source.pitch = sound.pitch;
            source.spatialBlend = sound.spatialBlend;
            source.dopplerLevel = sound.dopplerLevel;
            source.minDistance = sound.minDistance;
            source.maxDistance = sound.maxDistance;
        }

        return source;
    }

    #region Pooling

    private void InitializeSources()
    {
        sourcesPool = new List<AudioSource>();
        sourcesActivePool = new List<AudioSource>();

        for (int i = 0; i < poolSize; i++)
        {
            AudioSource source = CreateAudioSource(i);
            sourcesPool.Add(source);
        }
    }

    private AudioSource GetAvailableAudioSource()
    {
        AudioSource source;
        if (sourcesPool.Count > 0)
        {
            source = sourcesPool[0];
            sourcesPool.RemoveAt(0);
            sourcesActivePool.Add(source);
        }
        else
        {
            Debug.LogWarning("No available AudioSources in pool. Adding new source.");
            source = GetNewAudioSource();
        }

        return source;
    }
    
    private AudioSource GetNewAudioSource()
    {
        AudioSource source = CreateAudioSource(sourcesActivePool.Count);
        sourcesActivePool.Add(source);

        return source;
    }

    AudioSource CreateAudioSource(int i)
    {
        GameObject sourceObj = new GameObject("AudioSource_" + i);
        AudioSource source = sourceObj.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0.8f; // 1 is full 3D sound
        source.minDistance = 1;
        source.maxDistance = 50;
        source.dopplerLevel = 0.2f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        sourceObj.SetActive(false);
        sourceObj.transform.SetParent(transform);

        Debug.Log("Created AudioSource");
        return source;
    }

    private void ReturnAudioSourceToPool(AudioSource source)
    {
        sourcesActivePool.Remove(source);
        sourcesPool.Add(source);
    }

    #endregion
}
