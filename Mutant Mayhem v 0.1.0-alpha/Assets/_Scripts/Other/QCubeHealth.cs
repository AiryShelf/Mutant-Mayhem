using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QCubeHealth : Health
{
    [SerializeField] SpriteRenderer mySR;
    public List<Sprite> damageSprites;

    void FixedUpdate()
    {
        UpdateDamageSprite();        
    }

    public override void ModifyHealth(float value, GameObject other)
    {
        base.ModifyHealth(value, other);
        UpdateDamageSprite();
    }

    void UpdateDamageSprite()
    {
        float healthRatio = 1 - (health / maxHealth);
        int sprIndex = Mathf.FloorToInt(healthRatio * damageSprites.Count);
        sprIndex = Mathf.Clamp(sprIndex, 0, damageSprites.Count - 1);

        mySR.sprite = damageSprites[sprIndex];
    }

    public override void Die()
    {
        QCubeController.IsCubeDestroyed = true;
    } 
}