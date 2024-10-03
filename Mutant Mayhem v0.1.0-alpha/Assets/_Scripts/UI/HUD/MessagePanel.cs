using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessagePanel : MonoBehaviour
{
    public static MessagePanel Instance;

    [SerializeField] TextMeshProUGUI messageText;
    [SerializeField] CanvasGroup messageCanvasGroup;
    [SerializeField] TextPulser textPulser;
    [SerializeField] float timeToDisplay = 6f;
    [SerializeField] float pulseTime = 0.5f;
    [SerializeField] float minMessageTime = 2f;

    Color startColor;

    void Awake()
    {
        Instance = this;
        messageCanvasGroup.alpha = 0;
        startColor = messageText.color;
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
