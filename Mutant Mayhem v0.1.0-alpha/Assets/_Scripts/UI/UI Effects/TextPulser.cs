using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextPulser : MonoBehaviour
{
    Color pulseInColor;

    Color startColor;
    Coroutine flashMessage;

    void Awake()
    {
        
    }

    void OnEnable()
    {

    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    public void DisplayMessage(TextMeshProUGUI textToPulse, string message, 
                               Color color, float timeToDisplay, float pulseTime)
    {
        textToPulse.text = message;
        pulseInColor = color;
        startColor = textToPulse.color;
        
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
        textToPulse.color = startColor;
        StopAllCoroutines();
    }

    IEnumerator PulseIn(TextMeshProUGUI textToPulse, float pulseTime)
    {
        float timeElapsed = 0;
        while (timeElapsed < pulseTime)
        {
            Color newColor = Color.Lerp(startColor, pulseInColor, timeElapsed / pulseTime);
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
            Color newColor = Color.Lerp(pulseInColor, startColor, timeElapsed / pulseTime);
            textToPulse.color = newColor;
            yield return null;
            timeElapsed += Time.unscaledDeltaTime;
        }
        StartCoroutine(PulseIn(textToPulse, pulseTime));
    }

}
