using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TileStats
{   
    public RuleTileStructure ruleTileStructure;
    public float maxHealth;
    public float health;
    public Vector3Int rootGridPos;
    public bool isBlueprint = false;
    public float blueprintProgress = 0;
}

public class TileManager : MonoBehaviour
{
    public static TileManager Instance { get; private set; }

    // This stores the TileStats/reference for each tile in 
    // the Structures Tilemap (on "Structures" layer)
    private static Dictionary<Vector3Int, TileStats> _TileStatsDict = 
                                            new Dictionary<Vector3Int, TileStats>();
    private static List<Vector3Int> _StructurePositions = new List<Vector3Int>();
    public Player player;
    public static Tilemap StructureTilemap;
    public Grid StructureGrid;
    [SerializeField] LayerMask structureInteractLayerMask;

    public static Tilemap BlueprintTilemap;
    public static Tilemap AnimatedTilemap;
    [SerializeField] Tilemap destroyedTilemap;
    [SerializeField] Tilemap damageTilemap;
    [SerializeField] List<ParticleSystem> particlesToClear;
    [SerializeField] string textFlyWorldPlayerPoolName = "TextFlyWorld_Health_Player";
    [SerializeField] Color textFlyHealthLossColor;
    [SerializeField] Color textFlyHealthGainColor;
    [SerializeField] float textFlyAlphaMax;
    [SerializeField] Color buildBlueprintTextColor = Color.cyan;
    [SerializeField] string buildingBuiltSmallExplosionPoolName;
    [SerializeField] string buildingBuiltMediumExplosionPoolName;
    [SerializeField] string buildingBuiltLargeExplosionPoolName;
    [SerializeField] string damagingBuildingExplosionPoolName;
    [SerializeField] string aestheticBuildingExplosionPoolName;

    public int numberOfTilesHit;
    public int numberofTilesMissed;

    BuildingSystem buildingSystem;
    TurretManager turretManager;

    // Buffer for non-alloc physics queries
    private readonly Collider2D[] _panelColsBuffer = new Collider2D[64];
    // Buffer for non-alloc point queries at tile centers
    private readonly Collider2D[] _tileColsBuffer = new Collider2D[16];

    // Stores a persistent random rotation for damaged wall sprites (per root cell)
    private readonly Dictionary<Vector3Int, float> _wallDamageRotation = new Dictionary<Vector3Int, float>();

    //[SerializeField] GameObject debugDotPrefab;

    // For debugging
    Vector2 boxSize;
    Vector2 worldPos;
    Vector2 newWorldPos;

    private ShadowCaster2DTileMap _shadowCaster2DTileMap;

    // Re-references shadowCaster on accessing
    public ShadowCaster2DTileMap shadowCaster2DTileMap
    {
        get
        {
            if (_shadowCaster2DTileMap == null)
            {
                _shadowCaster2DTileMap = ShadowCaster2DTileMap.Instance;
                if (_shadowCaster2DTileMap == null)
                {
                    Debug.LogError("ShadowCaster2DTileMap is not found in the scene.");
                }
            }
            return _shadowCaster2DTileMap;
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        player = FindObjectOfType<Player>();
        playerOnlyMask = LayerMask.GetMask("PlayerOnly");
        buildingSystem = BuildingSystem.Instance;
        turretManager = TurretManager.Instance;
        StructureTilemap = GameObject.Find("StructureTilemap").GetComponent<Tilemap>();
        AnimatedTilemap = GameObject.Find("AnimatedTilemap").GetComponent<Tilemap>();
        BlueprintTilemap = GameObject.Find("BlueprintTilemap").GetComponent<Tilemap>();
        destroyedTilemap = GameObject.Find("DestroyedTilemap").GetComponent<Tilemap>();
    }

    void OnDisable()
    {
        _TileStatsDict.Clear();
        _wallDamageRotation.Clear();
    }

    #region Alter Tiles

    public bool AddBlueprintAt(Vector3Int gridPos, RuleTileStructure ruleTile, int rotation)
    {
        if (!buildingSystem.CheckSupplies(ruleTile.structureSO))
            return false;

        StructureSO rotatedStructure = StructureRotator.RotateStructure(ruleTile.structureSO, rotation);
        if (!AddNewTileToDict(gridPos, rotatedStructure))
        {
            MessageBanner.PulseMessage("Unable to build there.  It's blocked!", Color.red);
            Debug.LogWarning("Failed to add structure tiles to dict when placing blueprint");
            return false;
        }

        DroneBuildJob buildJob = new DroneBuildJob(DroneJobType.Build, GridCenterToWorld(gridPos));
        if (buildJob == null)
        {
            Debug.LogError("BuildingSystem: BuildJob creation failed");
            MessageBanner.PulseMessage("An error occurued!  Sorry about that, let me know and I'll fix it", Color.red);
            return false;
        }

        if (ruleTile.structureSO.canBuildOnlyOne)
        {
            if (buildingSystem.buildOnlyOneList.Contains(ruleTile.structureSO))
            {
                MessageBanner.Instance.DelayMessage("You've already built one.  One is the max!", Color.red, 0.1f);
                return false;
            }
            else 
            {
                buildingSystem.buildOnlyOneList.Add(ruleTile.structureSO);
                BuildingSystem._UnlockedStructuresDict[ruleTile.structureSO.structureType] = false;
                buildingSystem.buildMenuController.RefreshBuildList();
            }
        }

        ConstructionManager.Instance.AddBuildJob(buildJob);

        _TileStatsDict[gridPos].health *= 0.99f;
        
        BlueprintTilemap.SetTile(gridPos, _TileStatsDict[gridPos].ruleTileStructure.buildUiTile);

        Quaternion q = Quaternion.Euler(0, 0, rotation);
        Matrix4x4 matrix = Matrix4x4.Rotate(q);
        BlueprintTilemap.SetTransformMatrix(gridPos, matrix);

        StartCoroutine(RotateTileObject(BlueprintTilemap, gridPos, matrix));
        AddToPlacedCounter(ruleTile.structureSO.structureType);

        PlayBuildExplosionEffect(gridPos, ruleTile);

        return true;
    }

    public bool AddTileAt(Vector3Int rootPos, RuleTileStructure ruleTile)
    {
        if (!buildingSystem.CheckSupplies(ruleTile.structureSO))
            return false;

        rootPos = GridToRootPos(rootPos);

        _TileStatsDict[rootPos].isBlueprint = false;
        _TileStatsDict[rootPos].health = _TileStatsDict[rootPos].maxHealth;
        Matrix4x4 matrix = BlueprintTilemap.GetTransformMatrix(rootPos);
        BlueprintTilemap.SetTile(rootPos, null);
        BlueprintTilemap.SetTransformMatrix(rootPos, matrix);

        buildingSystem.UnlockStructures(ruleTile.structureSO, true);
        
        //if (!AddNewTileToDict(gridPos, rotatedStructure))
        //{
        //    Debug.LogWarning("Failed to add structure tiles to dict when placing tile");
        //    return false;
        //}

        // Set and rotate animated tile
        if (ruleTile.structureSO.structureType == StructureType.OneByOneWall ||
            ruleTile.structureSO.structureType == StructureType.RazorWire)
        {
            AnimatedTilemap.SetTile(rootPos, _TileStatsDict[rootPos].ruleTileStructure);
        }
        else if (ruleTile.structureSO.structureType == StructureType.OneByOneCorner)
        {
            AnimatedTilemap.SetTile(rootPos, _TileStatsDict[rootPos].ruleTileStructure.buildUiTile);
        }
        else
        {
            AnimatedTilemap.SetTile(rootPos, _TileStatsDict[rootPos].ruleTileStructure.damagedTiles[0]);
            AnimatedTilemap.RefreshAllTiles();
            RefreshSurroundingTiles(rootPos);
        }  

        AnimatedTilemap.SetTransformMatrix(rootPos, matrix);

        if (ruleTile.structureSO.isTurret)
        {
            turretManager.AddTurret(rootPos);
        }

        //StructureRotator.RotateTileAt(AnimatedTilemap, rootPos, rotation);

        // Set structure tile
        StructureTilemap.SetTile(rootPos, ruleTile);
        StructureTilemap.SetTransformMatrix(rootPos, matrix);
        //StructureRotator.RotateTileAt(StructureTilemap, rootPos, rotation);

        StartCoroutine(RotateTileObject(StructureTilemap, rootPos, matrix));

        ClearParticlesAndDebris(rootPos);                
        shadowCaster2DTileMap.Generate();
        UpdateTileDamageSprite(rootPos);

        RemoveFromPlacedCounter(ruleTile.structureSO.structureType);
        AddToBuiltCounter(ruleTile.structureSO.structureType);

        PlayBuildExplosionEffect(rootPos, ruleTile);
        
        return true;
    }

    public void SetRubbleTileAt(Vector3Int rootPos)
    {
        // Match rotation matrix from animated tilemap
        Matrix4x4 matrix = AnimatedTilemap.GetTransformMatrix(rootPos);
        // Get random destroyed tile
        int randomIndex = Random.Range(0, _TileStatsDict[rootPos].ruleTileStructure.destroyedTiles.Count);
        destroyedTilemap.SetTile(rootPos, _TileStatsDict[rootPos].ruleTileStructure.destroyedTiles[randomIndex]);
        destroyedTilemap.SetTransformMatrix(rootPos, matrix);

        // Random rotation for 1x1 tiles
        if (_TileStatsDict[rootPos].ruleTileStructure.structureSO.cellPositions.Count == 1)
        {
            float randomRotationZ = Random.Range(0f, 360f);
            Matrix4x4 rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 0, randomRotationZ));
            destroyedTilemap.SetTransformMatrix(rootPos, rotationMatrix);
        }

