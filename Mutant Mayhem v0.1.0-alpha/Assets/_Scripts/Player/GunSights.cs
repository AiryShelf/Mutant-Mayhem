using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSights : MonoBehaviour
{
    public LineRenderer lineRenderer;
    [SerializeField] int playerGunIndex;
    [SerializeField] float maxLength;

    Player player;

    void Awake()
    {
        player = FindObjectOfType<Player>();
    }

    void Update()
    {
        
        Vector3 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        target.z = 0;

        float dist = (target - transform.position).magnitude;
        dist = Mathf.Clamp(dist, 0, maxLength);

        /*
        // Set positions
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.right * dir.magnitude);
        */



        // Set the start position of the LineRenderer at the muzzle position
        lineRenderer.SetPosition(0, transform.position);

        // Set the end position of the LineRenderer along the muzzle's direction
        Vector3 beamEndPosition = transform.position + transform.right * dist;
        lineRenderer.SetPosition(1, beamEndPosition);


        /*
        // Position the particle system along the beam
        electricityEffect.transform.position = Vector3.Lerp(qCube.position, mousePosition, 0.5f);

        // Adjust particle emission direction.
        var emission = electricityEffect.emission;
        emission.rateOverTime = 50f;

        // Adjust particle shape module to align with the beam.
        var shape = electricityEffect.shape;
        //shape.position = Vector3.Lerp(qCube.position, mousePosition, 0.5f);
        shape.rotation = new Vector3(0, 0, Vector3.Angle(mousePosition - qCube.position, Vector3.right));
        */
    }

    public void RefreshSights()
    {
        maxLength = player.playerShooter.gunList[playerGunIndex].bulletLifeTime *
                    player.playerShooter.gunList[playerGunIndex].bulletSpeed;
    } 
}
