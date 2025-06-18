using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// This code is a mess, it's one of the first tools I've made myself
// ** ADD REGIONS AND COMMENTS, CLEAN UP IF POSSIBLE **
// At a recent quick glance, it looks like a state machine might work well?
public class FadeCanvasGroupsWave : MonoBehaviour
{
    [SerializeField] bool autoSelectFirstElement;
    [SerializeField] bool deactivateIndivsWithFade;
    [SerializeField] bool fadeOutInWave = false;
    public int batchSize = 1;
    [SerializeField] CanvasGroup initialGroup;
    [SerializeField] CanvasGroup myGroup;
    [SerializeField] FadeCanvasGroupsWave nextCanvasGroupsWave;
    [SerializeField] GameObject deactivateGroup;
    public List<CanvasGroup> individualElements;

    [SerializeField] float fadeStartDelay = 0f;
    [SerializeField] float delayBetweenElements = 0.1f;
    [SerializeField] float fadeStartTime = 1f;
    [SerializeField] float fadeIndivTime = 0.5f;
    public float fadeOutAllTime = 1f;
    [SerializeField] float lerpOutStopThreshold = 0.1f;
    
    Coroutine fadeIn;
    Coroutine fadeOut;
    [HideInInspector] public bool isTriggered;
    public bool startTriggered = false;
    
    bool hasSelectedFirst;

    void Start()
    {
        if (startTriggered)
            return;
            
        InitializeToFadedOut();
    }

    public void InitializeToFadedOut()
    {
        // Fade everything out
        if (myGroup)
        {
            myGroup.alpha = 0;
            myGroup.interactable = false;
            myGroup.blocksRaycasts = false;
        }
        foreach (CanvasGroup group in individualElements)
        {
            if (group == null)
                continue;
                
            group.alpha = 0;
            if (deactivateIndivsWithFade)
                group.gameObject.SetActive(false);
        }

        // Refresh
        foreach (CanvasGroup group in individualElements)
        {
            group.alpha = 0;
            if (deactivateIndivsWithFade)
                group.gameObject.SetActive(false);
        }

        // Turn the main child off, which contains all the individuals
        if (deactivateGroup)
            deactivateGroup.SetActive(false);
        
        isTriggered = false;
    }

    void Update()
    {
        if (isTriggered && fadeIn == null)
        {
            StopAllCoroutines();
            if (deactivateGroup)
                deactivateGroup.gameObject.SetActive(true);
    
            fadeIn = StartCoroutine(FadeMainIn());
        }
        else if (!isTriggered && fadeIn != null && fadeOut == null)
        {
            StopAllCoroutines();
            if (!fadeOutInWave)
                fadeOut = StartCoroutine(FadeOutAll());
            else
            {
                fadeOut = StartCoroutine(FadeOutWave());
            }
        }
    }

    public void ToggleTrigger()
    {
        if (!isTriggered)
            isTriggered = true;
        else
            isTriggered = false;
    }

    IEnumerator FadeMainIn()
    {
        if (fadeOut != null)
        {
            StopCoroutine(fadeOut);
            fadeOut = null;
        }
        
        if (initialGroup != null)
        {
            // Stop any fade ins.
            //FadeCanvasGroupsWave initialFadeGroup = initialGroup.GetComponent<FadeCanvasGroupsWave>();
            //initialFadeGroup.isTriggered = false;

            initialGroup.interactable = false;
            initialGroup.blocksRaycasts = false;
        }

        yield return new WaitForSecondsRealtime(fadeStartDelay);  

        if (myGroup)
        {
            myGroup.interactable = true;
            myGroup.blocksRaycasts = true;

            // Fade main panels
            float timeElapsed = 0;
            while (timeElapsed < fadeStartTime)
            {
                timeElapsed += Time.unscaledDeltaTime;
                float value = Mathf.Lerp(0, 1, timeElapsed / fadeStartTime);
                if (initialGroup != null)
                    initialGroup.alpha = 1 - value;
                if (myGroup)
                    myGroup.alpha = value;

                yield return null;
            }
        }

        if (initialGroup != null)
        {
            initialGroup.alpha = 0;
        }

        // Start next Wave
        if (nextCanvasGroupsWave != null)
        {
            nextCanvasGroupsWave.gameObject.SetActive(true);
            nextCanvasGroupsWave.isTriggered = true;
        }

        // Fades in 'batchSize' number of elements simultaneously
        int batch = batchSize;
        foreach (CanvasGroup individual in individualElements)
        {      
            if (individual == null)
                continue;
                 
            StartCoroutine(FadeInIndividual(individual));
            batch--;

            if (batch == 0)
            {
                yield return new WaitForSecondsRealtime(delayBetweenElements);
                batch = batchSize;
            }
        }
    }

    IEnumerator FadeInIndividual(CanvasGroup individual)
    {
        // Activate
        if (deactivateIndivsWithFade)
            individual.gameObject.SetActive(true);

        individual.interactable = true;
        individual.blocksRaycasts = true;

        // Select first button
        if (!hasSelectedFirst && autoSelectFirstElement && individual.GetComponent<Button>())
        {
            EventSystem.current.SetSelectedGameObject(individual.gameObject);
            hasSelectedFirst = true;
        }

        // Fade In
        float timeElapsed = 0;
        while (timeElapsed < fadeIndivTime)
        {

            timeElapsed += Time.unscaledDeltaTime;
            float value = Mathf.Lerp(individual.alpha, 1, timeElapsed / fadeIndivTime);
            individual.alpha = value;

            yield return null;
        }
    }

