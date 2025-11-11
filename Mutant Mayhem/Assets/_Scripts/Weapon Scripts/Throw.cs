using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowGrenade : MonoBehaviour
{
    [SerializeField] string explosionObjectPoolName;
    [SerializeField] float scaleFactor = 0.5f;
    [SerializeField] float height = 3f;
    [SerializeField] float durationSensitivity;
    [SerializeField] float durationMinimum;
    [SerializeField] float rotationAmount;
    [SerializeField] SpriteRenderer mySR;
    public Vector2 target;
    
    Quaternion rot;
    float deg;

    public void StartFly()
    {
        // Set new sorting layer
        mySR.sortingLayerID = SortingLayer.NameToID("FireParticles");
        
        // Set how long to fly through air
        float throwDuration = (transform.position - (Vector3)target).magnitude / durationSensitivity;
        throwDuration = Mathf.Clamp(throwDuration,durationMinimum, float.MaxValue);

        StartCoroutine(ThrowObject(transform.position, target, height, throwDuration));
    }

    IEnumerator ThrowObject(Vector3 start, Vector3 end, float height, float duration)
    {
        rotationAmount = Random.Range(-rotationAmount, rotationAmount);
        float elapsed = 0;
        while (elapsed < duration)
        {
            // Rotate
            deg += rotationAmount;
            rot = Quaternion.Euler(0, 0, deg);
            transform.rotation = rot;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Calculate the current position on the parabola
            float x = Mathf.Lerp(start.x, end.x, t);
            float y = height * (t - t * t) + Mathf.Lerp(start.y, end.y, t);

            // Update the position of the grenade
            transform.position = new Vector3(x, y, transform.position.z);

            // Scale the grenade
            float scale = scaleFactor * (t < 0.5f ? 2 * t : 2 * (1 - t));
            transform.localScale = Vector3.one * (scaleFactor + scale);

            yield return null;
        }

        GameObject obj = PoolManager.Instance.GetFromPool(explosionObjectPoolName);
        obj.transform.position = end;

        yield return null;

        Destroy(gameObject);
    }
}
