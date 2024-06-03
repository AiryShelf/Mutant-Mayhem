using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipSystem : MonoBehaviour
{
    private static TooltipSystem current;

    public Tooltip tooltip;
    public float fadeTime;
    public CanvasGroup tooltipCanvasGroup;
    Coroutine fadeIn;
    Coroutine fadeOut;

    void Awake()
    {
        tooltipCanvasGroup.alpha = 0;
        current = this;
    }

    public static void Show(string content, string header = "")
    {
        current.tooltip.SetText(content, header);
        //current.tooltip.gameObject.SetActive(true);
        //current.tooltipAnim.SetBool("isOn", true);
        current.StartFadeIn();
    }

    public static void Hide()
    {
        if (current != null)
            current.StartFadeOut();
    }

    void StartFadeIn()
    {
        if (fadeOut != null)
        {
            StopCoroutine(fadeOut);
            fadeOut = null;
        }

        if (fadeIn == null)
            fadeIn = StartCoroutine(FadeIn());
        else
            Debug.Log("Did not StartFadeIn");
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
            Debug.Log("Did not StartFadeOut");
        }
    }

    IEnumerator FadeIn()
    {
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
    }

}
