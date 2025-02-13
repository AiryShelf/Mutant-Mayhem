using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunSights : MonoBehaviour
{
    public LineRenderer lineRendererCenter;
    public LineRenderer lineRendererLeftEdge;
    public LineRenderer lineRendererRightEdge;
    [SerializeField] float edgeLengthFactor = 0.7f;
    [SerializeField] int playerGunIndex;
    [SerializeField] bool repairGun = false;
    [SerializeField] LayerMask collisionMask;

    [Header("If not to mouse:")]
    [SerializeField] float defaultDist = 5;

    public bool isElevated;
    Player player;
    PlayerShooter playerShooter;
    float maxLength;
    float accuracy;

    public void Initialize(Player player)
    {
        this.player = player;
        playerShooter = player.playerShooter;
        isElevated = playerShooter.isElevated;
            
        RefreshSettings();
    }

    void LateUpdate()
    {
        if (repairGun)
            isElevated = true;
            
        float baseLength;
        
        if (repairGun)
        {
            Vector3 target;
            if (InputController.LastUsedDevice == Gamepad.current)
                target = CursorManager.Instance.GetCustomCursorWorldPos();
            else
                target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            target.z = 0;
            baseLength = (target - transform.position).magnitude;
        }
        else
        {
            baseLength = defaultDist;
        }

        baseLength = Mathf.Clamp(baseLength, 0, maxLength);

        var (centerLength, leftLength, rightLength) =
            !isElevated
                ? RaycastLines(baseLength)
                : (baseLength, baseLength, baseLength);

        RefreshSettings();
        UpdateSights(centerLength, leftLength, rightLength);
    }

    public (float centerLength, float leftLength, float rightLength) RaycastLines(float baseLength)
    {
        float centerLength = baseLength;
        RaycastHit2D centerHit = Physics2D.Raycast(
            transform.position, 
            transform.right, 
            centerLength, 
            collisionMask
        );

        if (centerHit.collider != null)
        {
            centerLength = centerHit.distance + 0.4f;
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
            leftLength = leftHit.distance + 0.4f;
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
            rightLength = rightHit.distance + 0.4f;
        }
        
        return (centerLength, leftLength, rightLength);
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
