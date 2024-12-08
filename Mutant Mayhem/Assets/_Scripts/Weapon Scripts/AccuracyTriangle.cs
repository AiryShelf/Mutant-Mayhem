using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccuracyTriangle : MonoBehaviour
{
    public Transform weaponMuzzle; // The origin of the triangle
    public float range = 10f; // Maximum range of the triangle
    public float spreadAngle = 15f; // Angle of the accuracy triangle
    public Material triangleMaterial;

    private MeshFilter meshFilter;

    void Start()
    {
        // Add a MeshFilter and Renderer
        meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.material = triangleMaterial;

        UpdateTriangle();
    }

    void UpdateTriangle()
    {
        Mesh mesh = new Mesh();

        // Define the triangle vertices
        Vector3[] vertices = new Vector3[3];
        vertices[0] = weaponMuzzle.position; // Apex
        float angleRad = spreadAngle * Mathf.Deg2Rad;
        vertices[1] = weaponMuzzle.position + new Vector3(-spreadAngle / 2, range, 0); // Left base
        vertices[2] = weaponMuzzle.position + new Vector3(spreadAngle / 2, range, 0); // Right base
        //vertices[1] = weaponMuzzle.position + (Quaternion.Euler(0, -spreadAngle / 2, 0) * weaponMuzzle.right) * range; // Left base
        //vertices[2] = weaponMuzzle.position + (Quaternion.Euler(0, spreadAngle / 2, 0) * weaponMuzzle.right) * range; // Right base

        // Assign vertices and triangles
        int[] triangles = new int[3] { 0, 1, 2 };

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        // Assign mesh to the filter
        meshFilter.mesh = mesh;
    }
}
