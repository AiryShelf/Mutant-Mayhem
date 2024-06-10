using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "StructureSO")]
public class StructureSO : ScriptableObject
{
    [Header("Tile Info")]
    public Sprite uiImage;
    public string tileName;
    [TextArea(3,10)]
    public string description;
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
    SpotLight,
    None
}

public enum ActionType
{
    Select,
    Build,
    Destroy,
    Interact
}
