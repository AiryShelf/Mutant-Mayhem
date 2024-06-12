using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.Tilemaps;


public static class StructureRotator
{
    public static void RotateTile(Tilemap tilemap, Vector3Int gridPos, int rotation)
    {
        Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, 
                           Quaternion.Euler(0, 0, rotation), Vector3.one);
        tilemap.SetTransformMatrix(gridPos, matrix);
    }

    // Method to reset the tile's transform matrix to its default state
    public static void ResetTileMatrix(Tilemap tilemap, Vector3Int gridPos)
    {
        // Reset the transformation matrix to identity (default state)
        tilemap.SetTransformMatrix(gridPos, Matrix4x4.identity);
    }

    public static StructureSO RotateStructure(StructureSO source, int rotation)
    {
        // Create a new instance of StructureSO
        StructureSO rotatedStructure = ScriptableObject.CreateInstance<StructureSO>();

        // Copy the basic properties
        rotatedStructure.uiImage = source.uiImage;
        rotatedStructure.tileName = source.tileName;
        rotatedStructure.description = source.description;
        rotatedStructure.tileCost = source.tileCost;
        rotatedStructure.maxHealth = source.maxHealth;
        rotatedStructure.health = source.health;
        rotatedStructure.ruleTileStructure = source.ruleTileStructure;
        rotatedStructure.structureType = source.structureType;
        rotatedStructure.actionType = source.actionType;
        rotatedStructure.actionRange = source.actionRange;

        // Rotate cell positions
        rotatedStructure.cellPositions = RotateCellPositions(source.cellPositions, rotation);

        return rotatedStructure;
    }

    public static void RepositionGameObject(Tilemap tilemap, GameObject tileGameObject, 
                                            Vector3Int gridPos, Vector2Int bounds, int rotation)
    {       
        Vector3 newPos = tilemap.CellToWorld(gridPos);

        switch (rotation)
        {
            case 90:
                newPos = new Vector3(newPos.x - 0.5f + bounds.x / 2,
                                     newPos.y + 0.5f + bounds.y / 2, newPos.z);
                break;
            case 180:
                newPos = new Vector3(newPos.x - 0.5f + bounds.x / 2,
                                     newPos.y + 0.5f + bounds.y / 2, newPos.z);
                break;
            case 270:
                newPos = new Vector3(newPos.x - 0.5f + bounds.x / 2,
                                     newPos.y + 0.5f + bounds.y / 2, newPos.z);
                break;
            default: // 0 degrees
                newPos = new Vector3(newPos.x + 0.5f + bounds.x / 2,
                                     newPos.y - 0.5f + bounds.y / 2, newPos.z);
                break;
        }
        
        tileGameObject.transform.position = newPos;
    }

    private static List<Vector3Int> RotateCellPositions(List<Vector3Int> origPositions, int rotation)
    {
        List<Vector3Int> rotatedPositions = new List<Vector3Int>();

        foreach (var pos in origPositions)
        {
            Vector3Int rotatedPos;
            switch (rotation)
            {
                case 90:
                    rotatedPos = new Vector3Int(-pos.y, pos.x, pos.z);
                    break;
                case 180:
                    rotatedPos = new Vector3Int(-pos.x, -pos.y, pos.z);
                    break;
                case 270:
                    rotatedPos = new Vector3Int(pos.y, -pos.x, pos.z);
                    break;
                default:
                    rotatedPos = pos; // No rotation
                    break;
            }
            rotatedPositions.Add(rotatedPos);
        }

        return rotatedPositions;
    }

    public static Vector2Int CalculateBoundingBox(List<Vector3Int> cellPositions)
    {
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        foreach (var pos in cellPositions)
        {
            if (pos.x < minX) minX = pos.x;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y < minY) minY = pos.y;
            if (pos.y > maxY) maxY = pos.y;
        }

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;

        return new Vector2Int(width, height);
    }
}
