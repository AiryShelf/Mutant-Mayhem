using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [System.Serializable]
    private class ShakeCam
    {
        public CinemachineVirtualCamera vCam;
        [HideInInspector] public CinemachineBasicMultiChannelPerlin perlin;
        [HideInInspector] public float baseAmplitude;
        [HideInInspector] public float baseFrequency;
    }

    [Header("Virtual Cameras to Shake")]
    [SerializeField] private List<ShakeCam> shakeCameras = new List<ShakeCam>();

    private Coroutine currentShakeRoutine;
    private float currentIntensity = 0f;
    private float currentEndTime = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Cache Perlin components and base values
        foreach (var cam in shakeCameras)
        {
            if (cam.vCam == null) continue;

            cam.perlin = cam.vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (cam.perlin == null)
            {
                Debug.LogWarning($"CameraShake: No Perlin noise on {cam.vCam.name}. " +
                                 "Add CinemachineBasicMultiChannelPerlin to this vCam.");
                continue;
            }

            cam.baseAmplitude = cam.perlin.m_AmplitudeGain;
            cam.baseFrequency = cam.perlin.m_FrequencyGain;
        }
    }

    /// <summary>
    /// Shake the camera.
    /// intensity: roughly how strong the shake is (0.8 - 6 is a good range)
    /// duration: how long the shake lasts, in seconds.
    /// </summary>
    public void Shake(float intensity, float duration)
    {
        if (duration <= 0f || intensity <= 0f)
            return;

        float newEndTime = Time.time + duration;

        if (currentShakeRoutine != null)
        {
            // If there is already a shake running that is at least as strong
            // and lasts at least as long, ignore this weaker/shorter request.
            if (intensity <= currentIntensity && newEndTime <= currentEndTime - 0.1f)
            {
                return;
            }

            // Otherwise, override the current shake with the stronger/longer one.
            StopCoroutine(currentShakeRoutine);
        }

        currentIntensity = intensity;
        currentEndTime = newEndTime;

        currentShakeRoutine = StartCoroutine(ShakeRoutine(intensity, duration));
    }

    private IEnumerator ShakeRoutine(float intensity, float duration)
    {
        //Debug.Log($"CameraShake: Starting shake with intensity {intensity} for {duration} seconds.");
        float elapsed = 0f;

        // Set starting shake
        foreach (var cam in shakeCameras)
        {
            if (cam.perlin == null) continue;

            // Add intensity to amplitude; optionally bump frequency a bit
            cam.perlin.m_AmplitudeGain = cam.baseAmplitude + intensity;
            cam.perlin.m_FrequencyGain = cam.baseFrequency + intensity * 2f; // tweak to taste
        }

        // Fade out over time
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float fade = 1f - t; // linear fade; could use an AnimationCurve

            foreach (var cam in shakeCameras)
            {
                if (cam.perlin == null) continue;

                cam.perlin.m_AmplitudeGain = cam.baseAmplitude + intensity * fade;
                cam.perlin.m_FrequencyGain = cam.baseFrequency + intensity * 2f * fade;
            }

            yield return null;
        }

        // Restore base values
        foreach (var cam in shakeCameras)
        {
            if (cam.perlin == null) continue;

            cam.perlin.m_AmplitudeGain = cam.baseAmplitude;
            cam.perlin.m_FrequencyGain = cam.baseFrequency;
        }

        currentShakeRoutine = null;
        currentIntensity = 0f;
        currentEndTime = 0f;
    }
}