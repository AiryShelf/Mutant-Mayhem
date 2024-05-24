using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordController : MonoBehaviour
{
    [SerializeField] MeleeControllerPlayer meleeControllerPlayer;
    [SerializeField] Transform handleTrans;
    [SerializeField] Transform tipTrans;
    public PolygonCollider2D polyCollider;
    
    Vector2 previousHandlePos;
    Vector2 previousTipPos;

    void Start()
    {
        previousHandlePos = transform.InverseTransformPoint(handleTrans.position);
        previousTipPos = transform.InverseTransformPoint(tipTrans.position);
    }

    void FixedUpdate()
    {
        Vector2 handlePos = transform.InverseTransformPoint(handleTrans.position);
        Vector2 tipPos = transform.InverseTransformPoint(tipTrans.position);
        Vector2 meleeControllerPos = transform.InverseTransformPoint(meleeControllerPlayer.transform.position);

        Vector2[] points = {meleeControllerPos, handlePos, tipPos,
                            previousTipPos, previousHandlePos, meleeControllerPos};
        polyCollider.SetPath(0, points);

        previousHandlePos = handlePos;
        previousTipPos = tipPos;
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.tag == "Enemy")
        {
            meleeControllerPlayer.Hit(other, other.ClosestPoint(meleeControllerPlayer.transform.position));
        }
    }
}
