using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class AtmosphereController : MonoBehaviour
{
    [SerializeField] float windSpeed;
    [SerializeField] Vector2 windDirection;
    WindZone wind;
    Material atmosphere;
    Vector2 atmosphereOffset;
    Vector2 previousPos;
    QCubeController qCubeController;

    void Start()
    {
        wind = FindObjectOfType<WindZone>();
        if (wind == null)
        {
            Debug.LogError("Could not find WindZone");
        }

        atmosphere = GetComponent<SpriteRenderer>().material;
        if (atmosphere == null)
        {
            Debug.LogError("Could not find Atmosphere material");
        }

        qCubeController = FindObjectOfType<QCubeController>();
        if (qCubeController == null)
        {
            Debug.LogError("Could not find qCubeController");
        }

        previousPos = Camera.main.transform.position;
    }

    void Update()
    {
        // Scroll opposite of camera
        transform.position = (Vector2)Camera.main.transform.position;
        Vector2 cameraDir = (Vector2)transform.position - previousPos;
        atmosphere.mainTextureOffset += cameraDir/64;

        // Scroll with wind
        windSpeed = wind.windMain / 50;
        atmosphere.mainTextureOffset += windDirection * windSpeed * Time.deltaTime;

        previousPos = transform.position;
    }

    
}
