using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemFader : MonoBehaviour
{
    [Tooltip("List of particle systems to fade.")]
    public List<ParticleSystem> particleSystems;
    public float fadeDuration = 2f;
    [SerializeField] bool startFaded = false;

    private Coroutine fadeCoroutine;

    // Cache the initial MinMaxGradient values for fading back to original
    private List<ParticleSystem.MinMaxGradient> initialGradients;

    void Start()
    {
        InitializeColors();

        if (!startFaded) return;
        
        // Start faded out
        foreach (var ps in particleSystems)
        {
            if (ps == null) continue;
            
            ps.gameObject.SetActive(false);
        }
    }

    void InitializeColors()
    {
        initialGradients = new List<ParticleSystem.MinMaxGradient>();
        foreach (var ps in particleSystems)
        {
            if (ps != null)
            {
                initialGradients.Add(ps.main.startColor);
            }
            else
            {
                initialGradients.Add(new ParticleSystem.MinMaxGradient(Color.white));
            }
        }
    }

    public void FadeOutNewParticles()
    {
        // Start a fade to 0 alpha, interrupting any current fade
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeToAlpha(0));
    }

    public void FadeInNewParticles()
    {
        // Start a fade to the initial alpha, interrupting any current fade
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeToAlpha(1));
    }

    private IEnumerator FadeToAlpha(float targetAlpha)
    {
        float timer = 0f;

        // Cache the current state of the particle systems
        List<Color> currentColorsMin = new List<Color>();
        List<Color> currentColorsMax = new List<Color>();

        for (int i = 0; i < particleSystems.Count; i++)
        {
            var ps = particleSystems[i];
            if (ps == null)
            {
                currentColorsMin.Add(Color.white);
                currentColorsMax.Add(Color.white);
                continue;
            }

            var main = ps.main;
            var currentGradient = main.startColor;

            // Handle based on the gradient mode
            if (currentGradient.mode == ParticleSystemGradientMode.Color)
            {
                currentColorsMin.Add(currentGradient.color);
                currentColorsMax.Add(currentGradient.color);
            }
            else if (currentGradient.mode == ParticleSystemGradientMode.TwoColors)
            {
                currentColorsMin.Add(currentGradient.colorMin);
                currentColorsMax.Add(currentGradient.colorMax);
            }
            else
            {
                // For gradients, default to white for simplicity
                currentColorsMin.Add(Color.white);
                currentColorsMax.Add(Color.white);
            }
        }

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            for (int i = 0; i < particleSystems.Count; i++)
            {
                var ps = particleSystems[i];
                if (ps == null) continue;

                var main = ps.main;
                var initialGradient = initialGradients[i];
                ParticleSystem.MinMaxGradient fadedGradient = new ParticleSystem.MinMaxGradient();

                // Interpolate alpha based on the current state
                switch (initialGradient.mode)
                {
                    case ParticleSystemGradientMode.Color:
                        Color fadedColor = currentColorsMin[i];
                        fadedColor.a = Mathf.Lerp(currentColorsMin[i].a, targetAlpha * initialGradient.color.a, t);
                        fadedGradient = new ParticleSystem.MinMaxGradient(fadedColor);
                        break;

                    case ParticleSystemGradientMode.TwoColors:
                        Color fadedColorMin = currentColorsMin[i];
                        Color fadedColorMax = currentColorsMax[i];
                        fadedColorMin.a = Mathf.Lerp(currentColorsMin[i].a, targetAlpha * initialGradient.colorMin.a, t);
                        fadedColorMax.a = Mathf.Lerp(currentColorsMax[i].a, targetAlpha * initialGradient.colorMax.a, t);
                        fadedGradient = new ParticleSystem.MinMaxGradient(fadedColorMin, fadedColorMax);
                        break;

                    case ParticleSystemGradientMode.Gradient:
                    case ParticleSystemGradientMode.TwoGradients:
                        // Gradients are not updated dynamically in this example for simplicity
                        fadedGradient = initialGradient;
                        break;
                }

                main.startColor = fadedGradient;
            }

            yield return null;
        }

        // Ensure the final state matches the target
        for (int i = 0; i < particleSystems.Count; i++)
        {
            var ps = particleSystems[i];
            if (ps == null) continue;

            var main = ps.main;
            var initialGradient = initialGradients[i];

            switch (initialGradient.mode)
            {
                case ParticleSystemGradientMode.Color:
                    Color finalColor = initialGradient.color;
                    finalColor.a = targetAlpha * initialGradient.color.a;
                    main.startColor = new ParticleSystem.MinMaxGradient(finalColor);
                    break;

                case ParticleSystemGradientMode.TwoColors:
                    Color finalColorMin = initialGradient.colorMin;
                    Color finalColorMax = initialGradient.colorMax;
                    finalColorMin.a = targetAlpha * initialGradient.colorMin.a;
                    finalColorMax.a = targetAlpha * initialGradient.colorMax.a;
                    main.startColor = new ParticleSystem.MinMaxGradient(finalColorMin, finalColorMax);
                    break;

                case ParticleSystemGradientMode.Gradient:
                case ParticleSystemGradientMode.TwoGradients:
                    // No need to handle gradients for final state here
                    break;
            }
        }

        fadeCoroutine = null;
    }
}