using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "CustomRuleTile")]
public class RuleTileStructure : RuleTile
{
    [Header("Tile Settings")]
    [SerializeField] Tilemap structureTilemap;
    public StructureSO structureSO;
    public List<Vector3Int> cellPositions;
    public Sprite transparentSprite;
    public List<AnimatedTile> damagedTiles;
    public GameObject corpsePrefab;
    public GameObject hitEffectsPrefab;

    // Careful with this, it is a Unity method override.
    public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData) 
    {
        base.GetTileData(location, tilemap, ref tileData);
        tileData.colliderType = Tile.ColliderType.Sprite;
        tileData.sprite = transparentSprite;


    }
}
