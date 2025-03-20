using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class QGate : MonoBehaviour, ITileObject, IPowerConsumer
{
    [SerializeField] SpriteRenderer mySR;
    [SerializeField] List<Sprite> energyFieldDamageSprites;
    [SerializeField] Collider2D gateCollider;
    [SerializeField] List<Light2D> gateLights;

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
        int damageIndex = Mathf.FloorToInt(energyFieldDamageSprites.Count * healthRatio);

        //Debug.Log("Damage Index: " + damageIndex);
        if (hasPower)
        {
            mySR.sprite = energyFieldDamageSprites[damageIndex];
            //TileManager.AnimatedTilemap.SetTile(myGridPos, lightsOn[damageIndex]);

        }
        else
        {
            //TileManager.AnimatedTilemap.SetTile(myGridPos, lightsOff[damageIndex]);
        }
    }
}
