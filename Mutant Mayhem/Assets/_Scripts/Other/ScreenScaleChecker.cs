using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ScreenScaleChecker : MonoBehaviour
{
    public static event Action<float> OnAspectRatioChanged;
    [SerializeField] float timeBetweenChecks = 1f;

    public static float CurrentAspectRatio = 16f / 9f;
    int lastWidth = 0;
    int lastHeight = 0;

    void Start()
    {
        CurrentAspectRatio = (float)Screen.width / Screen.height;
        StartCoroutine(CheckScreenScale());
        SceneManager.sceneLoaded += NewSceneLoaded;
        
        StartCoroutine(DelayInvoke());
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= NewSceneLoaded;
    }

    IEnumerator CheckScreenScale()
    {
        yield return new WaitForSecondsRealtime(1);

        while (true)
        {
            if (Screen.width != lastWidth || Screen.height != lastHeight)
            {
                lastWidth = Screen.width;
                lastHeight = Screen.height;
                CurrentAspectRatio = (float)lastWidth / lastHeight;
                OnAspectRatioChanged?.Invoke(CurrentAspectRatio);
            }
            
            yield return new WaitForSecondsRealtime(timeBetweenChecks);
        }
    }

    void NewSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(DelayInvoke());
    }

    IEnumerator DelayInvoke()
    {
        yield return new WaitForEndOfFrame();
        yield return null;
        yield return new WaitForSecondsRealtime(1);
        OnAspectRatioChanged?.Invoke(CurrentAspectRatio);
    }
}
