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

    void Awake()
    {
        Instance = this;
        messageCanvasGroup.alpha = 0;
    }

    public static void ShowMessage(string message, Color color)
    {
        Instance.DisplayMessage(message, color);
    }
    void DisplayMessage(string message, Color color)
    {
        if (textPulser != null)
            textPulser.DisplayMessage(messageText, message, color, timeToDisplay, pulseTime);
        else 
            Debug.LogError("Could not find textPulser to show message");
    }
}
