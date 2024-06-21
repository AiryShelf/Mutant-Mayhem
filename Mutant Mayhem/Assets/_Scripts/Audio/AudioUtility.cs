using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioUtility
{
    public static Sound InitializeSoundEffect(Sound sound)
    {
        // Get the original clip's data
        int samples = sound.clip.samples;
        int channels = sound.clip.channels;
        int tailSamples = Mathf.CeilToInt(sound.clip.frequency * 0.1f);
        int totalSamples = samples + tailSamples;

        float[] data = new float[totalSamples * channels];
        float[] originalData = new float[samples * channels];
        sound.clip.GetData(originalData, 0);
        originalData.CopyTo(data, 0);

        // Zero the first sample in each channel
        for (int i = 0; i < channels; i++)
        {
            data[i] = 0f;
        }

        // Create new AudioClip with zeroed first sample and silent tail
        AudioClip newClip = AudioClip.Create(sound.soundName + "_initialized", 
                                             totalSamples, channels, sound.clip.frequency, false);
        newClip.SetData(data, 0);

        // Create copy of sound with new editted clip
        Sound newSound = new Sound
        {
            soundName = sound.soundName,
            clip = newClip,
            loop = sound.loop,
            volume = sound.volume,
            pitch = sound.pitch,
            pitchRandRange = sound.pitchRandRange,
            spatialBlend = sound.spatialBlend,
            dopplerLevel = sound.dopplerLevel,
            minDistance = sound.minDistance,
            maxDistance = sound.maxDistance,
        };
        
        return newSound;
    }
}
