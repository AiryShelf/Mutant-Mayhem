using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessagePortraitController : MonoBehaviour
{
    [Header("Audio Sync")]
    public AudioSource audioSource;
    public Animator animator;
    public string isTalkingParam = "isTalking";
    public float checkInterval = 0.1f;
    public float volumeThreshold = 0.01f;

    private float[] audioSamples = new float[256];
    private Coroutine volumeCheckRoutine;

    void OnEnable()
    {
        if (audioSource != null && animator != null)
        {
            volumeCheckRoutine = StartCoroutine(CheckVolumeRoutine());
        }
    }

    void OnDisable()
    {
        if (volumeCheckRoutine != null)
        {
            StopCoroutine(volumeCheckRoutine);
        }
        animator.SetBool(isTalkingParam, false);
    }

    public void SetAudioSource(AudioSource source)
    {
        audioSource = source;
        if (audioSource != null && animator != null)
        {
            if (volumeCheckRoutine != null)
            {
                StopCoroutine(volumeCheckRoutine);
            }
            volumeCheckRoutine = StartCoroutine(CheckVolumeRoutine());
        }
    }

    IEnumerator CheckVolumeRoutine()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(checkInterval);

            audioSource.GetOutputData(audioSamples, 0);
            float avgVolume = 0f;
            for (int i = 0; i < audioSamples.Length; i++)
            {
                avgVolume += Mathf.Abs(audioSamples[i]);
            }
            avgVolume /= audioSamples.Length;

            bool isTalking = avgVolume > volumeThreshold;
            animator.SetBool(isTalkingParam, isTalking);
        }
    }
}
