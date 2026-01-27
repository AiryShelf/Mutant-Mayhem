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
    [SerializeField] Transform spriteTransform;
    [SerializeField] float baseTrailWidth;
    [SerializeField] float minTrailWidth;

    Vector2 previousHandlePos;
    Vector2 previousTipPos;
    Vector2 handleWorldPos;
    Vector2 tipWorldPos;
    Vector2 meleeControllerWorldPos;
    Vector2 handlePos;
    Vector2 tipPos;
    Vector2 meleeControllerPos;
    Vector2 tipDirection;
    RaycastHit2D hit;

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
        // Dont hit ground enemies when elevated
        if (meleeControllerPlayer.playerShooter.isElevated && other.gameObject.layer == LayerMask.NameToLayer("Enemies"))
            return;

        if (other.CompareTag("Enemy"))
        {
            meleeControllerPlayer.Hit(other, other.ClosestPoint(meleeControllerPlayer.transform.position));
        }
    }

    void DrawSwordCollider()
    {
        handleWorldPos = handleTrans.position;
        tipWorldPos = tipTrans.position;
        meleeControllerWorldPos = meleeControllerPlayer.transform.position;

        handlePos = transform.InverseTransformPoint(handleWorldPos);
        tipPos = transform.InverseTransformPoint(tipWorldPos);
        meleeControllerPos = transform.InverseTransformPoint(meleeControllerWorldPos);

        // Check for structures or obstacles
        tipDirection = tipWorldPos - meleeControllerWorldPos;
        hit = Physics2D.Raycast(meleeControllerWorldPos, tipDirection, 
                   Vector2.Distance(meleeControllerWorldPos, tipWorldPos), 
                   LayerMask.GetMask("Structures"));
        
        if (!meleeControllerPlayer.playerShooter.isElevated)
        {
            if (hit.collider != null)
            {
                // Adjust the tip position for obstacles and convert to local space
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
                spriteTransform.localScale = new Vector2(1, widthMultiplier);
            }
            else
            {
                // No collision, so set the trail width to the full base value
                trailRenderer.widthMultiplier = baseTrailWidth;
                spriteTransform.localScale = new Vector2(1, 1);
            }
        }
        else
        {
            trailRenderer.widthMultiplier = baseTrailWidth;
            spriteTransform.localScale = new Vector2(1, 1);
        }

        // Draw polygon collider
        Vector2[] points = {meleeControllerPos, handlePos, tipPos,
                            previousTipPos, previousHandlePos, meleeControllerPos};
        polyCollider.SetPath(0, points);

        previousHandlePos = handlePos;
        previousTipPos = tipPos;
    }
}
