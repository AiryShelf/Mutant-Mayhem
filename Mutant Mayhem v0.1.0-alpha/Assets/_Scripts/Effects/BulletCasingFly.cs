using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCasingFly : MonoBehaviour
{
    
    [SerializeField] float scaleFactor = 0.5f;
    [SerializeField] float durationSensitivity;
    [SerializeField] float rotateSpeed;
    [SerializeField] float flyDist;
    [SerializeField] Vector2 flyDir;
    [SerializeField] float targetAccuracy = 1.5f;
    [SerializeField] Rigidbody2D myRb;

    [SerializeField] float startHeight;
    [SerializeField] float peakHeight;
    [SerializeField] float minScale = 1f;
    [SerializeField] float duration;
    [SerializeField] float stopSpeed = 1f;
    [SerializeField] string casingMethodName;
    public Transform casingTrans;
    Quaternion rot;
    float deg;

    void Start()
    {
        deg = Random.Range(0, 360);
        rot = Quaternion.Euler(0, 0, deg);
        transform.rotation = rot;

        Vector3 randomCirc = Random.insideUnitCircle * targetAccuracy;

        Vector3 worldDir = casingTrans.TransformDirection(flyDir.normalized)*flyDist + randomCirc;
        Vector3 target = casingTrans.position + worldDir;

        StartCoroutine(Fly(startHeight, peakHeight, duration));

        //Calculate initial force
        Vector3 dir = (target - transform.position).normalized;
        float dist = (transform.position - target).magnitude;
        myRb.AddForce(dir * dist / durationSensitivity, ForceMode2D.Impulse);

        rotateSpeed = Random.Range(-rotateSpeed, rotateSpeed);
        transform.parent = null;
    }

    void FixedUpdate()
    {
        deg += rotateSpeed;
        rot = Quaternion.Euler(0, 0, deg);
        transform.rotation = rot;
    }

    IEnumerator Fly(float startHeight, float peakHeight, float duration)
    {
        int currentFrame = 0;
        float elapsed = 0;
        while (elapsed < duration)
        {
            if (myRb.velocity.magnitude < stopSpeed && currentFrame > 10)
                break;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Calculate the current height
            float y = peakHeight * (t - t*t) + Mathf.Lerp(startHeight, 0, t);

            // Calculate the current scale
            float scale = scaleFactor * (t < 0.5f ? 2*t : 2*(1-t)) + y;
            scale = Mathf.Max(scale, minScale);
            transform.localScale = Vector3.one * scale;

            // Adjust scale base on height
            //transform.localScale *= y;

            currentFrame++;

            yield return null;
        }

        ParticleManager.Instance.PlayCasingEffectByName(casingMethodName, 
                                    transform, transform.rotation, false);
        Destroy(gameObject);
    }
}
