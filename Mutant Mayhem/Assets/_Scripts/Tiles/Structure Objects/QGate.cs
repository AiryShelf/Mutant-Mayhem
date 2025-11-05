using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class QGate : MonoBehaviour, ITileObject, IPowerConsumer, ITileObjectExplodable
{
    [SerializeField] SpriteRenderer mySR;
    [SerializeField] List<Sprite> energyFieldDamageSprites;
    [SerializeField] List<AnimatedTile> gateSidesDamageSprites;
    [SerializeField] Collider2D gateCollider;
    [SerializeField] List<Light2D> gateLights;

    public string explosionPoolName;

    public void Explode()
    {
        if (!string.IsNullOrEmpty(explosionPoolName))
        {
            GameObject explosion = PoolManager.Instance.GetFromPool(explosionPoolName);
            explosion.transform.position = transform.position;
        }
    }

    List<Color> gateLightStartColors;
    float healthRatio;
    bool hasPower = true;

    public void PowerOn() 
    {
        hasPower = true;
        mySR.enabled = true;
        gateCollider.enabled = true;
        foreach (var light in gateLights)
            light.enabled = true;

        UpdateTile();
    }

    public void PowerOff() 
    {
        hasPower = false;
        mySR.enabled = false;
        gateCollider.enabled = false;
        foreach (var light in gateLights)
            light.enabled = false;

        UpdateTile();
    }

    public void UpdateHealthRatio(float healthRatio)
    {
        this.healthRatio = healthRatio;

        UpdateTile();
    }

    void UpdateTile()
    {
        Vector3Int rootPos = TileManager.Instance.WorldToGrid(transform.position);
        rootPos = TileManager.Instance.GridToRootPos(rootPos);

        int damageIndex = GetDamageIndex(healthRatio);

        if (hasPower)
        {
            mySR.enabled = true;
            mySR.sprite = energyFieldDamageSprites[damageIndex];
            TileManager.AnimatedTilemap.SetTile(rootPos, gateSidesDamageSprites[damageIndex]);

        }
        else
        {
            mySR.enabled = false;
            TileManager.AnimatedTilemap.SetTile(rootPos, gateSidesDamageSprites[damageIndex]);
        }

        // Dim Lights with damage
        for (int i = 0; i < gateLights.Count; i++)
        {
            if (gateLightStartColors == null)
            {
                gateLightStartColors = new List<Color>();
                foreach (var light in gateLights)
                {
                    gateLightStartColors.Add(light.color);
                }
            }

            gateLights[i].color = Color.Lerp(gateLightStartColors[i], Color.red, 1 - healthRatio);
        }
    }

    int GetDamageIndex(float healthRatio)
    {
        int damageIndex;
        int spriteCount = gateSidesDamageSprites.Count;

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
