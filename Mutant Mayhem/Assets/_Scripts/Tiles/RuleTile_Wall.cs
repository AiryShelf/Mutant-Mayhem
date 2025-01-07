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
        if (neighbor == TilingRule.Neighbor.This)
        {
            foreach (var allowedTile in allowedNeighbors)
            {
                if (tile == allowedTile)
                    return true;

                // Additional check: Match if the tile is a RuleTileStructure and matches an allowed neighbor
                if (allowedTile is AnimatedTile && tile is AnimatedTile)
                {
                    // Ensure both tiles share the same type or identifier (e.g., name, type, etc.)
                    if (allowedTile.GetType() == tile.GetType())
                        return true;
                }
            }
        }

        return false;
    }
}
