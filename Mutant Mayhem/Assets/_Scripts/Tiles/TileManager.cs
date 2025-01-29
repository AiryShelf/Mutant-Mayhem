using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    // This stores the TileStats/reference for each tile in 
    // the Structures Tilemap (on "Structures" layer)
    private static Dictionary<Vector3Int, TileStats> _TileStatsDict = 
                                            new Dictionary<Vector3Int, TileStats>();
    private static List<Vector3Int> _StructurePositions = new List<Vector3Int>();
    public Player player;
    public static Tilemap StructureTilemap;
    public Grid StructureGrid;

    public Tilemap blueprintTilemap;
    public static Tilemap AnimatedTilemap;
    [SerializeField] Tilemap destroyedTilemap;
    [SerializeField] Tilemap damageTilemap;
    [SerializeField] List<ParticleSystem> particlesToClear;
    [SerializeField] Color textFlyHealthLossColor;
    [SerializeField] Color textFlyHealthGainColor;
    [SerializeField] float textFlyAlphaMax;
    [SerializeField] Color buildBlueprintTextColor = Color.cyan;

    public int numberOfTilesHit;
    public int numberofTilesMissed;

    BuildingSystem buildingSystem;
    TurretManager turretManager;

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
                _shadowCaster2DTileMap = FindObjectOfType<ShadowCaster2DTileMap>();
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
        player = FindObjectOfType<Player>();
        buildingSystem = FindObjectOfType<BuildingSystem>();
        turretManager = FindObjectOfType<TurretManager>();
        StructureTilemap = GameObject.Find("StructureTilemap").GetComponent<Tilemap>();
        AnimatedTilemap = GameObject.Find("AnimatedTilemap").GetComponent<Tilemap>();
        destroyedTilemap = GameObject.Find("DestroyedTilemap").GetComponent<Tilemap>();
        //if (!ReadTilemapToDict())
            //Debug.LogError("Error when trying to read the starting tilemap to dict");
        //shadowCaster2DTileMap = FindObjectOfType<ShadowCaster2DTileMap>();
    }

    void OnDisable()
    {
        _TileStatsDict.Clear();
    }

    #region Alter Tiles

    public bool AddBlueprintAt(Vector3Int gridPos, RuleTileStructure ruleTile, int rotation)
    {
        StructureSO rotatedStructure = StructureRotator.RotateStructure(ruleTile.structureSO, rotation);
        if (!AddNewTileToDict(gridPos, rotatedStructure))
        {
            Debug.LogWarning("Failed to add structure tiles to dict when placing blueprint");
            return false;
        }

        DroneBuildJob buildJob = new DroneBuildJob(DroneJobType.Build, GridCenterToWorld(gridPos));
        if (buildJob == null)
        {
            Debug.LogError("BuildingSystem: BuildJob creation failed");
            MessagePanel.PulseMessage("An error occurued!  Sorry about that, let me know and I'll fix it", Color.red);
            return false;
        }
        ConstructionManager.Instance.AddBuildJob(buildJob);

        _TileStatsDict[gridPos].health *= 0.99f;

        if (ruleTile.structureSO.tileName == "Wall")
            blueprintTilemap.SetTile(gridPos, _TileStatsDict[gridPos].ruleTileStructure);
        else if (ruleTile.structureSO.tileName == "Wall Corner")
            blueprintTilemap.SetTile(gridPos, _TileStatsDict[gridPos].ruleTileStructure.buildUiTile);
        else
        {
            blueprintTilemap.SetTile(gridPos, _TileStatsDict[gridPos].ruleTileStructure.damagedTiles[0]);
            RefreshSurroundingTiles(gridPos);
        }

        Quaternion q = Quaternion.Euler(0, 0, rotation);
        Matrix4x4 matrix = Matrix4x4.Rotate(q);
        blueprintTilemap.SetTransformMatrix(gridPos, matrix);

        if (ruleTile.structureSO.isTurret)
        {
            turretManager.currentNumTurrets++;
        }

        return true;
    }

    public bool AddTileAt(Vector3Int rootPos, RuleTileStructure ruleTile)
    {
        _TileStatsDict[rootPos].isBlueprint = false;
        _TileStatsDict[rootPos].health = _TileStatsDict[rootPos].maxHealth;
        Matrix4x4 matrix = blueprintTilemap.GetTransformMatrix(rootPos);
        blueprintTilemap.SetTile(rootPos, null);
        blueprintTilemap.SetTransformMatrix(rootPos, matrix);
        
        //if (!AddNewTileToDict(gridPos, rotatedStructure))
        //{
        //    Debug.LogWarning("Failed to add structure tiles to dict when placing tile");
        //    return false;
        //}

        // Set and rotate animated tile
        if (ruleTile.structureSO.tileName == "Wall")
            AnimatedTilemap.SetTile(rootPos, _TileStatsDict[rootPos].ruleTileStructure);
        else if (ruleTile.structureSO.tileName == "Wall Corner")
            AnimatedTilemap.SetTile(rootPos, _TileStatsDict[rootPos].ruleTileStructure.buildUiTile);
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

        StartCoroutine(RotateTileObject(rootPos, matrix));

        ClearParticlesAndDebris(rootPos);                
        shadowCaster2DTileMap.Generate();
        UpdateTileDamageSprite(rootPos);
        
        return true;
    }

    public void SetRubbleTileAt(Vector3Int rootPos)
    {
        Matrix4x4 matrix = AnimatedTilemap.GetTransformMatrix(rootPos);
        destroyedTilemap.SetTile(rootPos, _TileStatsDict[rootPos].ruleTileStructure.destroyedTile);
        destroyedTilemap.SetTransformMatrix(rootPos, matrix);

        StructureType type = _TileStatsDict[rootPos].ruleTileStructure.structureSO.structureType;
        if (type == StructureType.OneByOneWall ||
            type == StructureType.OneByOneCorner ||
            type == StructureType.TwoByTwoWall ||
            type == StructureType.TwoByTwoCorner)
        {
            float randomRotationZ = Random.Range(0f, 360f);  
            Matrix4x4 rotationMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 0, randomRotationZ));
            destroyedTilemap.SetTransformMatrix(rootPos, rotationMatrix);
        }
    }

    public void RemoveTileAt(Vector3Int gridPos)
    {
        ConstructionManager.Instance.TileRemoved(GridCenterToWorld(gridPos));
        Vector3Int rootPos = GridToRootPos(gridPos);
        
        // Find rotation matrix of tile at gridPos, convert source positions to rotation
        var tile = blueprintTilemap.GetTile(rootPos);
        Matrix4x4 matrix;
        int tileRot;
        Tilemap tilemap;
        if (tile != null)
        {
            tilemap = blueprintTilemap;
            matrix = blueprintTilemap.GetTransformMatrix(rootPos);
            tileRot = StructureRotator.GetRotationFromMatrix(blueprintTilemap.GetTransformMatrix(rootPos));
        }
        else
        {
            tilemap = AnimatedTilemap;
            tile = AnimatedTilemap.GetTile(rootPos);
            matrix = AnimatedTilemap.GetTransformMatrix(rootPos);
            tileRot = StructureRotator.GetRotationFromMatrix(AnimatedTilemap.GetTransformMatrix(rootPos));
        }
        RuleTileStructure rts = _TileStatsDict[rootPos].ruleTileStructure;
        List<Vector3Int> sourcePositions = rts.structureSO.cellPositions;
        //List<Vector3Int> rotatedPositions = GetStructurePositions(tilemap, rootPos);
        List<Vector3Int> rotatedPositions = StructureRotator.RotateCellPositionsBack(sourcePositions, tileRot);


        AnimatedTilemap.SetTile(rootPos, null);
        blueprintTilemap.SetTile(rootPos, null);

        //AnimatedTilemap.SetTransformMatrix(rootPos, matrix);
        //blueprintTilemap.SetTransformMatrix(rootPos, matrix);

        // Check for turrets
        if (rts.structureSO.isTurret)
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

        //Debug.Log("DESTROYED A TILE");
    }

    public void RefundTileAt(Vector3Int gridPos)
    {
        // Get remaining health ratio and tile cost
        
        int tileCost = (int)_TileStatsDict[gridPos].ruleTileStructure.structureSO.tileCost;
        int refund;
        
        if (blueprintTilemap.GetTile(gridPos))
        {
            refund = tileCost;
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
            pos = GridCenterToWorld(rootPos);
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
            _TileStatsDict[rootPos].health = _TileStatsDict[rootPos].maxHealth;
            AddTileAt(rootPos, _TileStatsDict[rootPos].ruleTileStructure);

            ConstructionManager.Instance.RemoveBuildJob(pos);
            //ConstructionManager.Instance.InsertRepairJob(new DroneJob(DroneJobType.Repair, pos));
            return true;
        }

        TextFly textFly = PoolManager.Instance.GetFromPool("TextFlyWorld_Health").GetComponent<TextFly>();
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

        _TileStatsDict[rootPos].health += value;
        _TileStatsDict[rootPos].health = Mathf.Clamp(_TileStatsDict[rootPos].health, 0, maxHealth);
        //Debug.Log("TILE HEALTH: " + _TileStatsDict[rootPos].health);

        float healthDifference = _TileStatsDict[rootPos].health - healthAtStart;
        Color color = textFlyHealthGainColor;
        if (healthDifference == 0)
            return;
        else if (healthDifference < 0)
        {
            //if (healthAtStart >= maxHealth)
                ConstructionManager.Instance.AddRepairJob(new DroneJob(DroneJobType.Repair, GridCenterToWorld(rootPos)));

            color = textFlyHealthLossColor;
            StatsCounterPlayer.DamageToStructures += -healthDifference;
            numberOfTilesHit++;
        }
        else if (healthDifference > 0)
        {
            if (_TileStatsDict[rootPos].health >= maxHealth -1)
                ConstructionManager.Instance.RemoveRepairJob(GridCenterToWorld(rootPos));
        }

        TextFly textFly = PoolManager.Instance.GetFromPool("TextFlyWorld_Health").GetComponent<TextFly>();
        textFly.transform.position = point;
        textFly.Initialize(Mathf.Abs(healthDifference).ToString("#0"), color, 
                           textFlyAlphaMax, hitDir.normalized, true, textPulseScaleMax);

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

    void UpdateTileDamageSprite(Vector3Int rootPos)
    {
        if (_TileStatsDict[rootPos].health == 0)
        {
            StatsCounterPlayer.StructuresLost++;
            SetRubbleTileAt(rootPos);
            RemoveTileAt(rootPos);
            return;
        }

        float healthRatio = 1 - (_TileStatsDict[rootPos].health / 
                                 _TileStatsDict[rootPos].maxHealth);

        Tilemap tilemap;
        if (_TileStatsDict[rootPos].ruleTileStructure .structureSO.tileName == "Wall" || 
            _TileStatsDict[rootPos].ruleTileStructure .structureSO.tileName == "Wall Corner")
            tilemap = damageTilemap;
        else
            tilemap = AnimatedTilemap;

        List<AnimatedTile> dTiles = _TileStatsDict[rootPos].ruleTileStructure.damagedTiles;
        if (dTiles.Count > 1)
        {
            int index = Mathf.FloorToInt(healthRatio * dTiles.Count);
            index = Mathf.Clamp(index, 0, dTiles.Count - 1);

            // Keep original rotation
            Matrix4x4 matrix = AnimatedTilemap.GetTransformMatrix(rootPos);

            if (tilemap.GetTile(rootPos) != null)
            {
                tilemap.SetTile(rootPos, null);
            }

            tilemap.SetTile(rootPos, _TileStatsDict[rootPos].ruleTileStructure.damagedTiles[index]);
            tilemap.SetTransformMatrix(rootPos, matrix);
            //StructureRotator.RotateTileAt(tilemap, rootPos, StructureRotator.GetRotationFromMatrix(matrix));
        }
        else 
        {
            // This handles doors and TileObjects
            int layerMask = LayerMask.GetMask("PlayerOnly");
            Collider2D[] cols = Physics2D.OverlapPointAll(new Vector2(rootPos.x + 0.5f, rootPos.y + 0.5f), layerMask);
            foreach (Collider2D col in cols)
            {
                TileObject tileObj = col.GetComponent<TileObject>();
                if (tileObj != null)
                {
                    //Debug.Log("TileObject found at: " + rootPos);
                    tileObj.UpdateHealthRatio(GetTileHealthRatio(rootPos));
                }
                else
                {
                    //Debug.Log("TileObject not found");
                }
            }
        }
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
                if (col.tag != "PickupTrigger")
                {
                    Debug.Log("CheckGridIsClear found a collider at the checked position");
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
            if (col.tag != "PickupTrigger")
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
        List<Vector3Int> structurePositions = new List<Vector3Int>(GetStructurePositions(blueprintTilemap, gridPos));
        foreach (var pos in structurePositions)
        {

            if (!CheckGridIsClear(pos, buildingSystem.layersForBuildClearCheck, false))
                return false;
        }

        return true;
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
                    Debug.Log("nextPos was already taken in dict when " +
                                "adding extra tiles to large tile");
                    return false;
                }
            }
        }
        return true;
    }

    #endregion

    #region Tools

    IEnumerator RotateTileObject(Vector3Int gridPos, Matrix4x4 matrix)
    {
        yield return new WaitForFixedUpdate();

        float angleRadians = Mathf.Atan2(matrix.m01, matrix.m00); // m01 = sin, m00 = cos
        float angleDegrees = -angleRadians * Mathf.Rad2Deg;

        GameObject tileObj = StructureTilemap.GetInstantiatedObject(gridPos);

        // Rotate GameObject
        if (tileObj != null)
        {
            tileObj.transform.rotation = Quaternion.Euler(0, 0, angleDegrees);
            Debug.Log("TileObj rotation set to: " + angleDegrees);
        }
        else 
        {
            Debug.LogWarning("tileObj was null when attempting to rotate");
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