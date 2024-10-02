using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public static class GameTools
{
    // Reference to a GameObject that runs coroutines
    private static CoroutineHandler coroutineHandler;

    // Initialize the coroutine handler
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        // Create a new GameObject to run coroutines
        GameObject coroutineObject = new GameObject("CoroutineHandler");
        coroutineHandler = coroutineObject.AddComponent<CoroutineHandler>();

        // Prevent it from being destroyed on scene change
        GameObject.DontDestroyOnLoad(coroutineObject);
    }

    // Public method to start a coroutine from anywhere
    public static Coroutine StartCoroutine(IEnumerator coroutine)
    {
        return coroutineHandler.StartCoroutine(coroutine);
    }

    // Public method to stop a coroutine
    public static void StopCoroutine(Coroutine coroutine)
    {
        if (coroutine != null)
        {
            coroutineHandler.StopCoroutine(coroutine);
        }
    }

    // Example of a lerp coroutine
    public static IEnumerator LerpPosition(Transform objectTransform, Vector3 startPos, Vector3 endPos, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = elapsed / duration;
            objectTransform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        objectTransform.position = endPos; // Ensure final position is set
    }

    public static IEnumerator LerpFloat(float startValue, float endValue, float duration, System.Action<float> onUpdate)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float currentValue = Mathf.Lerp(startValue, endValue, t);

            // Update the value via the callback
            onUpdate?.Invoke(currentValue);

            yield return null;
        }

        // Ensure the final value is set
        onUpdate?.Invoke(endValue);
    }

    public static void TextureOffsetCentered(Material material, Transform transform, Transform target, 
                                             Vector2 direction, float speed, ref Vector2 currentOffset)
    {
        Vector2 targetPos = target.transform.position;
        transform.position = targetPos;
        currentOffset += direction * speed * Time.deltaTime;
        Vector2 newOffset = targetPos/64 + currentOffset;

        material.mainTextureOffset = newOffset;
    }

    // Returns a percentage of gain or loss, depending on the factor
    // Ex. 1.2 = 20% and 0.8 = 20%
    public static string FactorToPercent(float factor)
    {
        float percentage = (factor - 1) * 100;

        return $"{Mathf.Abs(percentage):F0}%";
    }

    // Returns a string formatted 1 hour, 4 minutes, 4 seconds
    public static string FormatTimeFromSeconds(int totalSeconds)
    {
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;

        string formattedTime = "";
        
        if (hours > 1)
            formattedTime += $"{hours} hours, ";
        else if (hours > 0)
            formattedTime += $"{hours} hour, ";
        
        if (minutes > 1)
            formattedTime += $"{minutes} minutes, ";
        else if (minutes > 0)
            formattedTime += $"{minutes} minute, ";
        else if (hours > 0)
            formattedTime += $"{minutes} minutes, ";
        
        if (seconds > 1)
            formattedTime += $"{seconds} seconds";
        else if (seconds > 0)
            formattedTime += $"{seconds} second";
        else 
            formattedTime += $"{seconds} seconds";

        return formattedTime.Trim();
    }

    public static IEnumerator ShakeTransform(Transform transform, Vector2 startLocalPos,
                                             float shakeTime, float shakeAmount, float shakeSpeed)
    {
        float timeElapsed = 0;

        while (timeElapsed < shakeTime)
        {
            transform.localPosition = startLocalPos + Random.insideUnitCircle * shakeAmount;
            yield return new WaitForSeconds(shakeSpeed);
            timeElapsed += shakeSpeed;
        }

        // Reset position
        transform.localPosition = startLocalPos;
    }

    public static IEnumerator FlashSpriteOrImage(SpriteRenderer sr, Image image, float flashTime, float flashSpeed, Color flashColor, Color startColor)
    {
        float timeElapsed = 0;

        while (timeElapsed < flashTime)
        {
            // Calculate the percentage of completion for the flash cycle based on flashSpeed
            float t = Mathf.PingPong(timeElapsed / flashSpeed, 1);

            if (sr != null)
            {
                // Interpolate sprite color based on PingPong calculation
                sr.color = Color.Lerp(startColor, flashColor, t);
            }
            else if (image != null)
            {
                // Interpolate image color based on PingPong calculation
                image.color = Color.Lerp(startColor, flashColor, t);
            }

            // Wait for a frame, then increment timeElapsed
            yield return null;
            timeElapsed += Time.deltaTime;
        }

        // Reset color after flashing is done
        if (sr != null)
        {
            sr.color = startColor;
        }
        else if (image != null)
        {
            image.color = startColor;
        }
    }
}

// Helper MonoBehaviour class to run coroutines
public class CoroutineHandler : MonoBehaviour { }