        // Check instantiated object for ITileObjectExplodable
        GameObject tileObject = StructureTilemap.GetInstantiatedObject(rootPos);
        StructureType type = _TileStatsDict[rootPos].ruleTileStructure.structureSO.structureType;
        Debug.Log("TileObject for " + type.ToString() + ": " + tileObject);
        if (tileObject != null)
        {
            var explodable = tileObject.GetComponentInChildren<MonoBehaviour>();
            Debug.Log("Explodable for " + type.ToString() + ": " + explodable);
            if (explodable != null && explodable is ITileObjectExplodable tileObjectExplodable)
            {
                tileObjectExplodable.Explode();
                Debug.Log("Explosion triggered for " + type.ToString());
            }
        }

        // Damaging explosion effect, others are handled by tile Objects
        if (type == StructureType.OneByOneWall ||
            type == StructureType.OneByOneCorner)
        {
            if (!string.IsNullOrEmpty(damagingBuildingExplosionPoolName))
            {
                GameObject explosion = PoolManager.Instance.GetFromPool(damagingBuildingExplosionPoolName);
                explosion.transform.position = TileCellsCenterToWorld(rootPos);
            }
        }
        // Aesthetic explosion effect for buildings that do not create it with their object already.
        else if (type == StructureType.SolarPanels)
        {
            if (!string.IsNullOrEmpty(aestheticBuildingExplosionPoolName))
            {
                GameObject explosion = PoolManager.Instance.GetFromPool(aestheticBuildingExplosionPoolName);
                explosion.transform.position = TileCellsCenterToWorld(rootPos);
            }
        }
    }

    public Vector2 TileCellsCenterToWorld(Vector3Int rootPos)
    {
        if (!_TileStatsDict.ContainsKey(rootPos))
        {
            Debug.LogError($"TileCellsCenterToWorld: No tile at {rootPos}");
            return GridCenterToWorld(rootPos);
        }

        // Get the structure's source (unrotated) cell offsets
        List<Vector3Int> cellPositions = _TileStatsDict[rootPos].ruleTileStructure.structureSO.cellPositions;
        if (cellPositions == null || cellPositions.Count == 0)
        {
            return GridCenterToWorld(rootPos);
        }

        // Determine which tilemap currently holds the transform for this tile
        Tilemap tm = null;
        if (AnimatedTilemap != null && AnimatedTilemap.GetTile(rootPos) != null)
            tm = AnimatedTilemap;
        else if (BlueprintTilemap != null && BlueprintTilemap.GetTile(rootPos) != null)
            tm = BlueprintTilemap;
        else
            tm = StructureTilemap;

        // Read rotation and rotate the source offsets back to world-facing orientation
        int rotation = StructureRotator.GetRotationFromMatrix(tm.GetTransformMatrix(rootPos));
        List<Vector3Int> rotatedOffsets = StructureRotator.RotateCellPositionsBack(cellPositions, rotation);

        // Compute grid-space AABB over the absolute occupied cells
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        for (int i = 0; i < rotatedOffsets.Count; i++)
        {
            int gx = rootPos.x + rotatedOffsets[i].x;
            int gy = rootPos.y + rotatedOffsets[i].y;
            if (gx < minX) minX = gx;
            if (gy < minY) minY = gy;
            if (gx > maxX) maxX = gx;
            if (gy > maxY) maxY = gy;
        }

        // Convert grid AABB to world AABB (CellToWorld gives bottom-left corner of a cell)
        Vector2 worldMin = StructureTilemap.CellToWorld(new Vector3Int(minX, minY, 0));
        Vector2 worldMax = StructureTilemap.CellToWorld(new Vector3Int(maxX, maxY, 0)) + (Vector3)StructureTilemap.cellSize;

        // Center of the world-space box
        Vector2 center = (worldMin + worldMax) * 0.5f;
        return center;
    }

    public void GetStructureWorldAABB(Vector3Int rootPos, out Vector2 center, out Vector2 size)
    {
        center = GridCenterToWorld(rootPos);
        size = StructureTilemap.cellSize;

        if (!_TileStatsDict.ContainsKey(rootPos))
            return;

        List<Vector3Int> cellPositions = _TileStatsDict[rootPos].ruleTileStructure.structureSO.cellPositions;
        if (cellPositions == null || cellPositions.Count == 0)
            return;

        Tilemap tm = null;
        if (AnimatedTilemap != null && AnimatedTilemap.GetTile(rootPos) != null)
            tm = AnimatedTilemap;
        else if (BlueprintTilemap != null && BlueprintTilemap.GetTile(rootPos) != null)
            tm = BlueprintTilemap;
        else
            tm = StructureTilemap;

        int rotation = StructureRotator.GetRotationFromMatrix(tm.GetTransformMatrix(rootPos));
        List<Vector3Int> rotatedOffsets = StructureRotator.RotateCellPositionsBack(cellPositions, rotation);

        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        for (int i = 0; i < rotatedOffsets.Count; i++)
        {
            int gx = rootPos.x + rotatedOffsets[i].x;
            int gy = rootPos.y + rotatedOffsets[i].y;
            if (gx < minX) minX = gx;
            if (gy < minY) minY = gy;
            if (gx > maxX) maxX = gx;
            if (gy > maxY) maxY = gy;
        }

        Vector2 worldMin = StructureTilemap.CellToWorld(new Vector3Int(minX, minY, 0));
        Vector2 worldMax = StructureTilemap.CellToWorld(new Vector3Int(maxX, maxY, 0)) + (Vector3)StructureTilemap.cellSize;

        center = (worldMin + worldMax) * 0.5f;
        size = (worldMax - worldMin);
    }

    public void RemoveTileAt(Vector3Int gridPos)
    {
        ConstructionManager.Instance.TileRemoved(GridCenterToWorld(gridPos));
        Vector3Int rootPos = GridToRootPos(gridPos);

        // Remove any stored wall-damage rotation for this root
        _wallDamageRotation.Remove(rootPos);
        
        // Find rotation matrix of tile at gridPos, convert source positions to rotation
        var blueprintTile = BlueprintTilemap.GetTile(rootPos);
        Matrix4x4 matrix;
        int tileRot;
        Tilemap tilemap;
        RuleTileStructure ruleTile = _TileStatsDict[rootPos].ruleTileStructure;

        buildingSystem.LockStructures(ruleTile.structureSO, true);

        if (ruleTile.structureSO.canBuildOnlyOne)
        {
            if (buildingSystem.buildOnlyOneList.Contains(ruleTile.structureSO))
            {
                buildingSystem.buildOnlyOneList.Remove(ruleTile.structureSO);
                BuildingSystem._UnlockedStructuresDict[ruleTile.structureSO.structureType] = true;
                buildingSystem.buildMenuController.RefreshBuildList();
            }
            else 
                Debug.LogError("TileManager: Removed a 'buildOnlyOne' tile that was not tracked in 'buildOnlyOneList'");
        }

        if (blueprintTile != null)
        {
            tilemap = BlueprintTilemap;
            matrix = BlueprintTilemap.GetTransformMatrix(rootPos);
            tileRot = StructureRotator.GetRotationFromMatrix(BlueprintTilemap.GetTransformMatrix(rootPos));
            RemoveFromPlacedCounter(ruleTile.structureSO.structureType);
        }
        else
        {
            tilemap = AnimatedTilemap;
            blueprintTile = AnimatedTilemap.GetTile(rootPos);
            matrix = AnimatedTilemap.GetTransformMatrix(rootPos);
            tileRot = StructureRotator.GetRotationFromMatrix(AnimatedTilemap.GetTransformMatrix(rootPos));
        }
        
        List<Vector3Int> sourcePositions = ruleTile.structureSO.cellPositions;
        //List<Vector3Int> rotatedPositions = GetStructurePositions(tilemap, rootPos);
        List<Vector3Int> rotatedPositions = StructureRotator.RotateCellPositionsBack(sourcePositions, tileRot);


        AnimatedTilemap.SetTile(rootPos, null);
        BlueprintTilemap.SetTile(rootPos, null);

        //AnimatedTilemap.SetTransformMatrix(rootPos, matrix);
        //blueprintTilemap.SetTransformMatrix(rootPos, matrix);

        // Check for turrets
        if (ruleTile.structureSO.isTurret)
            turretManager.RemoveTurret(rootPos);

        // Remove from list and dict
        foreach (var pos in rotatedPositions)
        {
            if (_StructurePositions.Contains(pos))
                _StructurePositions.Remove(pos);
            _TileStatsDict.Remove(rootPos + pos);
            StructureTilemap.SetTile(rootPos + pos, null);           
        }

        if (shadowCaster2DTileMap != null)
            shadowCaster2DTileMap.Generate();
        else
            Debug.LogError("shadowCaster2DTileMap is null");

        RefreshSurroundingTiles(rootPos);
        damageTilemap.SetTile(rootPos, null);
        ParticleManager.Instance.ClearBulletHolesInBounds(GetStrucutureWorldBounds(rootPos));

        //Debug.Log("DESTROYED A TILE");
    }

    public void RefundTileAt(Vector3Int gridPos)
    {
        // Get remaining health ratio and tile cost
        int tileCost = (int)_TileStatsDict[gridPos].ruleTileStructure.structureSO.tileCost;
        int refund;
        
        if (BlueprintTilemap.GetTile(gridPos))
        {
            refund = Mathf.FloorToInt(tileCost * buildingSystem.structureCostMult);
        }
        else
        {
            float ratio = 1 - (_TileStatsDict[gridPos].maxHealth - _TileStatsDict[gridPos].health) /
                    _TileStatsDict[gridPos].maxHealth;
            refund = Mathf.FloorToInt(ratio * buildingSystem.structureCostMult * tileCost);
            refund = Mathf.Clamp(refund, 0, Mathf.FloorToInt(buildingSystem.structureCostMult * tileCost * 0.75f));
        }

        // Refund cost
        BuildingSystem.PlayerCredits += refund;
        PlayBuildExplosionEffect(gridPos, _TileStatsDict[gridPos].ruleTileStructure);
        //Debug.Log("Refunded a tile");
    }

    #endregion

    #region Modify Health

    public bool BuildBlueprintAt(Vector2 pos, float amount, float textPulseScaleMax, Vector2 hitDir)
    {
        Vector3Int gridPos = WorldToGrid(pos);
        Vector3Int rootPos;
        if (_TileStatsDict.ContainsKey(gridPos))
        {
            rootPos = GridToRootPos(gridPos);
            //pos = GridCenterToWorld(rootPos);
        }
        else
            return true;  // To stop building

        if (!_TileStatsDict.ContainsKey(rootPos))
        {
            Debug.LogError($"TileManager: No blueprint fround at {pos} to build");
            return true;  // To return the drone home for next task
        }

        _TileStatsDict[rootPos].blueprintProgress += amount;

        if (_TileStatsDict[rootPos].blueprintProgress >= _TileStatsDict[rootPos].ruleTileStructure.structureSO.blueprintBuildAmount)
        {
            //StructureRotator.RotateTileAt(blueprintTilemap, rootPos, rotation);
            if (!AddTileAt(rootPos, _TileStatsDict[rootPos].ruleTileStructure))
            {
                Debug.LogWarning("TileManager: Failed to add tile at blueprint completion, likely no supplies");
                return false;
            }
            _TileStatsDict[rootPos].health = _TileStatsDict[rootPos].maxHealth;

            ConstructionManager.Instance.RemoveBuildJob(GridCenterToWorld(rootPos));
            //ConstructionManager.Instance.InsertRepairJob(new DroneJob(DroneJobType.Repair, pos));
            return true;
        }

        TextFly textFly = PoolManager.Instance.GetFromPool(textFlyWorldPlayerPoolName).GetComponent<TextFly>();
        textFly.transform.position = pos;
        textFly.Initialize(Mathf.Abs(amount).ToString("#0"), buildBlueprintTextColor, 
                           textFlyAlphaMax, hitDir.normalized, true, textPulseScaleMax);

        return false;
    }

    public void ModifyHealthAt(Vector2 point, float value, float textPulseScaleMax, Vector2 hitDir)
    {
        Vector3Int gridPos = WorldToGrid(point);
        if (!_TileStatsDict.ContainsKey(gridPos))
        {
            numberofTilesMissed++; // For debug
            return;
        }

        Vector3Int rootPos = _TileStatsDict[gridPos].rootGridPos;
        float healthAtStart = _TileStatsDict[rootPos].health;
        float maxHealth = _TileStatsDict[rootPos].maxHealth;

        //if (!(_TileStatsDict[rootPos].ruleTileStructure.structureSO.structureType == StructureType.RazorWire &&
        //    value > 0))
        //{
            _TileStatsDict[rootPos].health += value;
            _TileStatsDict[rootPos].health = Mathf.Clamp(_TileStatsDict[rootPos].health, 0, maxHealth);
        //}
        
        //Debug.Log("TILE HEALTH: " + _TileStatsDict[rootPos].health);
        bool isRepairable = _TileStatsDict[rootPos].ruleTileStructure.structureSO.canBeRepaired;
        bool isException = _TileStatsDict[rootPos].ruleTileStructure.structureSO.structureType == StructureType.Mine;

        float healthDifference = _TileStatsDict[rootPos].health - healthAtStart;
        Color color = textFlyHealthGainColor;
        if (healthDifference == 0)
            return;
        else if (healthDifference < 0)
        {
            if (isRepairable)
                ConstructionManager.Instance.AddRepairJob(new DroneJob(DroneJobType.Repair, GridCenterToWorld(rootPos)));

            color = textFlyHealthLossColor;
            StatsCounterPlayer.DamageToStructures += -healthDifference;
            numberOfTilesHit++;
        }
        else if (healthDifference > 0)
        {
            if (!isRepairable)
                _TileStatsDict[rootPos].health -= healthDifference; // Revert health increase if not repairable
                
            if (_TileStatsDict[rootPos].health >= maxHealth)
                ConstructionManager.Instance.RemoveRepairJob(GridCenterToWorld(rootPos));
        }

        if (!isException)
        {
            TextFly textFly = PoolManager.Instance.GetFromPool(textFlyWorldPlayerPoolName).GetComponent<TextFly>();
            textFly.transform.position = point;
            textFly.Initialize(Mathf.Abs(healthDifference).ToString("#0"), color, 
                            textFlyAlphaMax, hitDir.normalized, true, textPulseScaleMax);
        }

        UpdateTileDamageSprite(rootPos);
    }

    public void ModifyMaxHealthAll(float factor)
    {
        foreach (Vector3Int rootPos in _StructurePositions)
        {
            ModifyMaxHealthAt(rootPos, factor);
        }
    }

    public void ModifyMaxHealthAt(Vector3Int gridPos, float factor)
    {
        if (_TileStatsDict.ContainsKey(gridPos))
        {
            // Get rootPos and structure
            Vector3Int rootPos = _TileStatsDict[gridPos].rootGridPos;
            StructureSO structureSO = _TileStatsDict[rootPos].ruleTileStructure.structureSO;

            // Store ratio of health remaining
            float ratio = _TileStatsDict[gridPos].health / _TileStatsDict[gridPos].maxHealth;

            // Modify max heatlh and set heatlh to ratio
            _TileStatsDict[gridPos].maxHealth = structureSO.maxHealth * factor;
            _TileStatsDict[gridPos].health = _TileStatsDict[gridPos].maxHealth * ratio;
        }
    }

    #endregion

    #region Update Tiles

    Tilemap tempTilemap;
    LayerMask playerOnlyMask;
    List<AnimatedTile> dTilesTemp = new List<AnimatedTile>();
    ContactFilter2D filterTemp = new ContactFilter2D();

    void UpdateTileDamageSprite(Vector3Int rootPos)
    {
        if (_TileStatsDict[rootPos].isBlueprint) return;

        // Check for destroyed
        if (_TileStatsDict[rootPos].health <= 0)
        {
            StatsCounterPlayer.StructuresLost++;
            SetRubbleTileAt(rootPos);
            RemoveTileAt(rootPos);
            return;
        }

        var structureSO = _TileStatsDict[rootPos].ruleTileStructure.structureSO;
        bool isWall = structureSO.tileName == "Wall";
        bool isCorner = structureSO.tileName == "Wall Corner";

        float healthRatio = _TileStatsDict[rootPos].health /
                                 _TileStatsDict[rootPos].maxHealth;

        if (isWall || isCorner)
            tempTilemap = damageTilemap;
        else
            tempTilemap = AnimatedTilemap;

        dTilesTemp = _TileStatsDict[rootPos].ruleTileStructure.damagedTiles;
        if (dTilesTemp.Count > 1)
        {
            int index = GetDamageIndex(healthRatio, dTilesTemp.Count);

            // Decide rotation for the damaged sprite.
            // For regular walls (not corners), we use a persistent random rotation (0, 90, 180, 270).
            Matrix4x4 matrix;

            if (isWall && !isCorner)
            {
                if (!_wallDamageRotation.TryGetValue(rootPos, out float angle))
                {
                    // Pick one of 0, 90, 180, 270 at random the first time this wall is damaged
                    int step = Random.Range(0, 4); // 0,1,2,3
                    angle = step * 90f;
                    _wallDamageRotation[rootPos] = angle;
                }

                matrix = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, angle));
            }
            else
            {
                // Keep original rotation for non-wall or corner tiles
                matrix = AnimatedTilemap.GetTransformMatrix(rootPos);
            }

            if (tempTilemap.GetTile(rootPos) != null)
            {
                tempTilemap.SetTile(rootPos, null);
            }

            tempTilemap.SetTile(rootPos, _TileStatsDict[rootPos].ruleTileStructure.damagedTiles[index]);
            tempTilemap.SetTransformMatrix(rootPos, matrix);
            //StructureRotator.RotateTileAt(tilemap, rootPos, StructureRotator.GetRotationFromMatrix(matrix));
        }
        else
        {
            Vector2 centerWorld = GridCenterToWorld(rootPos);

            // allocation-free overlap using ContactFilter2D + preallocated buffer
            // We want to detect any ITileObject at this cell, regardless of layer.
            filterTemp.NoFilter();
            filterTemp.useLayerMask = false;     // no layer restriction
            filterTemp.useTriggers = true;       // allow trigger panels/objects
            int hitCount = Physics2D.OverlapPoint(centerWorld, filterTemp, _tileColsBuffer);

            float ratio = _TileStatsDict[rootPos].health / _TileStatsDict[rootPos].maxHealth;
            bool updated = false;

            for (int i = 0; i < hitCount; i++)
            {
                var col = _tileColsBuffer[i];

                // Scan behaviours on this object
                ITileObject objFound = null;
                var behaviours = col.GetComponents<MonoBehaviour>();
                for (int b = 0; b < behaviours.Length; b++)
                {
                    if (behaviours[b] is ITileObject it) { objFound = it; break; }
                }

                // If not found, scan children (including inactive)
                if (objFound == null)
                {
                    behaviours = col.GetComponentsInChildren<MonoBehaviour>(true);
                    for (int b = 0; b < behaviours.Length; b++)
                    {
                        if (behaviours[b] is ITileObject it) { objFound = it; break; }
                    }
                }

                // Scan parent objects as well
                if (objFound == null)
                {
                    behaviours = col.GetComponentsInParent<MonoBehaviour>(true);
                    for (int b = 0; b < behaviours.Length; b++)
                    {
                        if (behaviours[b] is ITileObject it) { objFound = it; break; }
                    }
                }

                if (objFound != null)
                {
                    objFound.UpdateHealthRatio(ratio);
                    updated = true;
                }
            }

            // CHANGED: fallback — if no collider match was found (e.g., different layers), try the instantiated tile object
            if (!updated)
            {
                GameObject tileObj = AnimatedTilemap.GetInstantiatedObject(rootPos);
                if (tileObj == null)
                    tileObj = StructureTilemap.GetInstantiatedObject(rootPos);

                if (tileObj != null)
                {
                    var behaviours = tileObj.GetComponentsInChildren<MonoBehaviour>(true);
                    for (int b = 0; b < behaviours.Length; b++)
                    {
                        if (behaviours[b] is ITileObject it)
                        {
                            it.UpdateHealthRatio(ratio);
                            updated = true;
                            break;
                        }
                    }
                }
            }
        }
    }
    
    int GetDamageIndex(float healthRatio, int spriteCount)
    {
        int damageIndex;

        // Reserve index 0 for full health
        if (healthRatio >= 1f)
        {
            damageIndex = 0;
        }
        else
        {
            // Divide remaining indices (1 to spriteCount-1) across the 0–99% damage range
            float normalized = 1f - healthRatio;
            damageIndex = 1 + Mathf.FloorToInt(normalized * (spriteCount - 1));
            damageIndex = Mathf.Clamp(damageIndex, 1, spriteCount - 1);
        }

        return damageIndex;
    }

    public void RefreshSurroundingTiles(Vector3Int gridPos)
    {
        // Refresh the tile itself
        AnimatedTilemap.RefreshTile(gridPos);

        // Refresh its neighbors
        Vector3Int[] directions = new Vector3Int[]
        {
        Vector3Int.zero,
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.left,
        Vector3Int.right,
        Vector3Int.up + Vector3Int.left,
        Vector3Int.up + Vector3Int.right,
        Vector3Int.down + Vector3Int.left,
        Vector3Int.down + Vector3Int.right
        };

        bool neighborExists = false;

        foreach (Vector3Int direction in directions)
        {
            Vector3Int neighborPos = gridPos + direction;
            if (AnimatedTilemap.HasTile(neighborPos))
            {
                neighborExists = true;
                AnimatedTilemap.RefreshTile(neighborPos);
            }
        }

        // Fallback for solo tiles: Ensure it updates even without neighbors
        if (!neighborExists)
        {
            Debug.Log($"Solo tile refresh triggered at {gridPos}");
            AnimatedTilemap.RefreshTile(gridPos);
        }
    }

    #endregion

    #region Checks and Getters

    public Bounds GetStrucutureWorldBounds(Vector3Int rootPos)
    {
        Vector2 center;
        Vector2 size;
        GetStructureWorldAABB(rootPos, out center, out size);
        return new Bounds(center, size);
    }

    /// <summary>
    /// Returns all grid cells (keys known to _TileStatsDict) whose world-space
    /// cell rectangles intersect the circle centered at centerWorldPos with radius.
    /// This returns individual CELLS (may include multiple cells belonging to the same structure root).
    /// </summary>
    public List<Vector3Int> GetOccupiedCellsUnderCircle(Vector2 centerWorldPos, float radius)
    {
        List<Vector3Int> cells = new List<Vector3Int>();

        // Compute a bounding box in grid space to limit checks
        Vector2 worldMin = centerWorldPos - new Vector2(radius, radius);
        Vector2 worldMax = centerWorldPos + new Vector2(radius, radius);

        Vector3Int minCell = StructureTilemap.WorldToCell(worldMin);
        Vector3Int maxCell = StructureTilemap.WorldToCell(worldMax);

        for (int x = minCell.x - 1; x <= maxCell.x + 1; x++)
        {
            for (int y = minCell.y - 1; y <= maxCell.y + 1; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);

                if (!_TileStatsDict.ContainsKey(cell))
                    continue;

                if (!CircleIntersectsCell(centerWorldPos, radius, cell))
                    continue;

                cells.Add(cell);
            }
        }

        return cells;
    }

    /// <summary>
    /// Finds the single closest UiUpgradePanel within a circle centered at centerWorldPos with radius.
    /// This checks BOTH:
    ///  1) Regular scene objects via Physics2D.OverlapCircleAll (e.g., QCube), and
    ///  2) Tilemap-based structures by testing which cells intersect the circle.
    /// Returns null if none found.
    /// </summary>
    public (GameObject, PanelInteract) GetClosestObjectAndPanelUnderCircle(Vector2 playerWorldPos, float radius)
    {
        GameObject closestObject = null;
        PanelInteract closestPanel = null;
        float bestDistSq = float.PositiveInfinity;

        // --- 1) Check non-tilemap objects (e.g., QCube) via collider overlap ---
        if (structureInteractLayerMask.value != 0)
        {
            int hitCount = Physics2D.OverlapCircleNonAlloc(playerWorldPos, radius, _panelColsBuffer, structureInteractLayerMask);
            for (int i = 0; i < hitCount; i++)
            {
                var col = _panelColsBuffer[i];
                var panel = col.GetComponentInChildren<PanelInteract>();
                if (panel == null) continue;

                Vector2 closestPoint = col.ClosestPoint(playerWorldPos);
                float dSq = (closestPoint - playerWorldPos).sqrMagnitude;
                if (dSq < bestDistSq)
                {
                    bestDistSq = dSq;
                    closestObject = col.gameObject;
                    closestPanel = panel;
                }
            }
        }

        // --- 2) Check tilemap-based structures under the circle ---
        var cells = GetOccupiedCellsUnderCircle(playerWorldPos, radius);
        float cellSize = StructureTilemap.cellSize.x / 2; // assuming square cells
        foreach (var cell in cells)
        {
            Vector3Int rootPos = _TileStatsDict[cell].rootGridPos;
            //if (!seenRoots.Add(rootPos))
            //    continue;

            GameObject obj = StructureTilemap.GetInstantiatedObject(rootPos);
            if (obj == null)
                continue;

            PanelInteract panel = obj.GetComponentInChildren<PanelInteract>();
            if (panel == null)
                panel = obj.GetComponentInChildren<PanelInteract>(true);

            if (panel == null)
                continue;

            // If distance to cell is greater than best, skip
            Vector2 cellWorldPos = GridCenterToWorld(cell);
            float distSq = (cellWorldPos - playerWorldPos).sqrMagnitude;
            if (distSq > bestDistSq)
                continue;

            bestDistSq = distSq;
            closestObject = obj;
            closestPanel = panel;
        }

        return (closestObject, closestPanel);
    }

    /// <summary>
    /// Helper: accurate circle vs. axis-aligned cell-rectangle intersection test in world space.
    /// Returns true if the tile cell at 'cell' intersects the circle centered at 'centerWorldPos' with 'radius'.
    /// </summary>
    private bool CircleIntersectsCell(Vector2 centerWorldPos, float radius, Vector3Int cell)
    {
        // Get the world-space AABB for this grid cell
        Vector2 cellWorldMin = (Vector2)StructureTilemap.CellToWorld(cell);
        Vector2 cellSize = (Vector2)StructureTilemap.cellSize;
        Vector2 cellWorldMax = cellWorldMin + cellSize;

        // Clamp circle center to the rectangle to find the closest point
        float closestX = Mathf.Clamp(centerWorldPos.x, cellWorldMin.x, cellWorldMax.x);
        float closestY = Mathf.Clamp(centerWorldPos.y, cellWorldMin.y, cellWorldMax.y);

        float dx = centerWorldPos.x - closestX;
        float dy = centerWorldPos.y - closestY;

        return (dx * dx + dy * dy) <= radius * radius;
    }

    public float GetRepairCostAt(Vector2 worldPos, float repairAmount)
    {
        Vector3Int gridPos = WorldToGrid(worldPos);
        if (!_TileStatsDict.ContainsKey(gridPos))
        {
            Debug.LogWarning($"TileManager: No structure exists at: {gridPos}");
            return 0;
        }

        float maxHealth = _TileStatsDict[gridPos].maxHealth;
        float currentHealth = _TileStatsDict[gridPos].health;
        float actualRepairAmount = Mathf.Min(repairAmount, maxHealth - currentHealth);
        
        float tileCost = _TileStatsDict[gridPos].ruleTileStructure.structureSO.tileCost;
        float cost = actualRepairAmount / maxHealth * (tileCost * 0.5f);
        
        return cost;
    }

    public bool IsTileBlueprint(Vector2 worldPos)
    {
        Vector3Int gridPos = WorldToGrid(worldPos);
        if (_TileStatsDict.ContainsKey(gridPos) && _TileStatsDict[gridPos].isBlueprint)
            return true;

        return false;
    }

    public List<Vector3Int> GetAllStructurePositions()
    {
        return _StructurePositions;
    }

    public bool ContainsTileKey(Vector3Int gridPos)
    {
        if (_TileStatsDict.ContainsKey(gridPos))
            return true;
        else
            return false;
    }

    public Vector3Int GridToRootPos(Vector3Int gridPos)
    {
        if (!_TileStatsDict.ContainsKey(gridPos))
        {
            Debug.LogError("TileManager: GridToRootPos called on non-existent gridPos: " + gridPos);
            return Vector3Int.zero;
        }
        return _TileStatsDict[gridPos].rootGridPos;
    }

    public Vector3Int WorldToGrid(Vector2 point)
    {
        Vector3Int gridPos = StructureTilemap.WorldToCell(point);
        return gridPos;
    }

    public Vector2 GridToWorld(Vector3Int gridPos)
    {
        Vector2 worldPos = StructureTilemap.CellToWorld(gridPos);
        return worldPos;
    }

    public Vector2 GridCenterToWorld(Vector3Int gridPos)
    {
        Vector2 worldPos = StructureTilemap.CellToWorld(gridPos);
        worldPos += new Vector2(StructureTilemap.cellSize.x / 2, StructureTilemap.cellSize.y / 2);
        return worldPos;
    }
    
    public float GetTileHealthRatio(Vector3Int rootPos)
    {
        float healthRatio = (_TileStatsDict[rootPos].maxHealth - _TileStatsDict[rootPos].health) /
                             _TileStatsDict[rootPos].maxHealth;

        return healthRatio;
    }

    public bool CheckTileFullHealth(Vector3 point)
    {
        Vector3Int gridPos = StructureTilemap.WorldToCell(point);

        if (!_TileStatsDict.ContainsKey(gridPos))
        {
            // True causes repair bullet to do nothing, as it should
            return true;
        }

        if (_TileStatsDict[gridPos].health < _TileStatsDict[gridPos].maxHealth)
            return false;
        
        return true;
    }

    public bool CheckGridIsClear(Vector3Int rootPos, StructureSO structure, LayerMask layerMask, bool checkDict)
    {
        //Debug.Log("CheckGridIsClear started w/ " + structure + " at " + rootPos);
        foreach (Vector3Int pos in structure.cellPositions)
        {   
            if (checkDict)
            {
                // Check tile dictionary
                if (_TileStatsDict.ContainsKey(rootPos + pos))
                {
                    Debug.Log("Structure rootPos already exists in dictionary");
                    return false;
                }
            }

            // Check for colliders
            worldPos = StructureTilemap.CellToWorld(rootPos + pos);
                   
            boxSize = StructureTilemap.cellSize;
            newWorldPos = new Vector2(worldPos.x + boxSize.x/2, worldPos.y + boxSize.y/2); 
            boxSize = StructureTilemap.cellSize * 0.9f;

            Collider2D[] hits = Physics2D.OverlapBoxAll(newWorldPos, boxSize, 0, layerMask);
            
            foreach (Collider2D col in hits)
            {
                if (col.tag != "PickupTrigger" && col.gameObject.layer != LayerMask.NameToLayer("FlyingEnemies"))
                {
                    //Debug.Log("CheckGridIsClear found a collider at the checked position");
                    return false;
                }
            }
        }

        //Debug.Log("CheckGrid returned clear");
        return true;
    }

    public bool CheckGridIsClear(Vector3Int gridPos, LayerMask layerMask, bool checkDict)
    {
        //Debug.Log("CheckGridIsClear started w/ one gridPos");
        if (checkDict)
        {
            // Check tile dictionary
            if (_TileStatsDict.ContainsKey(gridPos))
            {
                //Debug.Log("Structure rootPos already exists in dictionary");
                return false;
            }
        }

        // Check for colliders
        worldPos = StructureTilemap.CellToWorld(gridPos);
                   
        boxSize = StructureTilemap.cellSize;
        newWorldPos = new Vector2(worldPos.x + boxSize.x/2, worldPos.y + boxSize.y/2); 
        boxSize = StructureTilemap.cellSize * 0.9f;

        Collider2D[] hits = Physics2D.OverlapBoxAll(newWorldPos, boxSize, 0, layerMask);
        
        foreach (Collider2D col in hits)
        {
            if (col.tag != "PickupTrigger" && col.gameObject.layer != LayerMask.NameToLayer("FlyingEnemies"))
            {
                //Debug.Log("Collider detected on CheckGridIsClear");
                return false;
            }
            
        }

        return true;
    }

    public List<Vector3Int> GetStructurePositions(Tilemap tilemap, Vector3Int gridPos)
    {
        if (_TileStatsDict.ContainsKey(gridPos))
        {
            Vector3Int rootPos = _TileStatsDict[gridPos].rootGridPos;
            if (!_TileStatsDict.ContainsKey(rootPos)) return null;

            RuleTileStructure rts = _TileStatsDict[rootPos].ruleTileStructure;
            
            // Get original positions
            List<Vector3Int> sourcePositions = new List<Vector3Int>(rts.structureSO.cellPositions);

            // Get rotation from transform matrix
            int rotation = GetTileRotation(tilemap, rootPos);

            // Rotate the sourcePositions
            List<Vector3Int> rotatedPositions = StructureRotator.RotateCellPositionsBack(sourcePositions, rotation);

            // Get positions relative to rootPos
            for (int i = 0; i < rotatedPositions.Count; i++)
            {
                rotatedPositions[i] += rootPos;
            }

            return rotatedPositions;
            
        }
        else
        {
            List<Vector3Int> positions = new List<Vector3Int>();
        
            return positions;
        }
    }

    int GetTileRotation(Tilemap tilemap, Vector3Int gridPos)
    {
        Vector3Int rootPos = GridToRootPos(gridPos);
        Matrix4x4 matrix = tilemap.GetTransformMatrix(rootPos);
        return StructureRotator.GetRotationFromMatrix(matrix);
    }

    public bool CheckBlueprintCellsAreClear(Vector3Int gridPos)
    {
        List<Vector3Int> structurePositions = new List<Vector3Int>(GetStructurePositions(BlueprintTilemap, gridPos));
        foreach (var pos in structurePositions)
        {

            if (!CheckGridIsClear(pos, buildingSystem.layersForBuildClearCheck, false))
                return false;
        }

        return true;
    }

    public StructureSO GetStructureAt(Vector2 worldPos)
    {
        Vector3Int gridPos = WorldToGrid(worldPos);

        if (_TileStatsDict.ContainsKey(gridPos))
            return _TileStatsDict[gridPos].ruleTileStructure.structureSO;
        else 
            return null;
    }

    #endregion

    #region Tile Dictionary

    // To load tile data from pre-set scene tiles, they must be placed on StructureTilemap
    bool ReadTilemapToDict()
    {
        foreach (Vector3Int gridPos in StructureTilemap.cellBounds.allPositionsWithin)
        {
            if (StructureTilemap.HasTile(gridPos))
            {
                var tileInMap = StructureTilemap.GetTile(gridPos);
                if (tileInMap is RuleTileStructure rts)
                {
                    // Need new method to add existing tiles with set health for tutorial level
                    if(AddNewTileToDict(gridPos, rts.structureSO))
                    {
                        AnimatedTilemap.SetTile(gridPos, 
                            _TileStatsDict[gridPos].ruleTileStructure.damagedTiles[0]);
                    }
                    else 
                    {
                        Debug.LogError("Failed to add a pre-existing tile to dict on scene load");
                        return false;
                    }
                }
            }
        }
        return true;
    }

    bool AddNewTileToDict(Vector3Int rootPos, StructureSO structure)
    {
        if (CheckGridIsClear(rootPos, structure, buildingSystem.layersForBuildClearCheck, true))
        {
            // Add the tile's data to the dictionary at root location
            if (!_TileStatsDict.ContainsKey(rootPos))
            {
                // Add to structures list
                _StructurePositions.Add(rootPos);

                float maxHP = structure.maxHealth * player.stats.structureStats.structureMaxHealthMult;
                // Add to TileStats
                _TileStatsDict.Add(rootPos, new TileStats
                {
                    ruleTileStructure = structure.ruleTileStructure,
                    maxHealth = maxHP,
                    health = maxHP,
                    rootGridPos = new Vector3Int(rootPos.x, rootPos.y, 0),
                    isBlueprint = true,
                });

                //Debug.Log("Structure added to structures list and dict");
            }
            else
            {
                Debug.LogError("RootPos was already taken when trying to add to dict");
                return false;
            }
        }

        // Add any other cellPositions;
        List<Vector3Int> positions = structure.cellPositions;
        if (positions.Count > 1)
        {
            for (int x = 1; x < positions.Count; x++)
            {
                Vector3Int nextPos = rootPos + positions[x];
                if (!_TileStatsDict.ContainsKey(nextPos))
                {
                    _TileStatsDict.Add(nextPos, _TileStatsDict[rootPos]);
                    //Debug.Log("added extra key at: " + nextPos);
                }
                else
                {
                    Debug.Log("nextPos was already taken in dict when adding extra tiles to large tile");
                    return false;
                }
            }
        }
        return true;
    }

    #endregion

    #region Tools
    
    void PlayBuildExplosionEffect(Vector3Int rootPos, RuleTileStructure ruleTile)
    {
        // Check number of cells to determine placed 'explosion' effect size
        int cellCount = ruleTile.structureSO.cellPositions.Count;
        if (cellCount > 4)
        {
            if (!string.IsNullOrEmpty(buildingBuiltLargeExplosionPoolName))
            {
                GameObject explosion = PoolManager.Instance.GetFromPool(buildingBuiltLargeExplosionPoolName);
                explosion.transform.position = TileCellsCenterToWorld(rootPos);
            }
        }
        else if (cellCount > 2)
        {
            if (!string.IsNullOrEmpty(buildingBuiltMediumExplosionPoolName))
            {
                GameObject explosion = PoolManager.Instance.GetFromPool(buildingBuiltMediumExplosionPoolName);
                explosion.transform.position = TileCellsCenterToWorld(rootPos);
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(buildingBuiltSmallExplosionPoolName))
            {
                GameObject explosion = PoolManager.Instance.GetFromPool(buildingBuiltSmallExplosionPoolName);
                explosion.transform.position = TileCellsCenterToWorld(rootPos);
            }
        }
    }

    public IEnumerator RotateTileObject(Tilemap tilemap, Vector3Int gridPos, Matrix4x4 matrix)
    {
        yield return new WaitForEndOfFrame();

        float angleRadians = Mathf.Atan2(matrix.m01, matrix.m00); // m01 = sin, m00 = cos
        float angleDegrees = -angleRadians * Mathf.Rad2Deg;

        GameObject tileObj = tilemap.GetInstantiatedObject(gridPos);

        // Rotate GameObject
        if (tileObj != null)
        {
            tileObj.transform.rotation = Quaternion.Euler(0, 0, angleDegrees);
            //Debug.Log("TileObj rotation set to: " + angleDegrees);
        }
        else 
        {
            //Debug.LogWarning("tileObj was null when attempting to rotate");
        }
    }

    void ClearParticlesAndDebris(Vector3Int gridPos)
    {
        List<Vector3Int> positions = GetStructurePositions(StructureTilemap, gridPos);
        foreach (ParticleSystem ps in particlesToClear)
        {
            // Access the particles from the particle system
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.main.maxParticles];
            int particleCount = ps.GetParticles(particles);

            for (int i = 0; i < particleCount; i++)
            {
                Vector3 particlePosition = particles[i].position;

                // Check if the particle is within any of the defined overlap boxes
                foreach (Vector3Int pos in positions)
                {
                    Vector2 boxCenter = StructureTilemap.CellToWorld(pos);
                    boxCenter = new Vector2(boxCenter.x + 0.5f, boxCenter.y + 0.5f);

                    if (IsParticleInBox(particlePosition, boxCenter, Vector2.one))
                    {
                        //particles[i].startLifetime = -1f;
                        //particles[i].startSize = 0;
                        //particles[i].startColor = new Color(0, 0, 0, 0);
                        particles[i].remainingLifetime = 0; // Remove the particle by setting its remaining lifetime to zero
                        break;
                    }
                }
            }
            ps.SetParticles(particles, particleCount);
        }

        foreach (Vector3Int pos in positions)
        {
            destroyedTilemap.SetTile(pos, null);
        }
    }

    bool IsParticleInBox(Vector3 particlePosition, Vector2 boxCenter, Vector2 boxSize)
    {
        
        bool inBox = particlePosition.x >= boxCenter.x - boxSize.x / 2 && 
                     particlePosition.x <= boxCenter.x + boxSize.x / 2 &&
                     particlePosition.y >= boxCenter.y - boxSize.y / 2 && 
                     particlePosition.y <= boxCenter.y + boxSize.y / 2;
        Debug.Log("Particle box check: " + inBox);
        return inBox;
    }

    void AddToBuiltCounter(StructureType structureType)
    {
        StatsCounterPlayer.StructuresBuilt++;

        switch (structureType)
        {
            case StructureType.OneByOneCorner:
            case StructureType.OneByOneWall:
                StatsCounterPlayer.WallsBuilt++;
                break;
            case StructureType.Gate:
            case StructureType.BlastGate:
                StatsCounterPlayer.GatesBuilt++;
                break;
            case StructureType.LaserTurret:
            case StructureType.GunTurret:
                StatsCounterPlayer.TurretsBuilt++;
                break;
            case StructureType.SolarPanels:
                StatsCounterPlayer.SolarPanelsBuilt++;
                break;
            case StructureType.MicroReactor:
                StatsCounterPlayer.MicroReactorsBuilt++;
                break;
            case StructureType.EngineeringBay:
                StatsCounterPlayer.EngineeringBaysBuilt++;
                break;
            case StructureType.PhotonicsBay:
                StatsCounterPlayer.PhotonicsBayBuilt++;
                break;
            case StructureType.BallisticsBay:
                StatsCounterPlayer.BallisticsBayBuilt++;
                break;
            case StructureType.ExplosivesBay:
                StatsCounterPlayer.ExplosivesBayBuilt++;
                break;
            case StructureType.RepairBay:
                StatsCounterPlayer.RepairBayBuilt++;
                break;
            case StructureType.DroneHangar:
                StatsCounterPlayer.DroneHangarsBuilt++;
                break;
            case StructureType.SupplyDepot:
                StatsCounterPlayer.SupplyDepotsBuilt++;
                break;
            default:
                Debug.LogError("TileManager: Untracked structure type for stats: " + structureType);
                break;
        }
    }
    
    void AddToPlacedCounter(StructureType structureType)
    {
        switch (structureType)
        {
            case StructureType.OneByOneCorner:
            case StructureType.OneByOneWall:
                StatsCounterPlayer.WallsPlaced++;
                break;
            case StructureType.Gate:
            case StructureType.BlastGate:
                StatsCounterPlayer.GatesPlaced++;
                break;
            case StructureType.LaserTurret:
            case StructureType.GunTurret:
                StatsCounterPlayer.TurretsPlaced++;
                break;
            case StructureType.SolarPanels:
                StatsCounterPlayer.SolarPanelsPlaced++;
                break;
            case StructureType.MicroReactor:
                StatsCounterPlayer.MicroReactorsPlaced++;
                break;
            case StructureType.EngineeringBay:
                StatsCounterPlayer.EngineeringBaysPlaced++;
                break;
            case StructureType.PhotonicsBay:
                StatsCounterPlayer.PhotonicsBayPlaced++;
                break;
            case StructureType.BallisticsBay:
                StatsCounterPlayer.BallisticsBayPlaced++;
                break;
            case StructureType.ExplosivesBay:
                StatsCounterPlayer.ExplosivesBayPlaced++;
                break;
            case StructureType.RepairBay:
                StatsCounterPlayer.RepairBayPlaced++;
                break;
            case StructureType.DroneHangar:
                StatsCounterPlayer.DroneHangarsPlaced++;
                break;
            case StructureType.SupplyDepot:
                StatsCounterPlayer.SupplyDepotsPlaced++;
                break;
            default:
                Debug.LogError("TileManager: Untracked structure type for stats: " + structureType);
                break;
        }
    }

    void RemoveFromPlacedCounter(StructureType structureType)
    {
        switch (structureType)
        {
            case StructureType.OneByOneCorner:
            case StructureType.OneByOneWall:
                StatsCounterPlayer.WallsPlaced--;
                break;
            case StructureType.Gate:
            case StructureType.BlastGate:
                StatsCounterPlayer.GatesPlaced--;
                break;
            case StructureType.LaserTurret:
            case StructureType.GunTurret:
                StatsCounterPlayer.TurretsPlaced--;
                break;
            case StructureType.SolarPanels:
                StatsCounterPlayer.SolarPanelsPlaced--;
                break;
            case StructureType.MicroReactor:
                StatsCounterPlayer.MicroReactorsPlaced--;
                break;
            case StructureType.EngineeringBay:
                StatsCounterPlayer.EngineeringBaysPlaced--;
                break;
            case StructureType.PhotonicsBay:
                StatsCounterPlayer.PhotonicsBayPlaced--;
                break;
            case StructureType.BallisticsBay:
                StatsCounterPlayer.BallisticsBayPlaced--;
                break;
            case StructureType.ExplosivesBay:
                StatsCounterPlayer.ExplosivesBayPlaced--;
                break;
            case StructureType.RepairBay:
                StatsCounterPlayer.RepairBayPlaced--;
                break;
            case StructureType.DroneHangar:
                StatsCounterPlayer.DroneHangarsPlaced--;
                break;
            case StructureType.SupplyDepot:
                StatsCounterPlayer.SupplyDepotsPlaced--;
                break;
            default:
                Debug.LogError("TileManager: Untracked structure type for stats: " + structureType);
                break;
        }
    }

    #endregion

    #region Debug

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (StructureTilemap != null)
        {
            // Draw dictionary cells
            foreach (var kvp in _TileStatsDict)
            {
                Vector3Int gridPos = kvp.Key;
                Vector3 worldPos = StructureTilemap.CellToWorld(gridPos);
                Vector3 cellSize = StructureTilemap.cellSize;

                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(worldPos + cellSize / 2, cellSize);
            }
        }

        Gizmos.color = Color.blue;

        Gizmos.DrawWireCube(newWorldPos, boxSize);
    }

    #endregion
}