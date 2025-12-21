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
    public bool canBeRepaired = true;
    public List<Vector3Int> cellPositions;

    [Header("Tile Type")]
    public RuleTileStructure ruleTileStructure;
    public RuleTileStructure blueprintTile;
    public StructureType structureType;
    public bool isTurret;
    public ActionType actionType;
    public Vector2Int actionRange = new Vector2Int(5, 5);

    [Header("Optional")]
    public int powerGenerated;
    public int powerNeighborBonus;
    public int powerCost = 0;
    public int supplyCost;
    public List<StructureSO> structuresToUnlock;
    public bool canBuildOnlyOne = false;
}

public enum StructureType
{
    SelectTool,
    RepairTool,
    DestroyTool,
    OneByOneWall,
    OneByFourWall_deprecated,
    OneByOneCorner,
    TwoByTwoWall_deprecated,
    TwoByEightWall_deprecated,
    TwoByTwoCorner_deprecated,
    SolarLight,
    Gate,
    BlastGate,
    ShootingPlatform,
    LaserTurret,
    GunTurret,
    LaserTurret2_deprecated,
    RocketTurret,
    GunTurret2_deprecated,
    RocketTurret2_deprecated,
    QGate,
    Mine,
    RazorWire,
    MicroReactor,
    FloodLight,
    EngineeringBay,
    PhotonicsBay,
    BallisticsBay,
    ExplosivesBay,
    DroneHangar,
    SolarPanels,
    RepairBay,
    QuantumCube,
    SupplyDepot,
    SpinningBlades,
    None
}

public enum ActionType
{
    Select,
    Build,
    Destroy,
    Interact
}
