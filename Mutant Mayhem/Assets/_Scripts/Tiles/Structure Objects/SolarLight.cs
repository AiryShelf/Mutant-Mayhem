using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolarLight : LightTile
{
    [SerializeField] SpriteRenderer mySR;
    [SerializeField] Sprite onSprite;
    [SerializeField] Sprite offSprite;

    protected override void LightsOn()
    {
        base.LightsOn();
        mySR.sprite = onSprite;
    }

    protected override void LightsOff()
    {
        base.LightsOff();
        mySR.sprite = offSprite;
    }
}
