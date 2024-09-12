using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FadeCanvasGroups : MonoBehaviour
{
    [SerializeField] bool autoSelectFirstElement;
    [SerializeField] CanvasGroup initialGroup;
    [SerializeField] CanvasGroup myGroupMain;
    [SerializeField] GameObject childGroup;
    public List<CanvasGroup> individualGroups;

    [SerializeField] float fadeStartDelay = 1f;
    [SerializeField] float fadeNextDelay = 0f;
    [SerializeField] float fadeStartTime = 2f;
    [SerializeField] float fadeNextTime = 0.5f;
    [SerializeField] float fadeOutTime = 1f;
    Coroutine fadeIn;
    Coroutine fadeOut;
    public bool triggered;
    
    bool selectedFirst;

    void Start()
    {
        // Fade everything out
        myGroupMain.alpha = 0;
        foreach (CanvasGroup group in individualGroups)
        {
            group.alpha = 0;
        }

        // Turn the main child off, which contains all the individuals
        childGroup.SetActive(false);
    }

    void Update()
    {
        if (triggered && fadeIn == null)
        {
            childGroup.gameObject.SetActive(true);
            fadeIn = StartCoroutine(FadeInAndOutMains());
        }
        else if (!triggered && fadeIn != null && fadeOut == null)
        {
            StopAllCoroutines();
            fadeOut = StartCoroutine(FadeOut());
        }
    }

    IEnumerator FadeInAndOutMains()
    {
        if (initialGroup != null)
            initialGroup.interactable = false;

        yield return new WaitForSecondsRealtime(fadeStartDelay);  

        // Fade main panels
        float timeElapsed = 0;
        while (timeElapsed < fadeStartTime)
        {
            timeElapsed += Time.unscaledDeltaTime;
            float value = Mathf.Lerp(0, 1, timeElapsed / fadeStartTime);
            if (initialGroup != null)
                initialGroup.alpha = 1 - value;
            myGroupMain.alpha = value;

            yield return new WaitForEndOfFrame();
        }

        StartCoroutine(FadeInIndividuals());
    }

    IEnumerator FadeInIndividuals()
    {
        foreach (CanvasGroup group in individualGroups)
        {
            // Select first button
            if (!selectedFirst && autoSelectFirstElement && group.GetComponent<Button>())
            {
                EventSystem.current.SetSelectedGameObject(group.gameObject);
                selectedFirst = true;
            }
            yield return new WaitForSecondsRealtime(fadeNextDelay);

            // Fade In
            float timeElapsed = 0;
            while (timeElapsed < fadeNextTime)
            {
                timeElapsed += Time.unscaledDeltaTime;
                float value = Mathf.Lerp(0, 1, timeElapsed / fadeNextTime);
                group.alpha = value;

                yield return new WaitForEndOfFrame();
            }
        }
    }

    IEnumerator FadeOut()
    {
        EventSystem.current.SetSelectedGameObject(null);
        selectedFirst = false;

        if (initialGroup != null)
            initialGroup.interactable = true;

        float timeElapsed = 0;
        while (timeElapsed < fadeOutTime)
        {
            timeElapsed += Time.unscaledDeltaTime;
            float value = Mathf.Lerp(1, 0, timeElapsed / fadeOutTime);
            myGroupMain.alpha = value;

            foreach (CanvasGroup group in individualGroups)
            {
                group.alpha = value;
            }

            // Fade initial canvas back in
            if (initialGroup != null)
            {
                initialGroup.alpha = 1 - value;
            }

            yield return new WaitForEndOfFrame();
        }        

        childGroup.SetActive(false);
        fadeIn = null;
        fadeOut = null;
        
        Debug.Log("Finished fade out");
    }
}
