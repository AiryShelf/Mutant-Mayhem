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
    [SerializeField] float fadeLightToValue = 0.25f;
    [SerializeField] float fadeSpriteToValue = 0.125f;

    float startLightValue;
    float startSpriteValue;
    Coroutine fadeCoroutine;

    void Awake()
    {
        startLightValue = light2d.intensity;
        startSpriteValue = sr.color.a;
    }

    void OnTriggerEnter2D(Collider2D other) 
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (other.CompareTag("Player"))
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeLightAndSprite(fadeSpriteToValue, fadeLightToValue));
        }
    }

    void OnTriggerExit2D(Collider2D other) 
    {
        if (!gameObject.activeInHierarchy)
            return;
            
        if (other.CompareTag("Player"))
        {
            if (!gameObject.activeSelf)
                return;
                
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeLightAndSprite(startSpriteValue, startLightValue));
        }
    }
    
    IEnumerator FadeLightAndSprite(float targetAlpha, float targetIntensity)
    {
        // Get the starting alpha and light intensity values
        float startAlpha = sr.color.a;
        float startIntensity = light2d.intensity;
        
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
            light2d.intensity = newIntensity;

            // Wait for the next frame
            timeElapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Ensure the final values are set
        Color finalColor = sr.color;
        finalColor.a = targetAlpha;
        sr.color = finalColor;
        light2d.intensity = targetIntensity;
    }
}
