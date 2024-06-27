using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioListenerFollow : MonoBehaviour
{
    public Transform target;
    public float zPos;

    void Update()
    {
        // Follow target with locked zPos
        transform.position = new Vector3(target.position.x,
                                         target.position.y,
                                         zPos);
    }
}
