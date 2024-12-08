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

    [Header("If not to mouse:")]
    [SerializeField] float defaultDist = 5;

    Player player;
    PlayerShooter playerShooter;
    float length;
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
        if (distToMouse)
        {
            Vector3 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            target.z = 0;
            length = (target - transform.position).magnitude;
        }
        else
            length = defaultDist;

        length = Mathf.Clamp(length, 0, maxLength);

        RefreshSettings();
        UpdateSights();
    }

    public void RefreshSettings()
    {
        maxLength = playerShooter.gunList[playerGunIndex].bulletLifeTime *
                    playerShooter.gunList[playerGunIndex].bulletSpeed;

        //accuracy = player.stats.accuracy + playerShooter.currentGunSO.accuracy;
    }

    public void SetAccuracy(float accuracy)
    {
        this.accuracy = accuracy;
    }

    void UpdateSights()
    {
        lineRendererCenter.SetPosition(0, transform.position);
        Vector2 beamEndPosition = transform.position + transform.right * length;
        lineRendererCenter.SetPosition(1, beamEndPosition);
        
        if (lineRendererLeftEdge == null && lineRendererRightEdge == null)
            return;
        
        Vector2 leftDir = GameTools.RotateVector2(transform.right, accuracy);
        Vector2 rightDir = GameTools.RotateVector2(transform.right, -accuracy);
        float edgeDist = defaultDist * edgeLengthFactor;

        lineRendererLeftEdge.SetPosition(0, (Vector2)transform.position);
        lineRendererLeftEdge.SetPosition(1, (Vector2)transform.position + leftDir * edgeDist);
        
        lineRendererRightEdge.SetPosition(0, (Vector2)transform.position);
        lineRendererRightEdge.SetPosition(1, (Vector2)transform.position + rightDir * edgeDist);
    }
}
