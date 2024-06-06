using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PanelSwitcher : MonoBehaviour
{
    public FadeCanvasGroupsWave backgroundGroupWave;
    public RectTransform[] panels;
    public float swipeDuration = 0.5f;
    public FadeCanvasGroupsWave prevButton;
    public FadeCanvasGroupsWave nextButton;
    public bool isTriggered;
    bool isOpen;

    private int currentPanelIndex = 0;
    private bool isSwiping = false;
    private Vector2 originalPosition;

    RectTransform myRect;

    void Start()
    {
        myRect = GetComponent<RectTransform>();
        
        originalPosition = transform.localPosition;

        UpdateNavButtons();

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

        if (isOpen)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                SwipeLeft();
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                SwipeRight();
            }
        }
    }

    void FadeIn()
    {
        isOpen = true;
        transform.localPosition = originalPosition;
        currentPanelIndex = 0;
        panels[0].GetComponent<FadeCanvasGroupsWave>().isTriggered = true;
        backgroundGroupWave.isTriggered = true;
        UpdateNavButtons();
    }

    void FadeOut()
    {
        isOpen = false;
        isSwiping = false;
        backgroundGroupWave.isTriggered = false;
        currentPanelIndex = 0;

        foreach (RectTransform panel in panels)
        {
            panel.GetComponent<FadeCanvasGroupsWave>().isTriggered = false;
        }

        UpdateNavButtons();
    }

    void UpdateNavButtons()
    {
        if (isOpen)
        {
            // Update prev/next buttons
            if (currentPanelIndex >= panels.Length - 1)
                nextButton.isTriggered = false;
            else
                nextButton.isTriggered = true;
            if (currentPanelIndex <= 0)
                prevButton.isTriggered = false;
            else
                prevButton.isTriggered = true;
        }
        else
        {
            // Fade out
            prevButton.isTriggered = false;
            nextButton.isTriggered = false;
        }
    }

    public void SwipeLeft()
    {
        if (currentPanelIndex <= 0) return;

        currentPanelIndex--;
        StartCoroutine(SwipeToPanel(currentPanelIndex, currentPanelIndex + 1));

        UpdateNavButtons();
    }

    public void SwipeRight()
    {
        if (currentPanelIndex >= panels.Length - 1) return;

        currentPanelIndex++;
        StartCoroutine(SwipeToPanel(currentPanelIndex, currentPanelIndex - 1));

        UpdateNavButtons();
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
            // Fade in and out ** Handled by fade groups **  Could add functionality to this
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
