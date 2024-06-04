using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessagePanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI messageText;
    [SerializeField] CanvasGroup messageCanvasGroup;
    [SerializeField] TextPulser textPulser;
    [SerializeField] float timeToDisplay = 4f;

    void Start()
    {
        messageCanvasGroup.alpha = 0;
    }

    public void ShowMessage(string message, Color color)
    {
        messageText.text = message;
        textPulser.pulseToColor = color;
        StartCoroutine(FlashMessage());
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
