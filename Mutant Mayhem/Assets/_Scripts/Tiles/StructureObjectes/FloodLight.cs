using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FloodLight : Light, ITileObject
{
    [SerializeField] List<AnimatedTile> lightsOn;
    [SerializeField] List<AnimatedTile> lightsOff;
    float healthRatio;
    int damageIndex = 0;
    bool isOn = true;
    Vector3Int myGridPos = Vector3Int.zero;

    protected override void Start()
    {
        myGridPos = TileManager.Instance.WorldToGrid(transform.position);

        base.Start();
    }

    protected override void TurnOn()
    {
        foreach (var light in lights)
            light.enabled = true;
        
        isOn = true;
        UpdateTile();
    }

    protected override void TurnOff()
    {
        foreach (var light in lights)
            light.enabled = false;

        isOn = false;
        UpdateTile();
    }

    public void UpdateHealthRatio(float healthRatio)
    {
        this.healthRatio = healthRatio;
        
        UpdateTile();
    }

    void UpdateTile()
    {
        damageIndex = Mathf.FloorToInt(lightsOn.Count * healthRatio);

        //Debug.Log("Damage Index: " + damageIndex);
        if (isOn)
        {
            TileManager.AnimatedTilemap.SetTile(myGridPos, lightsOn[damageIndex]);

            if (TileManager.Instance.shadowCaster2DTileMap != null && TileManager.Instance.shadowCaster2DTileMap.gameObject != null)
            {
                TileManager.Instance.shadowCaster2DTileMap.Generate();
            }
        }
        else
        {
            TileManager.AnimatedTilemap.SetTile(myGridPos, lightsOff[damageIndex]);

            if (TileManager.Instance.shadowCaster2DTileMap != null && TileManager.Instance.shadowCaster2DTileMap.gameObject != null)
            {
                TileManager.Instance.shadowCaster2DTileMap.Generate();
            }
        }
    }
}
