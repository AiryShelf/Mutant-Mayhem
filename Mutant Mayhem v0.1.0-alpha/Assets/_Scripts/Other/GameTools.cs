using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}

// Helper MonoBehaviour class to run coroutines
public class CoroutineHandler : MonoBehaviour { }