using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CreditsRoll : MonoBehaviour
{
    [SerializeField] RectTransform creditsRect;
    [SerializeField] FadeCanvasGroupsWave creditsFadeGroup;
    [SerializeField] TextMeshProUGUI creditsButtonText;
    [SerializeField] float scrollSpeed;
    [SerializeField] float timeToReturn = 50;

    Vector2 startPos;

    void Start()
    {
        startPos = creditsRect.anchoredPosition;
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void RollCredits()
    {
        StopAllCoroutines();

        creditsFadeGroup.isTriggered = true;
        creditsRect.anchoredPosition = startPos;
        creditsButtonText.text = "Return";
        EventSystem.current.SetSelectedGameObject(null);
        
        StartCoroutine(DelayReturn());
        StartCoroutine(Scroll());
    }

    public void StopCredits()
    {
        creditsFadeGroup.isTriggered = false;
        creditsButtonText.text = "Credits";
        StopAllCoroutines();
    }

    IEnumerator Scroll()
    {
        while (true)
        {
            Vector2 newPos = new Vector2(creditsRect.anchoredPosition.x, creditsRect.anchoredPosition.y + scrollSpeed);
            creditsRect.anchoredPosition = newPos;
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator DelayReturn()
    {
        yield return new WaitForSecondsRealtime(timeToReturn);
        StopCredits();
    }
}
