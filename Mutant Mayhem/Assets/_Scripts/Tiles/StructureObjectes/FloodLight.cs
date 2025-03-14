using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FloodLight : Light, ITileObject, IPowerConsumer
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
    }

    public void PowerOff()
    {
        hasPower = false;
        lightObj.SetActive(false);
    }

    public void UpdateHealthRatio(float healthRatio)
    {
        this.healthRatio = healthRatio;
        
        UpdateTile();
    }

    void UpdateTile()
    {
        damageIndex = Mathf.FloorToInt(headOnDamageSprites.Count * healthRatio);

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
}
