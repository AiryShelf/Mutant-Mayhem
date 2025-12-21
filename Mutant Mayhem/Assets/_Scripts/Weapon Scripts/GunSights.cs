using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunSights : MonoBehaviour
{
    [SerializeField] LineRenderer lineRendererCenter;
    [SerializeField] LineRenderer lineRendererLeftEdge;
    [SerializeField] LineRenderer lineRendererRightEdge;
    [SerializeField] float edgeLengthFactor = 0.7f;
    public int playerGunIndex;
    [SerializeField] bool repairGun = false;
    [SerializeField] LayerMask collisionMask;

    [Header("If not to mouse:")]
    [SerializeField] float defaultDist = 5;

    public bool isElevated;
    public float elevatedRangeMult = 1.2f;
    Player player;
    PlayerShooter playerShooter;
    float maxLength;
    float accuracy;

    public void Initialize(Player player)
    {
        this.player = player;
        playerShooter = player.playerShooter;
        isElevated = playerShooter.isElevated;
        playerGunIndex = playerShooter.currentGunIndex;

        RefreshSettings();
    }

    void LateUpdate()
    {
        // Keep these in sync in case the player switches or upgrades guns.
        isElevated = playerShooter.isElevated;
        if (!repairGun)
            playerGunIndex = playerShooter.currentGunIndex;

        RefreshSettings();

        float baseLength;

        if (repairGun)
        {
            Vector3 target;
            if (InputManager.LastUsedDevice == Gamepad.current)
                target = CursorManager.Instance.GetCustomCursorWorldPos();
            else
                target = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            target.z = 0;
            baseLength = (target - transform.position).magnitude;
        }
        else
        {
            // Non-repair guns should show the gun's max range (bullet lifetime * speed).
            // If something is misconfigured and range is zero, fall back to a reasonable default.
            baseLength = maxLength > 0f ? maxLength : defaultDist;
        }

        if (isElevated)
            baseLength *= elevatedRangeMult;

        baseLength = Mathf.Clamp(baseLength, 0, maxLength * (isElevated ? elevatedRangeMult : 1f));

        bool ignoreWalls = isElevated || repairGun;
        var (centerLength, leftLength, rightLength) =
            !ignoreWalls
                ? RaycastLines(baseLength)
                : (baseLength, baseLength, baseLength);

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
        if (playerShooter == null)
            return;

        if (playerGunIndex < 0 || playerGunIndex >= playerShooter.gunList.Count)
            return;

        var gun = playerShooter.gunList[playerGunIndex];
        if (gun == null)
        {
            maxLength = 0f;
            return;
        }

        maxLength = gun.bulletLifeTime * gun.bulletSpeed;
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
