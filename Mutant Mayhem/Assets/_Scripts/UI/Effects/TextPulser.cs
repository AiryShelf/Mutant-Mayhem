using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextPulser : MonoBehaviour
{
    public TextMeshProUGUI textToPulse;
    public Color pulseToColor = Color.black;
    public float pulseTime = 1f;

    Color startColor;

    void Awake()
    {
        startColor = textToPulse.color;
    }

    void OnEnable()
    {
        StartCoroutine(PulseIn());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator PulseIn()
    {
        float timeElapsed = 0;
        while (timeElapsed < pulseTime)
        {
            Color newColor = Color.Lerp(startColor, pulseToColor, timeElapsed / pulseTime);
            textToPulse.color = newColor;
            yield return new WaitForFixedUpdate();
            timeElapsed += Time.fixedDeltaTime;
        }
        StartCoroutine(PulseOut());
    }

    IEnumerator PulseOut()
    {
        float timeElapsed = 0;
        while (timeElapsed < pulseTime)
        {
            Color newColor = Color.Lerp(pulseToColor, startColor, timeElapsed / pulseTime);
            textToPulse.color = newColor;
            yield return new WaitForFixedUpdate();
            timeElapsed += Time.fixedDeltaTime;
        }
        StartCoroutine(PulseIn());
    }

}
