using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtmosphereController : MonoBehaviour
{
    [SerializeField] float windSpeed;
    [SerializeField] Vector2 windDirection;
    WindZone wind;
    Material atmosphere;
    Vector2 atmosphereOffset;
    Vector2 centerScreen;

    void Start()
    {
        wind = FindObjectOfType<WindZone>();
        windSpeed = wind.windMain / 50;
        atmosphere = GetComponent<SpriteRenderer>().material;
    }


    void Update()
    {
        MoveAtmosphere();
    }

    void MoveAtmosphere()
    {
        centerScreen = Camera.main.transform.position;
        transform.position = centerScreen;
        atmosphereOffset += windDirection * windSpeed * Time.deltaTime;
        Vector2 newOffset = centerScreen/64 + atmosphereOffset;

        atmosphere.mainTextureOffset = newOffset;
    }
}
