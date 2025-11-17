using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CriticalHitEffect : MonoBehaviour
{
    [SerializeField] string poolName = "Critical_Hit_FX";
    [SerializeField] Light2D critLight;
    [SerializeField] float duration = 0.2f;
    float lightStartIntensity;

    void Awake()
    {
        critLight.enabled = false;
        lightStartIntensity = critLight.intensity;
    }

    void OnEnable()
    {
        critLight.enabled = true;
        critLight.intensity = lightStartIntensity;
        StartCoroutine(ReturnToPoolAfterTime());
    }

    void OnDisable()
    {
        critLight.enabled = false;
        StopAllCoroutines();
    }

    IEnumerator ReturnToPoolAfterTime()
    {
        // Make intensity fade twice as fast as duration
        while (critLight.intensity > 0f)
        {
            critLight.intensity -= lightStartIntensity * Time.deltaTime / duration * 2f;
            yield return null;
        }
        PoolManager.Instance.ReturnToPool(poolName, this.gameObject);
    }
}
