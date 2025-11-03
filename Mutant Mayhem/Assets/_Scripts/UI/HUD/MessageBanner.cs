using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessageBanner : MonoBehaviour
{
    public static MessageBanner Instance;

    [SerializeField] TextMeshProUGUI messageText;
    [SerializeField] CanvasGroup messageCanvasGroup;
    [SerializeField] TextPulser textPulser;
    [SerializeField] float timeToDisplay = 6f;
    [SerializeField] float pulseTime = 0.5f;
    public static float TimeToDisplay;

    Color startColor;

    void Awake()
    {
        // Still gets detroyed on load
        Instance = this;

        TimeToDisplay = timeToDisplay;

        messageCanvasGroup.alpha = 0;
        startColor = messageText.color;
    }

    public void DelayMessage(string message, Color color, float delay)
    {
        StartCoroutine(DelayMessageRoutine(message, color, delay));
    }

    IEnumerator DelayMessageRoutine(string message, Color color, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        PulseMessage(message, color);
    }

    public static void PulseMessage(string message, Color pulseColor)
    {
        if (Instance != null)
            Instance.DisplayAndPulse(message, pulseColor);
    }


    void DisplayAndPulse(string message, Color pulseColor)
    {
        messageText.text = message;

        if (textPulser != null)
            textPulser.PulseTimedText(messageText, pulseColor, startColor, pulseTime, timeToDisplay);
        else 
            Debug.LogError("Could not find textPulser to show message");
    }
}
