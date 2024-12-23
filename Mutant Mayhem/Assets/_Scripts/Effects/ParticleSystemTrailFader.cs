using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemTrailFader : MonoBehaviour
{
    [Tooltip("List of Particle Systems to adjust.")]
    public List<ParticleSystem> particleSystems;
    public float fadeDuration = 1f;

    private List<Color> defaultColors = new List<Color>();
    private Coroutine currentCoroutine;

    void Start()
    {
        // Store the default "Color Over Trail" values for each particle system
        foreach (var ps in particleSystems)
        {
            if (ps != null && ps.trails.enabled)
            {
                defaultColors.Add(ps.trails.colorOverTrail.color); // Store default color
            }
            else
            {
                defaultColors.Add(new Color(1, 1, 1, 0)); // Default to transparent white if trails are not enabled
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

        // Cache the current "Color Over Trail" alpha values
        List<Color> currentColors = new List<Color>();
        foreach (var ps in particleSystems)
        {
            if (ps != null && ps.trails.enabled)
            {
                var color = ps.trails.colorOverTrail.color;
                currentColors.Add(color);
                if (targetAlpha == 1) ps.gameObject.SetActive(true); // Activate PS for FadeIn
            }
            else
            {
                currentColors.Add(new Color(1, 1, 1, 0)); // Default to transparent white
            }
        }

        // Gradually lerp the alpha values
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            for (int i = 0; i < particleSystems.Count; i++)
            {
                var ps = particleSystems[i];
                if (ps == null || !ps.trails.enabled) continue;

                var trailModule = ps.trails;

                // Lerp the alpha value
                var currentColor = currentColors[i];
                var targetColor = defaultColors[i];
                targetColor.a = targetAlpha;

                var newColor = Color.Lerp(currentColor, targetColor, t);

                trailModule.colorOverTrail = new ParticleSystem.MinMaxGradient(newColor); // Assign updated color
            }

            yield return null;
        }

        // Ensure the target state is fully applied
        for (int i = 0; i < particleSystems.Count; i++)
        {
            var ps = particleSystems[i];
            if (ps == null || !ps.trails.enabled) continue;

            var trailModule = ps.trails;

            var finalColor = defaultColors[i];
            finalColor.a = targetAlpha;

            trailModule.colorOverTrail = new ParticleSystem.MinMaxGradient(finalColor); // Apply final color

            if (targetAlpha == 0) ps.gameObject.SetActive(false); // Deactivate PS for FadeOut
        }

        currentCoroutine = null;
    }
}