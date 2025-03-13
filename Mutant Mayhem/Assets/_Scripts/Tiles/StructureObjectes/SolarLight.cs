using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolarLight : Light
{
    [SerializeField] SpriteRenderer mySR;
    [SerializeField] Sprite onSprite;
    [SerializeField] Sprite offSprite;

    protected override void TurnOn()
    {
        base.TurnOn();
        mySR.sprite = onSprite;
    }

    protected override void TurnOff()
    {
        base.TurnOff();
        mySR.sprite = offSprite;
    }
}
