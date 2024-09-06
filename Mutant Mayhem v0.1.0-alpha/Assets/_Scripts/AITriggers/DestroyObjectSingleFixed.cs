using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObjectSingleFixed : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(DestroyAfterFixedUpdate());
    }
    
    IEnumerator DestroyAfterFixedUpdate()
    {
        yield return new WaitForFixedUpdate();
        Destroy(gameObject);
    }
}
