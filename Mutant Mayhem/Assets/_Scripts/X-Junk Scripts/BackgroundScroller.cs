using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera followCam;
    Material mat;
    Vector3 previousPos;

    void Start()
    {
        mat = GetComponent<SpriteRenderer>().material;
        previousPos = followCam.transform.position;
    }

    void FixedUpdate()
    {
        // Camera speed and direction
        Vector2 dir = followCam.transform.position - previousPos;
        float ratio = dir.magnitude / mat.mainTextureOffset.magnitude;

        transform.position = (Vector2)followCam.transform.position;
        mat.mainTextureOffset -= dir.normalized * ratio;

        previousPos = followCam.transform.position;
    }
}
