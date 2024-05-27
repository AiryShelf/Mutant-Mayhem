using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FadeCanvasGroups1 : MonoBehaviour
{
    [SerializeField] bool autoSelectFirstElement;
    [SerializeField] int batchSize = 1;
    [SerializeField] CanvasGroup initialGroup;
    [SerializeField] CanvasGroup myGroupMain;
    [SerializeField] GameObject childGroup;
    public List<CanvasGroup> individualGroups;

    [SerializeField] float fadeStartDelay = 0f;
    [SerializeField] float delayBetweenElements = 0.3f;
    [SerializeField] float fadeStartTime = 1f;
    [SerializeField] float fadeNextElementTime = 0.5f;
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
            fadeOut = StartCoroutine(FadeOutAll());
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


        int batch = batchSize;
        foreach (CanvasGroup group in individualGroups)
        {       
            StartCoroutine(FadeInIndividual(group));
            batch--;

            if (batch == 0)
            {
                yield return new WaitForSecondsRealtime(delayBetweenElements);
                batch = batchSize;
            }
        }
    }

    IEnumerator FadeInIndividual(CanvasGroup group)
    {
        // Select first button
        if (!selectedFirst && autoSelectFirstElement && group.GetComponent<Button>())
        {
            EventSystem.current.SetSelectedGameObject(group.gameObject);
            selectedFirst = true;
        }

        // Fade In
        float timeElapsed = 0;
        while (timeElapsed < fadeNextElementTime)
        {
            timeElapsed += Time.unscaledDeltaTime;
            float value = Mathf.Lerp(0, 1, timeElapsed / fadeNextElementTime);
            group.alpha = value;

            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator FadeOutAll()
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
        
        //Debug.Log("Finished fade out");
    }
}
