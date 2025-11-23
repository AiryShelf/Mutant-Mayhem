using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem Instance;
    public static bool fadedOut;

    public Tooltip tooltip;
    public float tooltipDelay = 0.5f;
    public float fadeTime;
    public CanvasGroup tooltipCanvasGroup;
    Coroutine fadeIn;
    Coroutine fadeOut;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        tooltipCanvasGroup.alpha = 0;
        fadedOut = true;
    }

    public static void Show(string content, string header = "")
    {
        Instance.tooltip.SetText(content, header);
        //current.tooltip.gameObject.SetActive(true);
        //current.tooltipAnim.SetBool("isOn", true);
        Instance.StartFadeIn();
    }

    public static void Hide()
    {
        if (Instance != null)
            Instance.StartFadeOut();
    }

    void StartFadeIn()
    {
        if (fadeOut != null)
        {
            StopCoroutine(fadeOut);
            fadeOut = null;
        }

        if (fadeIn == null)
        {
            fadeIn = StartCoroutine(FadeIn());
        }
        else
        {
            StopCoroutine(fadeIn);
            fadeIn = StartCoroutine(FadeIn());
            //Debug.Log("Tooltip did not StartFadeIn");
        }
    }

    void StartFadeOut()
    {
        if (fadeIn != null)
        {
            StopCoroutine(fadeIn);
            fadeIn = null;
        }

        if (fadeOut == null)
        {
            fadeOut = StartCoroutine(FadeOut());
        }
        else
        {
            StopCoroutine(fadeOut);
            fadeOut = StartCoroutine(FadeOut());
            //Debug.Log("Did not StartFadeOut");
        }
    }

    IEnumerator FadeIn()
    {
        fadedOut = false;

        float timeElapsed = 0;
        float currentAlpha = tooltipCanvasGroup.alpha;
        while (currentAlpha * fadeTime + timeElapsed < fadeTime)
        {
            timeElapsed += Time.unscaledDeltaTime;
            float value = Mathf.Lerp(0, 1, currentAlpha * fadeTime + timeElapsed / fadeTime);
            tooltipCanvasGroup.alpha = value;
            yield return new WaitForEndOfFrame();
        }
        tooltipCanvasGroup.alpha = 1;
        fadeIn = null;
    }

    IEnumerator FadeOut()
    {
        float timeElapsed = 0;
        float currentAlpha = tooltipCanvasGroup.alpha;
        while ((1 - currentAlpha) * fadeTime + timeElapsed < fadeTime)
        {
            timeElapsed += Time.unscaledDeltaTime;
            float value = Mathf.Lerp(1, 0, (1 - currentAlpha) * fadeTime + timeElapsed / fadeTime);
            tooltipCanvasGroup.alpha = value;
            yield return new WaitForEndOfFrame();
        }
        tooltipCanvasGroup.alpha = 0;
        //current.tooltip.gameObject.SetActive(false);
        fadeOut = null;
        fadedOut = true;
    }

}
