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
    [SerializeField] Light2D reactorGlowLight;
    [SerializeField] Light2D[] circleLights;
    [SerializeField] SpriteRenderer[] circleSprites;
    [SerializeField] float lightIntensityMultiplier = 4f; // How much brighter lights get toward 0 health
    [SerializeField] float glowLightRadiusMultiplier = 3f;
    

    public string explosionPoolName;

    public void Explode()
    {
        if (!string.IsNullOrEmpty(explosionPoolName))
        {
            GameObject explosion = PoolManager.Instance.GetFromPool(explosionPoolName);
            Vector3Int rootPos = TileManager.Instance.WorldToGrid(transform.position);
            rootPos = TileManager.Instance.GridToRootPos(rootPos);
            explosion.transform.position = TileManager.Instance.TileCellsCenterToWorld(rootPos);
        }
    }
    Color[] lightStartColors;
    Color[] glowStartColor;
    float[] lightStartIntensities;
    float[] glowStartIntensity;
    float[] glowLightStartRadius;
    Color[] circleStartColors;

    public void UpdateHealthRatio(float healthRatio)
    {
        //Debug.Log("MicroReactor: Updating health ratio to " + healthRatio);
        // Store start colors and intensities
        if (lightStartColors == null)
        {
            lightStartColors = new Color[circleLights.Length];
            for (int j = 0; j < circleLights.Length; j++)
            {
                lightStartColors[j] = circleLights[j].color;
            }
        }
        if (glowStartColor == null)
        {
            glowStartColor = new Color[1];
            glowStartColor[0] = reactorGlowLight.color;
        }
        if (lightStartIntensities == null)  
        {
            lightStartIntensities = new float[circleLights.Length];
            for (int j = 0; j < circleLights.Length; j++)
            {
                lightStartIntensities[j] = circleLights[j].intensity;
            }
        }
        if (glowStartIntensity == null)
        {
            glowStartIntensity = new float[1];
            glowStartIntensity[0] = reactorGlowLight.intensity;
        }
        if (glowLightStartRadius == null)
        {
            glowLightStartRadius = new float[1];
            glowLightStartRadius[0] = reactorGlowLight.pointLightOuterRadius;
        }
        if (circleStartColors == null)
        {
            circleStartColors = new Color[circleSprites.Length];
            for (int i = 0; i < circleSprites.Length; i++)
            {
                circleStartColors[i] = circleSprites[i].color;
            }
        }

        // Change lights for damage
        for (int i = 0; i < circleLights.Length; i++)
        {
            circleLights[i].color = Color.Lerp(lightStartColors[i], Color.red, 1 - healthRatio);
            // Make lights brighter when damaged
            circleLights[i].intensity = Mathf.Lerp(lightStartIntensities[i], lightStartIntensities[i] * lightIntensityMultiplier, 1 - healthRatio);
        }

        // Change glow light for damage, make light larger
        reactorGlowLight.color = Color.Lerp(glowStartColor[0], Color.red, 1 - healthRatio);
        reactorGlowLight.intensity = Mathf.Lerp(glowStartIntensity[0], glowStartIntensity[0] * lightIntensityMultiplier, 1 - healthRatio);
        reactorGlowLight.pointLightOuterRadius = Mathf.Lerp(glowLightStartRadius[0], glowLightStartRadius[0] * glowLightRadiusMultiplier, 1 - healthRatio);

        // Change circle sprites for damage
        for (int i = 0; i < circleSprites.Length; i++)
        {
            circleSprites[i].color = Color.Lerp(circleStartColors[i], Color.red, 1 - healthRatio);
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