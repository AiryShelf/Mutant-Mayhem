using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SplashController : MonoBehaviour
{
    [SerializeField] float fadeTime = 1f;     // time to fade in/out
    [SerializeField] float displayTime = 3f;  // how long to show splash
    [SerializeField] float scaleAmount = 1.1f; // final scale multiplier
    [SerializeField] string nextSceneName = "MainMenu";
    [SerializeField] CanvasGroup canvasGroup;
    bool skipping = false;

    void Start()
    {
        StartCoroutine(FadeEffect());
        StartCoroutine(SlowScaleEffect());
    }

    void Update()
    {
        // Skip splash on any key press using new input system
        if (skipping) return;

        // Detect any key, mouse click, touch, or controller input
        if (Keyboard.current.anyKey.wasPressedThisFrame ||
            Mouse.current.leftButton.wasPressedThisFrame ||
            Mouse.current.rightButton.wasPressedThisFrame ||
            (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.allControls.Any(c => c.IsPressed())))
        {
            skipping = true;
            SceneManager.LoadScene(nextSceneName);
        }
    }

    IEnumerator FadeEffect()
    {
        // Fade in, hold for displayTime, fade out
        
        float timer = 0f;
        // Fade in
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(timer / fadeTime);
            yield return null;
        }
        // Hold
        yield return new WaitForSeconds(displayTime);
        // Fade out
        timer = 0f;
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(timer / fadeTime);
            yield return null;
        }
        // Load next scene
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator SlowScaleEffect()
    {
        // Slowly scale up the splash image over time
        float scaleDuration = fadeTime + displayTime + fadeTime;
        float timer = 0f;
        Vector3 initialScale = canvasGroup.transform.localScale;
        Vector3 targetScale = initialScale * scaleAmount;
        while (timer < scaleDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / scaleDuration);
            canvasGroup.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            yield return null;
        }
    }
}