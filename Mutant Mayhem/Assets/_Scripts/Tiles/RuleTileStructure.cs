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
    public AnimatedTile destroyedTile;
    
    [Header("Optional")]
    public AnimatedTile buildUiTile;

    public TileBase[] allowedNeighbors; // List of tiles this tile considers as neighbors

    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        // Allow the default behavior for exact matches
        if (base.RuleMatch(neighbor, tile))
            return true;

        // Custom behavior: Check if the tile is in the allowedNeighbors list
        if (neighbor == TilingRule.Neighbor.This)
        {
            foreach (var allowedTile in allowedNeighbors)
            {
                if (tile == allowedTile)
                    return true;
            }
        }

        return false;
    }

    /*
    public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData) 
    {
        base.GetTileData(location, tilemap, ref tileData);
        tileData.colliderType = Tile.ColliderType.Sprite;
        tileData.sprite = transparentSprite;
    }
    */
}
