using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throw : MonoBehaviour
{
    [SerializeField] GameObject prefabOnLanding;
    [SerializeField] float scaleFactor = 0.5f;
    [SerializeField] float height = 3f;
    [SerializeField] float durationSensitivity;
    [SerializeField] float rotationAmount;
    [SerializeField] SpriteRenderer mySR;
    
    Quaternion rot;
    float deg;

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        
    }

    public void StartFly()
    {
        // Set new sorting layer
        mySR.sortingLayerID = SortingLayer.NameToID("FireParticles");
        
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float throwDuration = (transform.position - mousePos).magnitude / durationSensitivity;

        StartCoroutine(ThrowGrenade(transform.position, mousePos, height, throwDuration));
    }

    IEnumerator ThrowGrenade(Vector3 start, Vector3 end, float height, float duration)
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
            float y = height * (t - t*t) + Mathf.Lerp(start.y, end.y, t);

            // Update the position of the grenade
            transform.position = new Vector3(x, y, transform.position.z);

            // Scale the grenade
            float scale = scaleFactor * (t < 0.5f ? 2*t : 2*(1-t));
            transform.localScale = Vector3.one * (scaleFactor+scale);

            yield return null;
        }
        if (prefabOnLanding != null)
        {
            Instantiate(prefabOnLanding, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
