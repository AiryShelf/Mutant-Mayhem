using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Daylight : MonoBehaviour
{
    public static event Action OnSunrise;
    public static event Action OnSunset;
    public static bool isDay;

    [SerializeField] Light2D sunlight;

    [Header("Sunrise Colors")]
    [SerializeField] Color sunriseColor1; 
    [SerializeField] Color sunriseColor2; 
    [SerializeField] Color sunriseColor3;
    [Space]
    [SerializeField] Color midDayColor;

    [Header("Sunset Colors")]
    [SerializeField] Color sunsetColor1; 
    [SerializeField] Color sunsetColor2;
    [SerializeField] Color sunsetColor3; 
    [Space]
    [SerializeField] Color nightColor;

    [Header("Timing Settings")]
    [SerializeField] float lerpTime = 2f;
    [SerializeField] float color1Time = 1f;
    [SerializeField] float color2Time = 1f;
    [SerializeField] float color3Time = 1f;
    [SerializeField] float totalTime = 10;

    Camera mainCamera;
    Vector3 screenWidth;
    Coroutine lerpPos;
    Coroutine lerpColor;

    bool isSunUp;
    bool isSunDown;

    void Awake()
    {
        isDay = false;
    }

    void Start()
    {
        mainCamera = Camera.main;
        screenWidth = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width*1.75f, Screen.height/2, 0));
        StartCoroutine(PlaySunriseEffect());
        Vector3 cameraZero = new Vector3(Camera.main.transform.position.x, 
                                                 Camera.main.transform.position.y, 0);
        transform.position = cameraZero + screenWidth;
    }

    void Update()
    {
        // Ensure the light is always locked to the camera and y pos
        if (isSunUp)
        {
            sunlight.transform.position = new Vector3(mainCamera.transform.position.x, 
                                                      mainCamera.transform.position.y, 0);
        }
        else if (isSunDown)
        {
            sunlight.transform.position = new Vector3(mainCamera.transform.position.x - screenWidth.x, 
                                                      mainCamera.transform.position.y, 0);
        }
        else 
        {
            sunlight.transform.position = new Vector3(transform.position.x, 
                                                      mainCamera.transform.position.y, 0);
        }
    }

    public IEnumerator PlaySunriseEffect()
    {
        // Sunrise Effect
        //StopAllCoroutines();
        if (lerpPos != null) StopCoroutine(lerpPos);
        if (lerpColor != null) 
        {
            StopCoroutine(lerpColor);
            sunlight.color = sunriseColor1;
        }

        lerpPos = StartCoroutine(ChangeLightPos(true));
        lerpColor = StartCoroutine(ChangeLightColor(sunriseColor1, color1Time));
        yield return lerpColor;
        lerpColor = StartCoroutine(ChangeLightColor(sunriseColor2, color2Time));
        yield return lerpColor;

        OnSunrise?.Invoke();
        isDay = true;

        lerpColor = StartCoroutine(ChangeLightColor(sunriseColor3, color3Time));
        yield return lerpColor;
        lerpColor = StartCoroutine(ChangeLightColor(midDayColor, 1));
        yield return lerpColor;
    }

    public IEnumerator PlaySunsetEffect()
    {
        //Debug.Log("Sunset effect called");
        // Sunset Effect
        //StopAllCoroutines();
        if (lerpPos != null) StopCoroutine(lerpPos);
        
        if (lerpColor != null) 
        {
            StopCoroutine(lerpColor);
            yield return StartCoroutine(SetToMidDay());
        }
        
        lerpPos = StartCoroutine(ChangeLightPos(false));
        lerpColor = StartCoroutine(ChangeLightColor(sunsetColor1, color1Time));
        yield return lerpColor;

        OnSunset?.Invoke();
        isDay = false;

        lerpColor = StartCoroutine(ChangeLightColor(sunsetColor2, color2Time));
        yield return lerpColor;
        lerpColor = StartCoroutine(ChangeLightColor(sunsetColor3, color3Time));
        yield return lerpColor;
        lerpColor = StartCoroutine(ChangeLightColor(nightColor, 1));
        yield return lerpColor;
    }

    IEnumerator ChangeLightColor(Color targetColor, float waitTime)
    {
        Color initialColor = sunlight.color;
        float elapsedTime = 0f;

        while (elapsedTime < lerpTime)
        {
            sunlight.color = Color.Lerp(initialColor, targetColor, elapsedTime / lerpTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        sunlight.color = targetColor;

        yield return new WaitForSeconds(waitTime);
        lerpColor = null;
    }

    IEnumerator ChangeLightPos(bool sunUp)
    {
        float elapsedTime = 0f;

        if (sunUp)
        {
            //Debug.Log("ChangeLightPos sunUp");
            // Release locked to left pos
            isSunDown = false;

            while (elapsedTime < totalTime)
            {
                Vector3 cameraZero = new Vector3(Camera.main.transform.position.x, 
                                                 Camera.main.transform.position.y, 0);

                sunlight.transform.position = Vector3.Lerp(cameraZero + screenWidth, 
                                                 cameraZero, elapsedTime / totalTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            sunlight.transform.position = new Vector3(Camera.main.transform.position.x, 
                                                 Camera.main.transform.position.y, 0);
            isSunUp = true;
        }
        else
        {
            while (elapsedTime < totalTime)
            {
                //Debug.Log("ChangeLightPos sunDown");
                // Release locked to middle pos
                isSunUp = false;
                Vector3 cameraZero = new Vector3(Camera.main.transform.position.x,  
                                                 Camera.main.transform.position.y, 0);

                sunlight.transform.position = Vector3.Lerp(cameraZero, 
                                                 cameraZero - screenWidth, elapsedTime / totalTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            sunlight.transform.position = new Vector3(Camera.main.transform.position.x, 
                                                 Camera.main.transform.position.y, 0) - screenWidth;
            isSunDown = true;
        }
        lerpPos = null;
    }


    IEnumerator SetToMidDay()
    {
        //Debug.Log("Set to MidDay");
        Color initialColor = sunlight.color;
        Vector3 startPos = sunlight.transform.position;
        float elapsedTime = 0;
        while (elapsedTime < lerpTime / 2)
            {
                Vector3 cameraZero = new Vector3(Camera.main.transform.position.x, 
                                                 Camera.main.transform.position.y, 0);

                sunlight.color = Color.Lerp(initialColor, midDayColor, elapsedTime / (lerpTime / 2));
                sunlight.transform.position = Vector3.Lerp(startPos, 
                                                 cameraZero, elapsedTime / (lerpTime / 2));
                elapsedTime += Time.deltaTime;
                yield return null;
            }
    }
}