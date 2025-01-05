using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSights : MonoBehaviour
{
    public LineRenderer lineRendererCenter;
    public LineRenderer lineRendererLeftEdge;
    public LineRenderer lineRendererRightEdge;
    [SerializeField] float edgeLengthFactor = 0.7f;
    [SerializeField] int playerGunIndex;
    [SerializeField] bool distToMouse = false;
    [SerializeField] LayerMask collisionMask;

    [Header("If not to mouse:")]
    [SerializeField] float defaultDist = 5;

    Player player;
    PlayerShooter playerShooter;
    float maxLength;
    float accuracy;

    public void Initialize(Player player)
    {
        this.player = player;
        playerShooter = player.playerShooter;
            
        RefreshSettings();
    }

    void LateUpdate()
    {
        float baseLength;
        
        if (distToMouse)
        {
            Vector3 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            target.z = 0;
            baseLength = (target - transform.position).magnitude;
        }
        else
        {
            baseLength = defaultDist;
        }

        baseLength = Mathf.Clamp(baseLength, 0, maxLength);

        float centerLength = baseLength;
        RaycastHit2D centerHit = Physics2D.Raycast(
            transform.position, 
            transform.right, 
            centerLength, 
            collisionMask
        );

        if (centerHit.collider != null)
        {
            centerLength = centerHit.distance;
        }

        float leftLength = baseLength * edgeLengthFactor;
        Vector2 leftDir = GameTools.RotateVector2(transform.right, accuracy);

        RaycastHit2D leftHit = Physics2D.Raycast(
            transform.position, 
            leftDir, 
            leftLength, 
            collisionMask
        );

        if (leftHit.collider != null)
        {
            leftLength = leftHit.distance;
        }
        float rightLength = baseLength * edgeLengthFactor;
        Vector2 rightDir = GameTools.RotateVector2(transform.right, -accuracy);

        RaycastHit2D rightHit = Physics2D.Raycast(
            transform.position, 
            rightDir, 
            rightLength, 
            collisionMask
        );

        if (rightHit.collider != null)
        {
            rightLength = rightHit.distance;
        }

        RefreshSettings();
        UpdateSights(centerLength, leftLength, rightLength);
    }

    public void RefreshSettings()
    {
        maxLength = playerShooter.gunList[playerGunIndex].bulletLifeTime *
                    playerShooter.gunList[playerGunIndex].bulletSpeed;
    }

    public void SetAccuracy(float accuracy)
    {
        this.accuracy = accuracy;
    }

    void UpdateSights(float centerLength, float leftLength, float rightLength)
    {
        lineRendererCenter.SetPosition(0, transform.position);
        Vector2 centerEndPos = transform.position + transform.right * centerLength;
        lineRendererCenter.SetPosition(1, centerEndPos);

        // If there are no edge line renderers, just exit.
        if (lineRendererLeftEdge == null && lineRendererRightEdge == null)
            return;

        Vector2 leftDir = GameTools.RotateVector2(transform.right, accuracy);
        lineRendererLeftEdge.SetPosition(0, transform.position);
        Vector2 leftEndPos = (Vector2)transform.position + (leftDir * leftLength);
        lineRendererLeftEdge.SetPosition(1, leftEndPos);

        Vector2 rightDir = GameTools.RotateVector2(transform.right, -accuracy);
        lineRendererRightEdge.SetPosition(0, transform.position);
        Vector2 rightEndPos = (Vector2)transform.position + (rightDir * rightLength);
        lineRendererRightEdge.SetPosition(1, rightEndPos);
    }
}
