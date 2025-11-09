using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class MicroReactor : MonoBehaviour, ITileObject, ITileObjectExplodable
{
    // Could add effect for insufficient power
    [SerializeField] List<AnimatedTile> damageTiles;
    [SerializeField] Light2D[] reactorLights;

    public string explosionPoolName;

    public void Explode()
    {
        if (!string.IsNullOrEmpty(explosionPoolName))
        {
            GameObject explosion = PoolManager.Instance.GetFromPool(explosionPoolName);
            explosion.transform.position = transform.position;
            Explosion explosionComp = explosion.GetComponent<Explosion>();
            if (explosionComp != null)
                explosionComp.Explode();
        }
    }
    int[] lightStartIntensities;
    Color[] lightStartColors;

    public void UpdateHealthRatio(float healthRatio)
    {
        Debug.Log("MicroReactor: Updating health ratio to " + healthRatio);

        // Dim lights for damage
        for (int i = 0; i < reactorLights.Length; i++)
        {
            if (lightStartIntensities == null)
            {
                lightStartIntensities = new int[reactorLights.Length];
                for (int j = 0; j < reactorLights.Length; j++)
                {
                    lightStartIntensities[j] = Mathf.FloorToInt(reactorLights[j].intensity);
                }
            }

            if (lightStartColors == null)
            {
                lightStartColors = new Color[reactorLights.Length];
                for (int j = 0; j < reactorLights.Length; j++)
                {
                    lightStartColors[j] = reactorLights[j].color;
                }
            }

            reactorLights[i].intensity = lightStartIntensities[i] * healthRatio;
            reactorLights[i].color = Color.Lerp(lightStartColors[i], Color.red, 1 - healthRatio);
        }

        int damageIndex = GetDamageIndex(healthRatio);
        
        // Update tile sprite for damage
        Vector3Int rootPos = TileManager.Instance.WorldToGrid(transform.position);
        rootPos = TileManager.Instance.GridToRootPos(rootPos);
        TileManager.AnimatedTilemap.SetTile(rootPos, damageTiles[damageIndex]);
    }

    int GetDamageIndex(float healthRatio)
    {
        int damageIndex;
        int spriteCount = damageTiles.Count;

        // Reserve index 0 for full health
        if (healthRatio >= 1f)
        {
            damageIndex = 0;
        }
        else
        {
            // Divide remaining indices (1 to spriteCount-1) across the 0–99% damage range
            float normalized = 1f - healthRatio;
            damageIndex = 1 + Mathf.FloorToInt(normalized * (spriteCount - 1));
            damageIndex = Mathf.Clamp(damageIndex, 1, spriteCount - 1);
        }

        return damageIndex;
    }
}