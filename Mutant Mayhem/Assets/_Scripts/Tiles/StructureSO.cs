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
    public List<Vector3Int> cellPositions;

    [Header("Tile Type")]
    
    public RuleTileStructure ruleTileStructure;
    public StructureType structureType;
    public bool isTurret;
    public ActionType actionType;
    public Vector2Int actionRange = new Vector2Int(5, 5);
}

public enum StructureType
{
    SelectTool,
    RepairTool,
    DestroyTool,
    OneByOneWall,
    OneByFourWall,
    OneByOneCorner,
    TwoByTwoWall,
    TwoByEightWall,
    TwoByTwoCorner,
    FloodLight,
    Door,
    BlastDoor,
    ThreeByThreePlatform,
    LaserTurret,
    GunTurret,
    LaserTurret2,
    RocketTurret,
    GunTurret2,
    RocketTurret2,
    None
}

public enum ActionType
{
    Select,
    Build,
    Destroy,
    Interact
}
