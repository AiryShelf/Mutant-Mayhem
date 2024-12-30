using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemTrailWidthFader : MonoBehaviour
{
    [Tooltip("List of Particle Systems to adjust.")]
    public List<ParticleSystem> particleSystems;
    public float fadeDuration = 1f;

    private List<float> defaultWidths = new List<float>();
    private Coroutine currentCoroutine;

    void Start()
    {
        // Store the default "Width Over Trail" values for each particle system
        foreach (var ps in particleSystems)
        {
            if (ps != null && ps.trails.enabled)
            {
                defaultWidths.Add(ps.trails.widthOverTrail.constant);
            }
            else
            {
                defaultWidths.Add(0); // Default to 0 if trails are not enabled
            }
        }
    }

    public void FadeOut()
    {
        // Stop any ongoing fade coroutine
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(FadeCoroutine(0));
    }

    public void FadeIn()
    {
        // Stop any ongoing fade coroutine
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        currentCoroutine = StartCoroutine(FadeCoroutine(1));
    }

    private IEnumerator FadeCoroutine(float targetAlpha)
    {
        float timer = 0f;

        // Cache the current "Width Over Trail" values
        List<float> currentWidths = new List<float>();
        foreach (var ps in particleSystems)
        {
            if (ps != null && ps.trails.enabled)
            {
                currentWidths.Add(ps.trails.widthOverTrail.constant);
                if (targetAlpha == 1) ps.gameObject.SetActive(true); // Activate PS for FadeIn
            }
            else
            {
                currentWidths.Add(0); // Default to 0
            }
        }

        // Gradually lerp the width values
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            for (int i = 0; i < particleSystems.Count; i++)
            {
                var ps = particleSystems[i];
                if (ps == null || !ps.trails.enabled) continue;

                var trailModule = ps.trails;

                // Lerp the width value
                float currentWidth = currentWidths[i];
                float targetWidth = (targetAlpha == 1) ? defaultWidths[i] : 0;
                trailModule.widthOverTrail = new ParticleSystem.MinMaxCurve(Mathf.Lerp(currentWidth, targetWidth, t));
            }

            yield return null;
        }

        // Ensure the target state is fully applied
        for (int i = 0; i < particleSystems.Count; i++)
        {
            var ps = particleSystems[i];
            if (ps == null || !ps.trails.enabled) continue;

            var trailModule = ps.trails;

            float targetWidth = (targetAlpha == 1) ? defaultWidths[i] : 0;
            trailModule.widthOverTrail = new ParticleSystem.MinMaxCurve(targetWidth);

            if (targetAlpha == 0) ps.gameObject.SetActive(false); // Deactivate PS for FadeOut
        }

        currentCoroutine = null;
    }
}