    IEnumerator FadeOutWave()
    {
        if (fadeIn != null)
        {
            StopCoroutine(fadeIn);
            fadeIn = null;
        }
         // test **might need to check and stop coroutines before nullifying

        // Fades out 'batchSize' number of elements simultaneously
        int batch = batchSize;
        foreach (CanvasGroup group in individualElements)
        {       
            StartCoroutine(FadeOutIndividual(group));
            batch--;

            if (batch == 0)
            {
                yield return new WaitForSecondsRealtime(delayBetweenElements);
                batch = batchSize;
            }
        }

        StartCoroutine(FadeOutMain());
        yield return new WaitForSecondsRealtime(fadeIndivTime);
       
        if (deactivateGroup)
            deactivateGroup.SetActive(false);

        fadeIn = null;
        fadeOut = null;
    }
    

    IEnumerator FadeOutIndividual(CanvasGroup group)
    {
        group.interactable = false;
        group.blocksRaycasts = false;
        // Fade Out
        float timeElapsed = 0;
        while (timeElapsed < fadeIndivTime)
        {

            timeElapsed += Time.unscaledDeltaTime;
            float value = Mathf.Lerp(group.alpha, 0, timeElapsed / fadeIndivTime);
            group.alpha = value;

            // Allows quicker deactivation for lists
            if (value <= lerpOutStopThreshold)
            {
                timeElapsed = fadeIndivTime;
                group.alpha = 0;
            }

            yield return null;
        }

        // Deactivate
        if (deactivateIndivsWithFade)
            group.gameObject.SetActive(false);
    }

    IEnumerator FadeOutMain()
    {
        if (autoSelectFirstElement)
            EventSystem.current.SetSelectedGameObject(null);
        hasSelectedFirst = false;

        if (initialGroup != null)
        {
            initialGroup.gameObject.SetActive(true);
            initialGroup.interactable = true;  
            initialGroup.blocksRaycasts = true;  
        }

        if (myGroup != null)
        {
            myGroup.interactable = false;
            myGroup.blocksRaycasts = false;
        }

        float timeElapsed = 0;
        while (timeElapsed < fadeIndivTime)
        {
            timeElapsed += Time.unscaledDeltaTime;
            float value = Mathf.Lerp(1, 0, timeElapsed / fadeOutAllTime);

            bool stop = value <= lerpOutStopThreshold;

            // Fade myGroup out
            if (myGroup)
            {
                myGroup.alpha = value;
                if (stop)
                {
                    myGroup.alpha = 0;
                }
            }

            // Fade initial canvas back in
            if (initialGroup != null)
            {
                initialGroup.alpha = 1 - value;
                if (stop)
                {
                    initialGroup.alpha = 0;
                }   
            }

            if (stop)
            {
                timeElapsed = fadeIndivTime;
                //myGroup.alpha = 0;
            }       

            yield return null;
        }
    }

    IEnumerator FadeOutAll()
    {
        //if (fadeIn != null)
        //{
           // StopCoroutine(fadeIn);
           // fadeIn = null;
        //}
        
        if (autoSelectFirstElement)
            EventSystem.current.SetSelectedGameObject(null);
        hasSelectedFirst = false;

        if (initialGroup != null)
        {
            initialGroup.gameObject.SetActive(true);
            initialGroup.interactable = true;   
            initialGroup.blocksRaycasts = true;        
        }

        if (myGroup != null)
        {
            myGroup.interactable = false;
            myGroup.blocksRaycasts = false;
        }

        float timeElapsed = 0;
        while (timeElapsed < fadeOutAllTime)
        {
            timeElapsed += Time.unscaledDeltaTime;
            float value = Mathf.Lerp(1, 0, timeElapsed / fadeOutAllTime);

            bool stop = value <= lerpOutStopThreshold;

            // Fade myGroup out
            if (myGroup)
            {
                myGroup.alpha = value;
                if (stop)
                {
                    myGroup.alpha = 0;
                }
            }

            // Fade individuals out
            foreach (CanvasGroup group in individualElements)
            {
                group.alpha = value;
                if (stop)
                {
                    group.alpha = 0;
                }   
            }

            // Fade initial canvas back in
            if (initialGroup != null)
            {
                initialGroup.alpha = 1 - value;
                if (stop)
                {
                    initialGroup.alpha = 1;
                }   
            }

            if (stop)
            {
                timeElapsed = fadeOutAllTime;
                //myGroup.alpha = 0;
                //Debug.Log("Stop happened");
            }       

            yield return null;
        }

        // Deactivate
        if (deactivateIndivsWithFade)
        foreach (CanvasGroup group in individualElements)
            {
                group.gameObject.SetActive(false);
            }
                    
        if (deactivateGroup)
            deactivateGroup.SetActive(false);

        fadeIn = null;
        fadeOut = null;
        
        //Debug.Log("Finished fade out");
    }
}
