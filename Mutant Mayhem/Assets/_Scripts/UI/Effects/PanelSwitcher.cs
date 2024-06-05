using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelSwitcher : MonoBehaviour
{
    public FadeCanvasGroupsWave backgroundGroupWave;
    public RectTransform[] panels; // Array to hold your panels
    public float swipeDuration = 0.5f; // Duration for the swipe animation
    public bool isTriggered;
    bool isOpen;

    private int currentPanelIndex = 0;
    private bool isSwiping = false;
    private Vector2 originalPosition;

    RectTransform myRect;

    void Start()
    {
        myRect = GetComponent<RectTransform>();
        // Set the original position for the parent
        originalPosition = transform.localPosition;
        //UpdatePanelVisibility();
    }

    void Update()
    {
        if (isTriggered)
        {
            if (!isOpen)
                FadeIn();
        }
        else
        {
            if (isOpen)
                FadeOut();
        }

        if (isSwiping) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            SwipeLeft();
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            SwipeRight();
        }
    }

    void FadeIn()
    {
        isOpen = true;
        transform.localPosition = originalPosition;
        currentPanelIndex = 0;
        panels[0].GetComponent<FadeCanvasGroupsWave>().isTriggered = true;
        backgroundGroupWave.isTriggered = true;
    }

    void FadeOut()
    {
        isOpen = false;
        isSwiping = false;
        backgroundGroupWave.isTriggered = false;

        foreach (RectTransform panel in panels)
        {
            panel.GetComponent<FadeCanvasGroupsWave>().isTriggered = false;
        }
    }

    void SwipeLeft()
    {
        if (currentPanelIndex <= 0) return;

        currentPanelIndex--;
        StartCoroutine(SwipeToPanel(currentPanelIndex, currentPanelIndex + 1));
    }

    void SwipeRight()
    {
        if (currentPanelIndex >= panels.Length - 1) return;

        currentPanelIndex++;
        StartCoroutine(SwipeToPanel(currentPanelIndex, currentPanelIndex - 1));
    }

    IEnumerator SwipeToPanel(int targetIndex, int prevIndex)
    {
        //panels[targetIndex].gameObject.SetActive(true);
        isSwiping = true;
        Vector2 startPosition = transform.localPosition;
        Vector2 endPosition = originalPosition - new Vector2(targetIndex * myRect.sizeDelta.x, 0);

        CanvasGroup prevCanv = panels[prevIndex].GetComponent<CanvasGroup>();
        CanvasGroup targCanv = panels[targetIndex].GetComponent<CanvasGroup>();

        prevCanv.GetComponent<FadeCanvasGroupsWave>().isTriggered = false;
        targCanv.GetComponent<FadeCanvasGroupsWave>().isTriggered = true;

        float timeElapsed = 0;
        while (timeElapsed < swipeDuration)
        {
            transform.localPosition = Vector2.Lerp(startPosition, endPosition, timeElapsed / swipeDuration);
            // Fade in and out
            //targCanv.alpha = Mathf.Lerp(0, 1, timeElapsed / swipeDuration);
            //prevCanv.alpha = Mathf.Lerp(1, 0, timeElapsed / swipeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = endPosition;
        isSwiping = false;
        //UpdatePanelVisibility();
    }

    void UpdatePanelVisibility()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].gameObject.SetActive(i == currentPanelIndex);
        }
    }
}
