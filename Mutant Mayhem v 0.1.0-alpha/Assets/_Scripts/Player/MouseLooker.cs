using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class MouseLooker : MonoBehaviour
{
    [SerializeField] float distDivisor;
    [SerializeField] Player player;
    public Transform playerTrans;
    public bool deathTriggered;
    public bool lockedToPlayer;
    Vector3 mousePos;

    void Start()
    {
        gameObject.transform.parent = null;
    }

    void FixedUpdate()
    {
        if (deathTriggered)
        {
            return;
        }

        if (lockedToPlayer)
        {
            transform.position = player.transform.position;
        }

        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector3 difference = mousePos - playerTrans.position;
        difference /= distDivisor;
        Vector3 newPos = playerTrans.position + difference;
        transform.position = newPos;
    }
}
