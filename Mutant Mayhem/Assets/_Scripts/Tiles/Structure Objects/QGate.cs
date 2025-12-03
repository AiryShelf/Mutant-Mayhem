using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class QGate : MonoBehaviour, ITileObject, IPowerConsumer, ITileObjectExplodable
{
    [SerializeField] SpriteRenderer fieldSr;
    [SerializeField] SpriteRenderer waveSr;
    [SerializeField] List<Sprite> energyFieldDamageSprites;
    [SerializeField] List<Sprite> energyFieldWaveSprites;
    [SerializeField] float srAlphaMin = 0.4f;
    [SerializeField] float srAlphaMax = 0.8f;
    [SerializeField] List<AnimatedTile> gateSidesDamageTiles;
    [SerializeField] Collider2D gateCollider;
    [SerializeField] List<Light2D> gateLights;

    public string explosionPoolName;

    public void Explode()
    {
        if (!string.IsNullOrEmpty(explosionPoolName))
        {
            GameObject explosion = PoolManager.Instance.GetFromPool(explosionPoolName);
            Vector3Int rootPos = TileManager.Instance.WorldToGrid(transform.position);
            explosion.transform.position = TileManager.Instance.TileCellsCenterToWorld(rootPos);
        }
    }
    
    float healthRatio = 1f;
    bool hasPower = true;

    public void PowerOn() 
    {
        hasPower = true;
        fieldSr.enabled = true;
        waveSr.enabled = true;
        gateCollider.enabled = true;
        foreach (var light in gateLights)
            light.enabled = true;

        UpdateTile();
    }

    public void PowerOff() 
    {
        hasPower = false;
        fieldSr.enabled = false;
        waveSr.enabled = false;
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
        Debug.Log("QGate UpdateTile called with healthRatio: " + healthRatio + " and hasPower: " + hasPower + " at position: " + transform.position);
        Vector3Int rootPos = TileManager.Instance.WorldToGrid(transform.position);
        rootPos = TileManager.Instance.GridToRootPos(rootPos);

        int damageIndex = GetDamageIndex(healthRatio);

        if (hasPower)
        {
            fieldSr.enabled = true;
            fieldSr.sprite = energyFieldDamageSprites[damageIndex];
            waveSr.sprite = energyFieldWaveSprites[damageIndex];
            TileManager.AnimatedTilemap.SetTile(rootPos, gateSidesDamageTiles[damageIndex]);
        }
        else
        {
            fieldSr.enabled = false;
            TileManager.AnimatedTilemap.SetTile(rootPos, gateSidesDamageTiles[damageIndex]);
        }

        // Reduce field sr alpha with damage
        Color srColor = fieldSr.color;
        float ratio = Mathf.InverseLerp(srAlphaMin, srAlphaMax, healthRatio);
        Debug.Log("QGate fieldSr alpha ratio: " + ratio);
        srColor.a = ratio;
        fieldSr.color = srColor;
    }

    int GetDamageIndex(float healthRatio)
    {
        int damageIndex;
        int spriteCount = gateSidesDamageTiles.Count;

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
