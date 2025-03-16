using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnToPoolSingleFixed : MonoBehaviour
{
    [SerializeField] string objectPoolName;

    void OnEnable()
    {
        StartCoroutine(DestroyAfterFixedUpdate());
    }
    
    IEnumerator DestroyAfterFixedUpdate()
    {
        yield return new WaitForFixedUpdate();
        PoolManager.Instance.ReturnToPool(objectPoolName, gameObject);
    }
}
