using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
public enum SoundOrigin
{
    Player,
    Enemy,
    Tile,
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] List<SoundSO> playerSoundsOrig;
    [SerializeField] List<SoundSO> enemySoundsOrig;
    [SerializeField] List<SoundSO> tileSoundsOrig;

    List<SoundSO> playerSounds;
    List<SoundSO> enemySounds;
    List<SoundSO> tileSounds;

    void Awake()
    {
        // Singleton
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

        // Initialize Sounds
        // PlayerSoundsDict
        foreach (SoundSO sound in playerSoundsOrig)
        {
            SoundSO newSound = AudioUtility.InitializeSoundEffect(sound);
            playerSounds.Add(newSound);
        }
        // EnemySoundsDict
        foreach (SoundSO sound in enemySoundsOrig)
        {
            SoundSO newSound = AudioUtility.InitializeSoundEffect(sound);
            enemySounds.Add(newSound);
        }
        // TileSoundsDict
        foreach (SoundSO sound in tileSoundsOrig)
        {
            SoundSO newSound = AudioUtility.InitializeSoundEffect(sound);
            tileSounds.Add(newSound);
        }
    }

    public void PlaySoundAt(SoundOrigin origin, SoundSO sound, Vector2 pos)
    {
        List<SoundSO> list = GetSoundList(origin);
        if (!list.Contains(sound))
        {
           Debug.LogError("SoundName not found in dict when playing sound"); 
           return;
        }

        SoundSO newSound = list[sound];
        AudioManager.instance.PlaySoundAt(newSound, pos);
    }

    public void PlaySoundFollow(SoundOrigin origin, SoundSO sound, Transform target)
    {
        List<SoundSO> list = GetSoundList(origin);
        if (!list.Contains(sound))
        {
           Debug.LogError("SoundName not found in dict when playing sound"); 
           return;
        }

        SoundSO newSound = list[sound];
        AudioManager.instance.PlaySoundFollow(newSound, target);
    }

    List<SoundSO> GetSoundList(SoundOrigin origin)
    {
        List<SoundSO> list;

        switch (origin)
        {
            case SoundOrigin.Player:
                list = playerSounds;
                break;
            case SoundOrigin.Enemy:
                list = playerSounds;
                break;
            case SoundOrigin.Tile:
                list = playerSounds;
                break;
            default:
                list = playerSounds;
                Debug.LogError("Sound dict not found when attempting to play sound");
                break;
        }

        return list;
    }
}
*/
