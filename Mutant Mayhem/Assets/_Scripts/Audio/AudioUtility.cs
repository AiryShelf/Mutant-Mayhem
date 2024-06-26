using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioUtility
{
    /// <summary>
    /// Used once to create new sound which prevents pops/clicks
    /// </summary>
    public static Sound InitializeSoundEffect(Sound sound)
    {
        // Create copy of sound to add editted clips
        Sound newSound = new Sound
        {
            soundName = sound.soundName,
            clips = new AudioClip[sound.clips.Length],
            loop = sound.loop,
            volume = sound.volume,
            pitch = sound.pitch,
            pitchRandRange = sound.pitchRandRange,
            spatialBlend = sound.spatialBlend,
            dopplerLevel = sound.dopplerLevel,
            minDistance = sound.minDistance,
            maxDistance = sound.maxDistance,
        };

        for (int i = 0; i < sound.clips.Length; i++)
        {
            // Get the original clip's data
            int samples = sound.clips[i].samples;
            int channels = sound.clips[i].channels;
            int tailSamples = Mathf.CeilToInt(sound.clips[i].frequency * 0.1f);
            int totalSamples = samples + tailSamples;

            float[] data = new float[totalSamples * channels];
            float[] originalData = new float[samples * channels];
            sound.clips[i].GetData(originalData, 0);
            originalData.CopyTo(data, 0);

            // Zero the first sample in each channel
            for (int c = 0; c < channels; c++)
            {
                data[c] = 0f;
            }

            // Create new AudioClip with zeroed first sample and silent tail
            AudioClip newClip = AudioClip.Create(sound.soundName + "_initialized", 
                                                totalSamples, channels, sound.clips[i].frequency, false);
            newClip.SetData(data, 0);

            // Add the adjusted clip to newSound
            newSound.clips[i] = newClip;            
        }

        return newSound;
    }
}
