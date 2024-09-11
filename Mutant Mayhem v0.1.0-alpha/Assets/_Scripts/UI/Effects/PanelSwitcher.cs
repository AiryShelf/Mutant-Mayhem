using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelSwitcher : MonoBehaviour
{
    public FadeCanvasGroupsWave backgroundGroupWave;
    public RectTransform[] panels;
    public Button[] tabButtons;
    public float swipeDuration = 0.5f;
    public FadeCanvasGroupsWave prevButton;
    public FadeCanvasGroupsWave nextButton;
    public bool isTriggered;
    bool isOpen;

    private int currentPanelIndex = 0;
    private Vector2 originalPosition;
    ColorBlock defaultColorBlock;
    ColorBlock highlightedColorBlock;

    RectTransform myRect;

    void Start()
    {
        myRect = GetComponent<RectTransform>();
        originalPosition = transform.localPosition;

        // Set up colorBlocks for tab buttons
        defaultColorBlock = tabButtons[0].colors;
        highlightedColorBlock = defaultColorBlock;
        highlightedColorBlock.normalColor = defaultColorBlock.highlightedColor;

        UpdateNavButtons();
        AssignTabButtonHandlers();
    }

    void Update()
    {
        if (Time.timeScale == 0)
            return;
        
        // Handle opening/closing 
        if (isTriggered)
        {
            if (!isOpen)
                OpenPanel();
        }
        else
        {
            if (isOpen)
                ClosePanel();
        }

        // Check swipe input
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

    void AssignTabButtonHandlers()
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int index = i; // Capture the loop variable
            tabButtons[i].onClick.AddListener(() => OnTabClicked(index));
        }
    }

    public void OnTabClicked(int tabIndex)
    {
        if (tabIndex != currentPanelIndex)
        {
            StartCoroutine(SwipeToPanel(tabIndex, currentPanelIndex));
            currentPanelIndex = tabIndex;
            UpdateNavButtons();
            UpdateTabHighlight();  // Update visual highlight of the tabs
        }
    }

    void UpdateTabHighlight()
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            
            // Highlight selected tab only
            if (i == currentPanelIndex)
            {
                tabButtons[i].colors = highlightedColorBlock;
                tabButtons[i].Select();
            }
            else
                tabButtons[i].colors = defaultColorBlock;  // Reset default
        }
    }

    void OpenPanel()
    {
        isOpen = true;
        transform.localPosition = originalPosition;
        currentPanelIndex = 0;
        panels[0].GetComponent<FadeCanvasGroupsWave>().isTriggered = true;
        backgroundGroupWave.isTriggered = true;
        UpdateNavButtons();
        UpdateTabHighlight();
    }

    void ClosePanel()
    {
        isOpen = false;
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
        /* Testing looping through panels
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
        */
    }

    public void SwipeLeft()
    {
        if (currentPanelIndex <= 0)
        {
            currentPanelIndex = panels.Length - 1;
            StartCoroutine(SwipeToPanel(currentPanelIndex, 0));
        }
        else
        {
            currentPanelIndex--;
            StartCoroutine(SwipeToPanel(currentPanelIndex, currentPanelIndex + 1));
        }

        UpdateNavButtons();
        UpdateTabHighlight();
    }

    public void SwipeRight()
    {
        if (currentPanelIndex >= panels.Length - 1) 
        {
            currentPanelIndex = 0;
            StartCoroutine(SwipeToPanel(currentPanelIndex, panels.Length - 1));
        }
        else
        {
            currentPanelIndex++;
            StartCoroutine(SwipeToPanel(currentPanelIndex, currentPanelIndex - 1));
        }

        UpdateNavButtons();
        UpdateTabHighlight();
    }

    IEnumerator SwipeToPanel(int targetIndex, int prevIndex)
    {
        Vector2 startPosition = transform.localPosition;
        Vector2 endPosition = originalPosition - new Vector2(targetIndex * myRect.sizeDelta.x, 0);

        CanvasGroup prevCanv = panels[prevIndex].GetComponent<CanvasGroup>();
        CanvasGroup targCanv = panels[targetIndex].GetComponent<CanvasGroup>();

        prevCanv.GetComponent<FadeCanvasGroupsWave>().isTriggered = false;
        targCanv.GetComponent<FadeCanvasGroupsWave>().isTriggered = true;

        float timeElapsed = 0;
        while (timeElapsed < swipeDuration)
        {
            yield return null;

            transform.localPosition = Vector2.Lerp(startPosition, endPosition, timeElapsed / swipeDuration);
            timeElapsed += Time.deltaTime;
        }

        transform.localPosition = endPosition;
    }
}
