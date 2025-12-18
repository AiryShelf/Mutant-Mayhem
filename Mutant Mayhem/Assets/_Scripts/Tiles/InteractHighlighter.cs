using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class InteractHighlighter : MonoBehaviour
{
    [SerializeField] SpriteRenderer sr;
    [SerializeField] Light2D glowLight;
    [SerializeField] float fadeTime = 1f;

    float srStartAlpha;
    float glowLightStartIntensity;
    Coroutine fadeCoroutine;
    Coroutine disableCoroutine;

    void Awake()
    {
        srStartAlpha = sr.color.a;
        sr.enabled = false;
        glowLightStartIntensity = glowLight.intensity;
        glowLight.enabled = false;
        DisableGlow();
    }

    public void EnableGlow()
    {
        //Debug.Log("Enable Glow");
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        sr.enabled = true;
        glowLight.enabled = true;
        fadeCoroutine = StartCoroutine(FadeLightAndSprite(srStartAlpha, glowLightStartIntensity));

        if (disableCoroutine != null)
            StopCoroutine(disableCoroutine);
    }

    public void DisableGlow()
    {
        if (disableCoroutine != null)
            StopCoroutine(disableCoroutine);
        disableCoroutine = StartCoroutine(DisableRoutine());
    }

    IEnumerator DisableRoutine()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeLightAndSprite(0f, 0f));
        yield return fadeCoroutine;

        sr.enabled = false;
        glowLight.enabled = false;
    }

    IEnumerator FadeLightAndSprite(float targetAlpha, float targetIntensity)
    {
        // Get the starting values to smooth fading out while fading in etc.  Prevent jumps.
        float startAlpha = sr.color.a;
        float startIntensity = glowLight.intensity;
        
        float timeElapsed = 0f;

        while (timeElapsed < fadeTime)
        {
            // Lerp the alpha and intensity
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, timeElapsed / fadeTime);
            float newIntensity = Mathf.Lerp(startIntensity, targetIntensity, timeElapsed / fadeTime);

            // Set the sprite's alpha
            Color newColor = sr.color;
            newColor.a = newAlpha;
            sr.color = newColor;

            // Set the light's intensity
            glowLight.intensity = newIntensity;

            // Wait for the next frame
            timeElapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Ensure the final values are set
        Color finalColor = sr.color;
        finalColor.a = targetAlpha;
        sr.color = finalColor;
        glowLight.intensity = targetIntensity;

        fadeCoroutine = null;
    }
}
