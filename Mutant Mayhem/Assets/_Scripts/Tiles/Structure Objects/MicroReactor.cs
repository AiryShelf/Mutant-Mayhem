using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class MicroReactor : MonoBehaviour, ITileObject
{
    // Could add effect for insufficient power
    [SerializeField] List<AnimatedTile> damageTiles;
    [SerializeField] Light2D[] reactorLights;
    int[] lightStartIntensities;
    Color[] lightStartColors;

    public void UpdateHealthRatio(float healthRatio)
    {
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

        Vector3Int rootPos = TileManager.Instance.WorldToGrid(transform.position);
        rootPos = TileManager.Instance.GridToRootPos(rootPos);

        int damageIndex = Mathf.FloorToInt(damageTiles.Count * healthRatio);

        TileManager.AnimatedTilemap.SetTile(rootPos, damageTiles[damageIndex]);
    }
}
