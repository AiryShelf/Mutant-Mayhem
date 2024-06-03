using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class MouseLooker : MonoBehaviour
{
    [SerializeField] float distDivisor;
    [SerializeField] Player player;
    [SerializeField] QCubeController qCubeController;
    [SerializeField] float timeToLerpOnDeath;
    [SerializeField] CinemachineMixingCamera mixingCamera;
    [SerializeField] float startCamera0MouseWeight = 0.4f;
    [SerializeField] float startCamera1PlayerWeight = 0.8f;

    public Transform playerTrans;
    Vector3 mousePos;

    bool deathTriggered;

    void Start()
    {
        gameObject.transform.parent = null;

        // Reset Camera weights
        mixingCamera.m_Weight0 = startCamera0MouseWeight;
        mixingCamera.m_Weight1 = startCamera1PlayerWeight;
    }

    void OnDisable()
    {
        mixingCamera.m_Weight0 = startCamera0MouseWeight;
        mixingCamera.m_Weight1 = startCamera1PlayerWeight;
    }


    void FixedUpdate()
    {
        if (!deathTriggered)
        {
            if (QCubeController.IsDestroyed)
            {
                deathTriggered = true;
                StartCoroutine(LerpToPosition(qCubeController.transform.position));
                StartCoroutine(LerpCameras(false));
            }
            else if (player.isDead)
            {
                deathTriggered = true;
                //StartCoroutine(LerpToPosition(player.transform.position));
                StartCoroutine(LerpCameras(true));
            }
            else
            {
                mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                Vector3 difference = mousePos - playerTrans.position;
                difference /= distDivisor;
                Vector3 newPos = playerTrans.position + difference;
                transform.position = newPos;
            }
        }
    }

    IEnumerator LerpToPosition(Vector3 pos)
    {
        Vector2 startPos = transform.position;
        float timeElapsed = 0;
        
        while (timeElapsed < timeToLerpOnDeath)
        {
            // Lerp to position
            timeElapsed += Time.deltaTime;
            Vector2 newPos = Vector3.Lerp(startPos, pos, timeElapsed / timeToLerpOnDeath);
            transform.position = newPos;

            yield return new WaitForEndOfFrame();
        }

        
    }

    IEnumerator LerpCameras(bool player)
    {
        float weight0 = mixingCamera.m_Weight0;
        float weight1 = mixingCamera.m_Weight1;
        float timeElapsed = 0;

        while (timeElapsed < timeToLerpOnDeath)
        {
            timeElapsed += Time.deltaTime;
            
            if (player)
            {
                // Lerp cameraMix to Player
                mixingCamera.m_Weight0 = Mathf.Lerp(weight0, 0, timeElapsed / timeToLerpOnDeath);
                mixingCamera.m_Weight1 = Mathf.Lerp(weight1, 1, timeElapsed / timeToLerpOnDeath);
            }
            else
            {
                // Lerp cameraMix to mouseLooker
                mixingCamera.m_Weight0 = Mathf.Lerp(weight0, 1, timeElapsed / timeToLerpOnDeath);
                mixingCamera.m_Weight1 = Mathf.Lerp(weight1, 0, timeElapsed / timeToLerpOnDeath);
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
