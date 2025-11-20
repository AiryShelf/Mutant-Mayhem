using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RangeCircle : MonoBehaviour
{
    [SerializeField] int segments = 50;
    public float radius = 5f;
    public float radiusStart;
    [SerializeField] float dotSize = 0.2f;
    [SerializeField] bool startOn = false;
    public bool isRepairRange = false;
    LineRenderer lineRenderer;
    Player player;

    void Awake()
    {
        player = FindObjectOfType<Player>();
        radiusStart = radius;
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segments + 1;
        lineRenderer.loop = true;
        CreateCircle();
        EnableCircle(startOn);
    }

    public void EnableCircle(bool enable)
    {
        //Debug.Log($"RangeCircle: Setting Circle Enabled to {enable} for {gameObject.name}");
        lineRenderer.gameObject.SetActive(enable);
    }

    void CreateCircle()
    {
        //Debug.Log("RangeCircle: Creating Circle for " + gameObject.name);
        float angle = 360f / segments;
        Vector3[] points = new Vector3[segments + 1];

        for (int i = 0; i < segments + 1; i++)
        {
            float rad = Mathf.Deg2Rad * (i * angle);
            float x = Mathf.Cos(rad) * radius;
            float y = Mathf.Sin(rad) * radius;
            points[i] = new Vector3(x, y, 0);
        }

        lineRenderer.SetPositions(points);

        // Calculate the circumference of the circle
        float circumference = 2 * Mathf.PI * radius;

        // Adjust the main texture scale to repeat the texture along the length of the line
        float textureRepeatCount = circumference / dotSize;  // Number of times the texture should repeat
        lineRenderer.material.mainTextureScale = new Vector2(textureRepeatCount, 1);
    }

    void Update()
    {
        transform.rotation = Quaternion.identity;

        if (isRepairRange && player != null)
        {
            if (player.stats.playerShooter.isElevated)
                radius = radiusStart * 1.3f;
            else
                radius = radiusStart;
        }

        // Delete this after tuning the circle
        float angle = 360f / segments;
        Vector3[] points = new Vector3[segments + 1];

        for (int i = 0; i < segments + 1; i++)
        {
            float rad = Mathf.Deg2Rad * (i * angle);
            float x = Mathf.Cos(rad) * radius;
            float y = Mathf.Sin(rad) * radius;
            points[i] = new Vector3(x, y, 0);
        }

        lineRenderer.SetPositions(points);

        // Calculate the circumference of the circle
        float circumference = 2 * Mathf.PI * radius;

        // Adjust the main texture scale to repeat the texture along the length of the line
        float textureRepeatCount = circumference / dotSize;  // Number of times the texture should repeat
        lineRenderer.material.mainTextureScale = new Vector2(textureRepeatCount, 1);
        //lineRenderer.enabled = false;
    }

    
}
