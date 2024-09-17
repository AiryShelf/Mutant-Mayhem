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
    Vector2 centerScreen;
    QCubeController qCubeController;

    void Start()
    {
        wind = FindObjectOfType<WindZone>();
        if (wind == null)
        {
            Debug.LogError("Could not find WindZone");
        }
        windSpeed = wind.windMain / 50;

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
    }


    void FixedUpdate()
    {
        // Scroll texture across screen
        GameTools.TextureOffsetCentered(atmosphere, transform, qCubeController.transform, 
                                        windDirection, windSpeed, ref atmosphereOffset);
    }

    // Now in GameTools
    void MoveAtmosphere()
    {
        centerScreen = Camera.main.transform.position;
        transform.position = centerScreen;
        atmosphereOffset += windDirection * windSpeed * Time.deltaTime;
        Vector2 newOffset = centerScreen + atmosphereOffset;

        atmosphere.mainTextureOffset = newOffset;
    }
}
