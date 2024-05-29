using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "StructureSO")]
public class StructureSO : ScriptableObject
{
    [Header("Tile Info")]
    public Sprite uiImage;
    public String tileName;
    [TextArea(3,10)]
    public String description;
    public float tileCost;
    public float maxHealth;
    public float health;

    [Header("Tile Type")]
    
    public RuleTileStructure ruleTileStructure;
    public StructureType structureType;
    public ActionType actionType;
    public Vector2Int range = new Vector2Int(5, 5);

}

public enum StructureType
{
    SelectTool,
    RepairTool,
    DestroyTool,
    OneByOneWall,
    TwoByTwoWall,
    None
}

public enum ActionType
{
    Select,
    Build,
    Destroy,
    Interact
}
