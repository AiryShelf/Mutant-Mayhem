using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class QGate : MonoBehaviour, ITileObject, IPowerConsumer
{
    [SerializeField] SpriteRenderer mySR;
    [SerializeField] List<Sprite> energyFieldDamageSprites;
    [SerializeField] List<AnimatedTile> gateSidesDamageSprites;
    [SerializeField] Collider2D gateCollider;
    [SerializeField] List<Light2D> gateLights;
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

        int damageIndex = Mathf.FloorToInt(energyFieldDamageSprites.Count * healthRatio);

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
}
