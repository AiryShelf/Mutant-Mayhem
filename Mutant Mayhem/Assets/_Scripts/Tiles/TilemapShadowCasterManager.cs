using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(CompositeShadowCaster2D))]
public class TilemapShadowCasterManager : MonoBehaviour
{
    public Tilemap tilemap;

    private CompositeShadowCaster2D compositeShadowCaster2D;
    private CompositeCollider2D compositeCollider2D;

    void Start()
    {
        compositeShadowCaster2D = GetComponent<CompositeShadowCaster2D>();
        compositeCollider2D = tilemap.GetComponent<CompositeCollider2D>();
        UpdateShadowCasters();
    }

    public void UpdateShadowCasters()
    {
        // Clear existing shadow casters
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Create new shadow casters based on composite collider paths
        for (int i = 0; i < compositeCollider2D.pathCount; i++)
        {
            Vector2[] path = new Vector2[compositeCollider2D.GetPathPointCount(i)];
            compositeCollider2D.GetPath(i, path);

            GameObject shadowCaster = new GameObject("ShadowCaster");
            shadowCaster.transform.SetParent(transform);
            ShadowCaster2D shadowCasterComponent = shadowCaster.AddComponent<ShadowCaster2D>();

            // Set the shape path for the shadow caster
            //compositeShadowCaster2D. = path;

            // THIS MIGHT NOT WORK
        }
    }

    // Call this method whenever a tile is placed or removed
    public void OnTilemapChanged()
    {
        UpdateShadowCasters();
    }
}