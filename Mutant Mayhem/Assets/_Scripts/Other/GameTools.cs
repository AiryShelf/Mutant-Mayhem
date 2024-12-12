using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// Helper MonoBehaviour class to run coroutines
public class CoroutineHandler : MonoBehaviour { }

public static class GameTools
{
    private static CoroutineHandler coroutineHandler;

    // Initialize the coroutine handler
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        // Create a new GameObject to run coroutines
        GameObject coroutineObject = new GameObject("CoroutineHandler");
        coroutineHandler = coroutineObject.AddComponent<CoroutineHandler>();

        GameObject.DontDestroyOnLoad(coroutineObject);
    }

    // Start a coroutine from anywhere
    public static Coroutine StartCoroutine(IEnumerator coroutine)
    {
        return coroutineHandler.StartCoroutine(coroutine);
    }

    public static void StopCoroutine(Coroutine coroutine)
    {
        if (coroutine != null)
        {
            coroutineHandler.StopCoroutine(coroutine);
        }
    }

    public static Vector2 RotateVector2(Vector2 v, float angle)
    {
        float radians = angle * Mathf.Deg2Rad; // Convert angle to radians
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        // Apply 2D rotation formula
        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
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

    /// <summary>
    /// Returns a percentage of gain or loss from a base factor of 1
    /// </summary>
    /// <param name="factor"></param>
    /// <returns>
    /// 1.2 returns "20%" and 0.8 returns "20%"
    /// </returns>

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
    
    public static bool RollForCrit(float critChance)
    {
        return Random.value <= critChance;
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

    #region Flash / Pulse

    public static IEnumerator FlashSprite(SpriteRenderer sr, float flashTime, float flashSpeed, Color flashColor, Color startColor)
    {
        if (sr == null)
        {
            Debug.LogError("GameTools: Sprite is null for FlashSprite()");
            yield break;
        }
        float timeElapsed = 0;

        while (timeElapsed < flashTime)
        {
            float t = Mathf.PingPong(timeElapsed / flashSpeed, 1);
            sr.color = Color.Lerp(startColor, flashColor, t);
            yield return null;
            timeElapsed += Time.deltaTime;
        }
            
        
        sr.color = startColor;
    }

    public static IEnumerator FlashImage(Image image, float flashTime, float flashSpeed, Color flashColor, Color startColor)
    {
        if (image == null)
        {
            Debug.LogError("GameTools: Image is null for FlashImage()");
            yield break;
        }

        float timeElapsed = 0;
        while (timeElapsed < flashTime)
        {
            float t = Mathf.PingPong(timeElapsed / flashSpeed, 1);
            image.color = Color.Lerp(startColor, flashColor, t);
            yield return null;
            timeElapsed += Time.deltaTime;
        }

        image.color = startColor;
    }

    public static IEnumerator FlashWorldText(TextMeshPro text, float flashTime, float flashSpeed, Color flashColor, Color startColor)
    {
        if (text = null)
        {
            Debug.LogError("GameTools: Text is null for FlashText()");
            yield break;
        }

        float timeElapsed = 0;
        while (timeElapsed < flashTime)
        {
            float t = Mathf.PingPong(timeElapsed / flashSpeed, 1);
            text.color = Color.Lerp(startColor, flashColor, t);
            yield return null;
            timeElapsed += Time.deltaTime;
        }

        text.color = startColor;
    }

    public static IEnumerator PulseEffect(Transform transform, float pulseDuration, Vector3 pulseScaleMin, Vector3 pulseScaleMax)
    {
        float elapsedTime = 0f;
        Vector3 scaleStart = transform.localScale;

        while (elapsedTime < pulseDuration)
        {
            if (transform == null)
                yield break;

            float t = elapsedTime / pulseDuration;
            float easedT;

            // Different easing functions for expansion and contraction
            if (t < 0.25f)
            {
                float expansionT = t * 4;
                //easedT = 1 - Mathf.Pow(1 - expansionT, 3); // Fast In
                easedT = expansionT;
            }
            else
            {
                float contractionT = (t - 0.25f) * (1 / 0.75f);
                easedT = 1 - contractionT; // Slow out
            }

            Vector3 scale = Vector3.Lerp(pulseScaleMin, pulseScaleMax, easedT);
            transform.localScale = scale;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (transform)
            transform.localScale = scaleStart;
    }

    #endregion
}



