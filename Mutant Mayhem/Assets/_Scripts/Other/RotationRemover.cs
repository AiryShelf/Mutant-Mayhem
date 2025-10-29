using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationRemover : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(WaitToRemoveRotation());
    }
    
    IEnumerator WaitToRemoveRotation()
    {
        yield return new WaitForFixedUpdate();
        transform.rotation = Quaternion.identity;
    }
}
