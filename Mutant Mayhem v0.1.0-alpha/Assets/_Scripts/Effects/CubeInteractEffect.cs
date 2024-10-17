using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CubeInteractEffect : MonoBehaviour
{
    [SerializeField] Collider2D trigger;
    [SerializeField] SpriteRenderer sr;
    [SerializeField] Light2D light2d;
    [SerializeField] float fadeTime = 1f;
    [SerializeField] float fadeToValue = 0.5f;

    Coroutine fadeCoroutine;

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Player"))
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeLightAndSprite(fadeToValue));
        }
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        if (other.CompareTag("Player"))
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeLightAndSprite(0f));
        }
    }
    
    IEnumerator FadeLightAndSprite(float target)
    {
        // Get the starting alpha and light intensity values
        float startAlpha = sr.color.a;
        float startIntensity = light2d.intensity;
        
        float timeElapsed = 0f;

        while (timeElapsed < fadeTime)
        {
            // Lerp the alpha and intensity
            float newAlpha = Mathf.Lerp(startAlpha, target, timeElapsed / fadeTime);
            float newIntensity = Mathf.Lerp(startIntensity, target, timeElapsed / fadeTime);

            // Set the sprite's alpha
            Color newColor = sr.color;
            newColor.a = newAlpha;
            sr.color = newColor;

            // Set the light's intensity
            light2d.intensity = newIntensity;

            // Wait for the next frame
            timeElapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Ensure the final values are set
        Color finalColor = sr.color;
        finalColor.a = target;
        sr.color = finalColor;
        light2d.intensity = target;
    }
}
