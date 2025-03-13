using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tiles and Structures/StructureSO")]
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
    public float blueprintBuildAmount = 100;
    public List<Vector3Int> cellPositions;

    [Header("Tile Type")]
    
    public RuleTileStructure ruleTileStructure;
    public RuleTileStructure blueprintTile;
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
    SolarLight,
    Gate,
    BlastGate,
    ThreeByThreePlatform,
    LaserTurret,
    GunTurret,
    LaserTurret2,
    RocketTurret,
    GunTurret2,
    RocketTurret2,
    QGate,
    Mine,
    RazorWire,
    MicroReactor,
    FloodLight,
    None
}

public enum ActionType
{
    Select,
    Build,
    Destroy,
    Interact
}
