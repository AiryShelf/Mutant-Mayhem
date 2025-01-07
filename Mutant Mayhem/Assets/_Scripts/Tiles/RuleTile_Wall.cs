using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Tiles and Structures/RuleTileWall")]
public class RuleTile_Wall : RuleTileStructure
{
    public AnimatedTile[] allowedNeighbors;

    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        // Allow the default behavior
        if (base.RuleMatch(neighbor, tile))
            return true;

        // Custom behavior: Check if the tile is in the allowedNeighbors list
        if (neighbor == TilingRule.Neighbor.This && tile is AnimatedTile animatedTile)
        {
            // Check if the tile exists in the allowedNeighbors list
            foreach (var allowedTile in allowedNeighbors)
            {
                if (animatedTile == allowedTile)
                    return true;
            }
        }

        return false;
    }
}
