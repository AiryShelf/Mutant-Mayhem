using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationRemover : MonoBehaviour
{
    void FixedUpdate()
    {
        transform.rotation = Quaternion.identity;
    }
}
