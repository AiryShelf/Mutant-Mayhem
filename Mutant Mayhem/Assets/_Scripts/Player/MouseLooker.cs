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


    void FixedUpdate()
    {
        if (!deathTriggered)
        {
            if (QCubeController.IsDead)
            {
                deathTriggered = true;
                mixingCamera.m_Weight0 = 1;
                mixingCamera.m_Weight1 = 0;
                StartCoroutine(LerpToPosition(qCubeController.transform.position));
            }
            else if (player.isDead)
            {
                deathTriggered = true;
                mixingCamera.m_Weight0 = 1;
                mixingCamera.m_Weight1 = 0;
                StartCoroutine(LerpToPosition(player.transform.position));
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
            timeElapsed += Time.deltaTime;
            Vector2 newPos = Vector3.Lerp(startPos, pos, timeElapsed / timeToLerpOnDeath);
            transform.position = newPos;
            yield return new WaitForEndOfFrame();
        }
    }
}
