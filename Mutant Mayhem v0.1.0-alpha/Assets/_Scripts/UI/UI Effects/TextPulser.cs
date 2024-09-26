using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextPulser : MonoBehaviour
{
    [Header("Optional:")]
    [SerializeField] bool pulseOnEnable;
    [SerializeField] TextMeshProUGUI textToPulse;
    [SerializeField] Color pulseInColor;
    [SerializeField] Color pulseOutColor;
    [SerializeField] float pulseTime;
    [SerializeField] float timeToDisplay; // If zero, pulse forever

    Coroutine flashMessage;

    void OnEnable()
    {
        if (pulseOnEnable)
            flashMessage = StartCoroutine(PulseIn(textToPulse, pulseTime));
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    public void PulseTimedText(TextMeshProUGUI textToPulse, Color pulseColor, 
                          Color startColor, float pulseTime, float timeToDisplay)
    {
        pulseInColor = pulseColor;
        pulseOutColor = startColor;
        
        if (flashMessage != null)
        {
            StopAllCoroutines();
            textToPulse.color = startColor;
        }
        flashMessage = StartCoroutine(FlashMessage(textToPulse, timeToDisplay, pulseTime));
    }

    IEnumerator FlashMessage(TextMeshProUGUI textToPulse, float timeToDisplay, float pulseTime)
    {
        StartCoroutine(PulseIn(textToPulse, pulseTime));
        CanvasGroup messageCanvasGroup = textToPulse.GetComponent<CanvasGroup>();

        messageCanvasGroup.alpha = 1;

        yield return new WaitForSecondsRealtime(timeToDisplay);

        messageCanvasGroup.alpha = 0;
        textToPulse.color = pulseOutColor;
        StopAllCoroutines();
    }

    IEnumerator PulseIn(TextMeshProUGUI textToPulse, float pulseTime)
    {
        float timeElapsed = 0;
        while (timeElapsed < pulseTime)
        {
            Color newColor = Color.Lerp(pulseOutColor, pulseInColor, timeElapsed / pulseTime);
            textToPulse.color = newColor;
            yield return null;
            timeElapsed += Time.unscaledDeltaTime;
        }
        StartCoroutine(PulseOut(textToPulse, pulseTime));
    }

    IEnumerator PulseOut(TextMeshProUGUI textToPulse, float pulseTime)
    {
        float timeElapsed = 0;
        while (timeElapsed < pulseTime)
        {
            Color newColor = Color.Lerp(pulseInColor, pulseOutColor, timeElapsed / pulseTime);
            textToPulse.color = newColor;
            yield return null;
            timeElapsed += Time.unscaledDeltaTime;
        }
        StartCoroutine(PulseIn(textToPulse, pulseTime));
    }

}
