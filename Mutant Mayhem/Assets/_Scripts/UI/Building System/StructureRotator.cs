using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.Tilemaps;


public static class StructureRotator
{
    public static void RotateTileAt(Tilemap tilemap, Vector3Int gridPos, int rotation)
    {
        //Debug.Log($"Applying rotation {rotation} at position {gridPos}");
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

        // Copy properties
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

    public static int GetRotationFromMatrix(Matrix4x4 matrix)
    {
        //Vector3 lossyScale = matrix.lossyScale;
        float angle = Mathf.Atan2(matrix.m01, matrix.m00) * Mathf.Rad2Deg;

        if (Mathf.Approximately(angle, 0)) return 0;
        if (Mathf.Approximately(angle, 90)) return 90;
        if (Mathf.Approximately(angle, 180) || Mathf.Approximately(angle, -180)) return 180;
        if (Mathf.Approximately(angle, -90)) return 270;

        return 0;
    }

    // This ended up being pointless thankfully, it was a headache!
    public static void RepositionGameObject(Tilemap tilemap, GameObject tileGameObject, 
                                            Vector3Int gridPos, Vector2Int bounds, int rotation)
    {
        Vector3 newPos = tilemap.CellToWorld(gridPos);

        /*
        switch (rotation)
        {
            case 90:
                offsetX = (bounds.y - 1) / 2f;
                offsetY = -(bounds.x - 1) / 2f;
                break;
            case 180:
                offsetX = -(bounds.x - 1) / 2f;
                offsetY = -(bounds.y - 1) / 2f;
                break;
            case 270:
                offsetX = -(bounds.y - 1) / 2f;
                offsetY = (bounds.x - 1) / 2f;
                break;
            default: // 0 degrees
                offsetX = (bounds.x - 1) / 2f;
                offsetY = (bounds.y - 1) / 2f;
                break;
        }
        */

        
        switch (rotation)
        {
            case 90:
                newPos = new Vector3(newPos.x - 0.5f + bounds.x / 2,
                                     newPos.y + 0.5f + bounds.y / 2, newPos.z);
                break;
            case 180:
                newPos = new Vector3(newPos.x + 0.5f + bounds.x / 2,
                                     newPos.y - 0.5f + bounds.y / 2, newPos.z);
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

        //newPos = new Vector3(newPos.x);
        

        //newPos = new Vector3(newPos.x + offsetX, newPos.y + offsetY, newPos.z);
        
        tileGameObject.transform.position = newPos;
    }

    public static List<Vector3Int> RotateCellPositions(List<Vector3Int> origPositions, int rotation)
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

    public static List<Vector3Int> RotateCellPositionsBack(List<Vector3Int> cellPositions, int rotation)
    {
        List<Vector3Int> originalPositions = new List<Vector3Int>();

        foreach (var pos in cellPositions)
        {
            Vector3Int originalPos;
            switch (rotation)
            {
                case 90:
                    originalPos = new Vector3Int(pos.y, -pos.x, pos.z);
                    break;
                case 180:
                    originalPos = new Vector3Int(-pos.x, -pos.y, pos.z);
                    break;
                case 270:
                    originalPos = new Vector3Int(-pos.y, pos.x, pos.z);
                    break;
                default:
                    originalPos = pos; // No rotation
                    break;
            }
            originalPositions.Add(originalPos);
        }

        return originalPositions;
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
