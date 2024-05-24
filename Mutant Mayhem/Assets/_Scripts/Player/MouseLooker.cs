using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLooker : MonoBehaviour
{
    [SerializeField] float distDivisor;
    public Transform playerTrans;
    Vector3 mousePos;

    void Start()
    {
        //player = FindObjectOfType<Player>();
        gameObject.transform.parent = null;
    }


    void FixedUpdate()
    {
        if (playerTrans.GetComponent<Player>().isDead)
        {
            transform.position = playerTrans.position;
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
