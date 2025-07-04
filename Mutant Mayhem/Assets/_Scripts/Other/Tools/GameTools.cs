using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    #region Lerp Functions

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

    #endregion

    #region String Formatting

    /// <summary>
    /// Converts a float into a formatted string with "k" for thousands or "M" for millions.
    /// </summary>
    /// <param name="value">The float value to convert.</param>
    /// <returns>A string representing the formatted value.</returns>
    public static string ConvertToStatValue(float value)
    {
        float absValue = Mathf.Abs(value);

        // [CHANGED] We create a helper function to format the scaled number 
        // to a max of three total digits (plus a decimal if needed).
        string FormatScaled(float scaled)
        {
            int decimals = 0;

            if (scaled < 10f) decimals = 2;
            else if (scaled < 100f) decimals = 1;
            else decimals = 0;

            // Use rounding instead of flooring
            float rounded = (float)System.Math.Round(scaled, decimals);

            // Convert to string with the chosen decimal setting
            // "F" format enforces decimal places
            return rounded.ToString($"F{decimals}");
        }

        // Check whether we should show 'M', 'k', or raw integer
        if (absValue > 999_999f)
        {
            // [CHANGED] Scale to millions and format
            float inMillions = value / 1_000_000f;
            return $"{FormatScaled(inMillions)} M";
        }
        else if (absValue > 999f)
        {
            // [CHANGED] Scale to thousands and format
            float inThousands = value / 1_000f;
            return $"{FormatScaled(inThousands)} k";
        }
        else
        {
            string rawFormatted = FormatScaled(value);
            if (rawFormatted.EndsWith(".00") || rawFormatted.EndsWith(".0"))
            {
                return value.ToString("F1");
            }
            // [CHANGED] Even for values under 1000, 
            // we call the same helper so we get consistent rounding
            return FormatScaled(value);
        }
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

    // Returns a string formatted 4 hour, 2 minutes, 0 seconds
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

    public static Vector2 GetRandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        float radian = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)).normalized;
    }

    #endregion

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

    public static IEnumerator FlashImage(Image image, float flashTime, float flashDelay, Color flashColor, Color startColor)
    {
        if (image == null)
        {
            Debug.LogError("GameTools: Image is null for FlashImage()");
            yield break;
        }

        float timeElapsed = 0;
        while (timeElapsed < flashTime)
        {
            float t = Mathf.PingPong(timeElapsed / flashDelay, 1);
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

    #region Others

    public static void TextureOffsetCentered(Material material, Transform transform, Transform target,
                                             Vector2 direction, float speed, ref Vector2 currentOffset)
    {
        Vector2 targetPos = target.transform.position;
        transform.position = targetPos;
        currentOffset += direction * speed * Time.deltaTime;
        Vector2 newOffset = targetPos / 64 + currentOffset;

        material.mainTextureOffset = newOffset;
    }
    
    public static List<Vector2> ComputeConvexHull(List<Vector2> points)
    {
        if (points.Count < 3)
            return points;

        List<Vector2> hull = new List<Vector2>();

        // Find the leftmost point
        Vector2 pointOnHull = points.OrderBy(p => p.x).ThenBy(p => p.y).First();

        Vector2 currentPoint = pointOnHull;
        Vector2 endpoint;

        do
        {
            hull.Add(currentPoint);
            endpoint = points[0];

            for (int i = 1; i < points.Count; i++)
            {
                // If endpoint is currentPoint, or if the currentPoint->endpoint->points[i] makes a right turn
                float cross = Cross(currentPoint, endpoint, points[i]);
                if (endpoint == currentPoint || cross < 0f || (cross == 0f && Vector2.Distance(currentPoint, points[i]) > Vector2.Distance(currentPoint, endpoint)))
                {
                    endpoint = points[i];
                }
            }

            currentPoint = endpoint;

        } while (currentPoint != pointOnHull);

        return hull;
    }

    private static float Cross(Vector2 o, Vector2 a, Vector2 b)
    {
        return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
    }

    public static List<Vector2> OrderPolygonPoints(List<Vector2> points)
    {
        if (points == null || points.Count < 3)
            return points;

        // Compute centroid
        Vector2 centroid = Vector2.zero;
        foreach (Vector2 p in points)
            centroid += p;
        centroid /= points.Count;

        // Sort points by angle from centroid (produces a perimeter loop)
        List<Vector2> ordered = points
            .OrderBy(p => Mathf.Atan2(p.y - centroid.y, p.x - centroid.x))
            .ToList();

        // Optional: check winding direction and reverse if needed
        if (!IsClockwise(ordered))
            ordered.Reverse();

        return ordered;
    }

    public static List<Vector2> RotatePointsToAnchor(List<Vector2> points, System.Func<Vector2, float> scoreFunc)
    {
        if (points == null || points.Count == 0)
            return points;

        // Find the best anchor point based on scoreFunc (minimizing score)
        int bestIndex = 0;
        float bestScore = scoreFunc(points[0]);

        for (int i = 1; i < points.Count; i++)
        {
            float score = scoreFunc(points[i]);
            if (score < bestScore)
            {
                bestScore = score;
                bestIndex = i;
            }
        }

        // Rotate list so that bestIndex point is first
        List<Vector2> rotated = new List<Vector2>();

        for (int i = 0; i < points.Count; i++)
        {
            int index = (bestIndex + i) % points.Count;
            rotated.Add(points[index]);
        }

        return rotated;
    }

    private static bool IsClockwise(List<Vector2> points)
    {
        // Shoelace formula
        float sum = 0f;
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 current = points[i];
            Vector2 next = points[(i + 1) % points.Count];
            sum += (next.x - current.x) * (next.y + current.y);
        }
        return sum > 0f;
    }
    
    #endregion
}



