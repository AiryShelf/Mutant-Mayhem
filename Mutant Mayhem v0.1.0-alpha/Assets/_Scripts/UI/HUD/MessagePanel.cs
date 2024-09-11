using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessagePanel : MonoBehaviour
{
    public static MessagePanel instance;

    [SerializeField] TextMeshProUGUI messageText;
    [SerializeField] CanvasGroup messageCanvasGroup;
    [SerializeField] TextPulser textPulser;
    [SerializeField] float timeToDisplay = 4f;

    Coroutine flashMessage;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        messageCanvasGroup.alpha = 0;
    }

    public static void ShowMessage(string message, Color color)
    {
        instance.DisplayMessage(message, color);
    }

    public void DisplayMessage(string message, Color color)
    {
        messageText.text = message;
        textPulser.pulseToColor = color;
        
        if (flashMessage != null)
            StopCoroutine(flashMessage);
        flashMessage = StartCoroutine(FlashMessage());
    }

    IEnumerator FlashMessage()
    {
        messageCanvasGroup.alpha = 1;
        float timeElapsed = 0;

        while (timeElapsed < timeToDisplay)
        {
            yield return new WaitForFixedUpdate();
            timeElapsed += Time.fixedDeltaTime;
        }
        messageCanvasGroup.alpha = 0;
    }
}
