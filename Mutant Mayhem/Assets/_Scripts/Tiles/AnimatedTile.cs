using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class AnimatedTileTest : TileBase {
    public Sprite[] animatedSprites;
    public float animationSpeed = 1f;
    public float animationStartTime;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        if (animatedSprites != null && animatedSprites.Length > 0)
        {
            float t = Time.time * animationSpeed + this.animationStartTime;
            int index = Mathf.FloorToInt(t);
            index = index % this.animatedSprites.Length;
            
            tileData.sprite = this.animatedSprites[index];
        }
    }
}
