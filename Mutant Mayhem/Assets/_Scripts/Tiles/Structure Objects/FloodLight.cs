using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FloodLight : LightTile, ITileObject, IPowerConsumer
{
    [SerializeField] SpriteRenderer headSR;
    [SerializeField] List<Sprite> headOnDamageSprites;
    [SerializeField] List<Sprite> headOffDamageSprites;
    [SerializeField] GameObject lightObj;
    float healthRatio;
    int damageIndex = 0;
    bool isOn = true;
    bool hasPower = true;
    Vector3Int myGridPos = Vector3Int.zero;

    protected override void Start()
    {
        myGridPos = TileManager.Instance.WorldToGrid(transform.position);

        base.Start();
    }

    protected override void LightsOn()
    {
        base.LightsOn();
        
        isOn = true;
        UpdateTile();
    }

    protected override void LightsOff()
    {
        base.LightsOff();

        isOn = false;
        UpdateTile();
    }

    public void PowerOn()
    {
        hasPower = true;
        lightObj.SetActive(true);

        UpdateTile();
        //Debug.Log("Floodlight: PowerOn ran");
    }

    public void PowerOff()
    {
        hasPower = false;
        lightObj.SetActive(false);

        UpdateTile();
        //Debug.Log("Floodlight: PowerOff ran");

    }

    public void UpdateHealthRatio(float healthRatio)
    {
        this.healthRatio = healthRatio;
        damageIndex = GetDamageIndex(healthRatio);
        
        UpdateTile();
    }

    void UpdateTile()
    {
        //Debug.Log("Damage Index: " + damageIndex);
        if (isOn && hasPower)
        {
            headSR.sprite = headOnDamageSprites[damageIndex];
            //TileManager.AnimatedTilemap.SetTile(myGridPos, lightsOn[damageIndex]);

        }
        else
        {
            headSR.sprite = headOffDamageSprites[damageIndex];
            //TileManager.AnimatedTilemap.SetTile(myGridPos, lightsOff[damageIndex]);
        }
    }

    int GetDamageIndex(float healthRatio)
    {
        int damageIndex;
        int spriteCount = headOnDamageSprites.Count;

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
