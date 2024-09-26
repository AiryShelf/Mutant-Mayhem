using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordController : MonoBehaviour
{
    [SerializeField] MeleeControllerPlayer meleeControllerPlayer;
    [SerializeField] Transform handleTrans;
    [SerializeField] Transform tipTrans;
    public PolygonCollider2D polyCollider;
    [SerializeField] TrailRenderer trailRenderer;
    [SerializeField] float baseTrailWidth;
    [SerializeField] float minTrailWidth;
    
    Vector2 previousHandlePos;
    Vector2 previousTipPos;

    void OnEnable()
    {
        previousHandlePos = transform.InverseTransformPoint(meleeControllerPlayer.transform.position);
        previousTipPos = transform.InverseTransformPoint(meleeControllerPlayer.transform.position);
    }

    void FixedUpdate()
    {
        DrawSwordCollider();
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Enemy"))
        {
            meleeControllerPlayer.Hit(other, other.ClosestPoint(meleeControllerPlayer.transform.position));
        }
    }

    void DrawSwordCollider()
    {
        Vector2 handleWorldPos = handleTrans.position;
        Vector2 tipWorldPos = tipTrans.position;
        Vector2 meleeControllerWorldPos = meleeControllerPlayer.transform.position;

        Vector2 handlePos = transform.InverseTransformPoint(handleWorldPos);
        Vector2 tipPos = transform.InverseTransformPoint(tipWorldPos);
        Vector2 meleeControllerPos = transform.InverseTransformPoint(meleeControllerWorldPos);

        // Check for structures
        Vector2 tipDirection = tipWorldPos - meleeControllerWorldPos;
        RaycastHit2D hit = Physics2D.Raycast(meleeControllerWorldPos, tipDirection, 
                           Vector2.Distance(meleeControllerWorldPos, tipWorldPos), 
                           LayerMask.GetMask("Structures"));
        
        if (!meleeControllerPlayer.playerShooter.isElevated)
        {
            if (hit.collider != null)
            {
                // Adjust the tip position and convert to local space
                Vector2 hitLocalPoint = transform.InverseTransformPoint(hit.point);
                tipPos = hitLocalPoint;

                // Calculate the distance between the handle and the hit point
                float hitDistance = Vector2.Distance(handleWorldPos, hit.point);
                float distanceToTip = Vector2.Distance(handleWorldPos, tipWorldPos);

                // Adjust trail width based on the proportion of the distance to the hit point
                float widthMultiplier = hitDistance / distanceToTip;
                if (widthMultiplier <= minTrailWidth)
                    widthMultiplier = 0;

                // Set the trail width with the base value as a multiplier
                trailRenderer.widthMultiplier = baseTrailWidth * widthMultiplier;
            }
            else
            {
                // No collision, so set the trail width to the full base value
                trailRenderer.widthMultiplier = baseTrailWidth;
            }
        }
        else
        {
            trailRenderer.widthMultiplier = baseTrailWidth;
        }

        // Draw polygon collider
        Vector2[] points = {meleeControllerPos, handlePos, tipPos,
                            previousTipPos, previousHandlePos, meleeControllerPos};
        polyCollider.SetPath(0, points);

        previousHandlePos = handlePos;
        previousTipPos = tipPos;
    }
}
