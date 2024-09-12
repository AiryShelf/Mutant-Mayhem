using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Tiles and Structures/RuleTileStructure")]
public class RuleTileStructure : RuleTile
{
    [Header("Tile Settings")]
    public StructureSO structureSO;
    public List<AnimatedTile> damagedTiles;
    public GameObject corpsePrefab;
    public GameObject hitEffectsPrefab;
    [Header("Optional")]
    public AnimatedTile buildUiTile;

/*
    public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData) 
    {
        base.GetTileData(location, tilemap, ref tileData);
        tileData.colliderType = Tile.ColliderType.Sprite;
        tileData.sprite = transparentSprite;
    }
*/
}
