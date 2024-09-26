using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileStats
{   
    [SerializeField]public RuleTileStructure ruleTileStructure;
    public float maxHealth;
    public float health;
    public Vector3Int rootGridPos;
}

public class TileManager : MonoBehaviour
{
    // This stores the TileStats/reference for each tile in 
    // the Structures Tilemap (on "Structures" layer)
    private static Dictionary<Vector3Int, TileStats> _TileStatsDict = 
                                            new Dictionary<Vector3Int, TileStats>();
    private static List<Vector3Int> _Structures = new List<Vector3Int>();
    public Player player;
    public static Tilemap StructureTilemap;
    public Grid StructureGrid;
    public static Tilemap AnimatedTilemap;
    public LayerMask layersForBuildClearCheck;
    public LayerMask layersForBuildClearingArea; // Finish implementation for corpses
    [SerializeField] List<ParticleSystem> particlesToClear;
    public ParticleSystem repairEffect;
    public int amountRepairParticles = 5;

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
        if (!ReadTilemapToDict())
            Debug.LogError("Error when trying to read the starting tilemap to dict");
        //shadowCaster2DTileMap = FindObjectOfType<ShadowCaster2DTileMap>();
    }

    void OnDisable()
    {
        _TileStatsDict.Clear();
    }

    #region Alter Tiles

    public bool AddTileAt(Vector3Int gridPos, StructureSO structure, int rotation)
    {
        if (CheckGridIsClear(gridPos, structure, layersForBuildClearCheck, true))
        {
            if (AddNewTileToDict(gridPos, structure))
            {
                // Set and rotate animated tile
                AnimatedTilemap.SetTile(gridPos, 
                    _TileStatsDict[gridPos].ruleTileStructure.damagedTiles[0]);
                StructureRotator.RotateTileAt(AnimatedTilemap, gridPos, rotation);

                // Set structure tile
                StructureTilemap.SetTile(gridPos, structure.ruleTileStructure);
                
                // Rotate the structure tile, find new bounds
                StructureRotator.RotateTileAt(StructureTilemap, gridPos, rotation);
                Vector2Int bounds = StructureRotator.CalculateBoundingBox(structure.cellPositions);

                if (structure.cellPositions.Count > 1)
                {
                    StartCoroutine(RotateTileObject(gridPos, bounds, rotation));
                }

                ClearParticles(gridPos);                
                shadowCaster2DTileMap.Generate();
                StatsCounterPlayer.StructuresBuilt++;
                //Debug.Log("Added a Tile");
                
                Debug.Log("Structure tile and Animated tile placed");
                return true;
            }
            else
            {
                Debug.LogWarning("Failed to add structure tiles to dict when placing tile");
                return false;
            }
        }
        else
        {
            Debug.Log("Grid was not clear for building on");
            return false;
        }       
    }

    void ClearParticles(Vector3Int gridPos)
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
    }

    bool IsParticleInBox(Vector3 particlePosition, Vector2 boxCenter, Vector2 boxSize)
    {
        
        bool inBox = particlePosition.x >= boxCenter.x - boxSize.x / 2 && particlePosition.x <= boxCenter.x + boxSize.x / 2 &&
                     particlePosition.y >= boxCenter.y - boxSize.y / 2 && particlePosition.y <= boxCenter.y + boxSize.y / 2;
        Debug.Log("Particle box check: " + inBox);
        return inBox;
    }

    IEnumerator RotateTileObject(Vector3Int gridPos, Vector2Int bounds, int rotation)
    {
        yield return new WaitForFixedUpdate();

        GameObject tileObj = StructureTilemap.GetInstantiatedObject(gridPos);

        // Rotate and position GameObject
        if (tileObj != null)
        {
            //StructureRotator.RepositionGameObject(StructureTilemap, tileObj, gridPos, bounds, rotation);
            tileObj.transform.rotation = Quaternion.Euler(0, 0, rotation);
        }
        else
        {
            //Debug.Log("TileObject not found");
        }
    }

    public void RemoveTileAt(Vector3Int rootPos)
    {
        // Find rotation matrix of tile at gridPos, convert source positions to rotation
        int tileRot = StructureRotator.GetRotationFromMatrix(AnimatedTilemap.GetTransformMatrix(rootPos));
        RuleTileStructure rts = _TileStatsDict[rootPos].ruleTileStructure;
        List<Vector3Int> sourcePositions = rts.structureSO.cellPositions;
        List<Vector3Int> rotatedPositions = StructureRotator.RotateCellPositionsBack(sourcePositions, tileRot);

        AnimatedTilemap.SetTile(rootPos, null);

        // Check for turrets
        if (rts.structureSO.isTurret)
        {
            turretManager.RemoveTurret(rootPos);
        }

        // Remove from list and dict
        foreach (var pos in rotatedPositions)
        {
            if (_Structures.Contains(pos))
                _Structures.Remove(rootPos + pos);
            _TileStatsDict.Remove(rootPos + pos);
            StructureTilemap.SetTile(rootPos + pos, null);           
        }

        if (shadowCaster2DTileMap != null)
            shadowCaster2DTileMap.Generate();
        else
            Debug.LogError("shadowCaster2DTileMap is null");

        //Debug.Log("DESTROYED A TILE");
    }

    public void RefundTileAt(Vector3Int gridPos)
    {
        // Get remaining health ratio and tile cost
        float ratio = 1 - (_TileStatsDict[gridPos].maxHealth - _TileStatsDict[gridPos].health) /
                      _TileStatsDict[gridPos].maxHealth;
        int cost = Mathf.FloorToInt(buildingSystem.structureCostMult * 
                   _TileStatsDict[gridPos].ruleTileStructure.structureSO.tileCost);

        // Refund cost
        BuildingSystem.PlayerCredits += cost * ratio / 2;

        //Debug.Log("Refunded a tile");
    }

    public void ModifyHealthAt(Vector2 point, float amount)
    {
        Vector3Int gridPos = WorldToGrid(point);
        //Debug.Log("modify health called");
        if (_TileStatsDict.ContainsKey(gridPos))
        {
            if (amount < 0)
            {
                numberOfTilesHit++;
            }
            
            Vector3Int rootPos = _TileStatsDict[gridPos].rootGridPos;
            _TileStatsDict[rootPos].health += amount;
            _TileStatsDict[rootPos].health = Mathf.Clamp(_TileStatsDict[rootPos].health, 
                                                         0, _TileStatsDict[rootPos].maxHealth);
            //Debug.Log("TILE HEALTH: " + _TileStatsDict[rootPos].health);

            if (_TileStatsDict[rootPos].health == 0)
            {
                StatsCounterPlayer.StructuresLost++;
                RemoveTileAt(rootPos);
                return;
            }

            UpdateAnimatedTile(rootPos);
        }
        else
        {
            // For debug
            numberofTilesMissed++;
            //Debug.Log("Key not found: " + gridPos);
        }
    }

    public void ModifyMaxHealthAll(float factor)
    {
        foreach (Vector3Int rootPos in _Structures)
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

    void UpdateAnimatedTile(Vector3Int rootPos)
    {
        // AnimatedTilemap.GetTile(rootPos);
        float healthRatio = 1 - (_TileStatsDict[rootPos].health / 
                                 _TileStatsDict[rootPos].maxHealth);
        List<AnimatedTile> dTiles = _TileStatsDict[rootPos].ruleTileStructure.damagedTiles;

        if (dTiles.Count > 1)
        {
            int index = Mathf.FloorToInt(healthRatio * dTiles.Count);
            index = Mathf.Clamp(index, 0, dTiles.Count - 1);

            // Keep original rotation
            Matrix4x4 matrix = AnimatedTilemap.GetTransformMatrix(rootPos);

            if (AnimatedTilemap.GetTile(rootPos) != null)
            {
                AnimatedTilemap.SetTile(rootPos, null);
            }

            AnimatedTilemap.SetTile(rootPos, 
                _TileStatsDict[rootPos].ruleTileStructure.damagedTiles[index]);
            AnimatedTilemap.SetTransformMatrix(rootPos, matrix);
        }
        else 
        {
            int layerMask = LayerMask.GetMask("PlayerOnly");
            Collider2D tileObj = Physics2D.OverlapPoint(new Vector2(rootPos.x + 0.5f, rootPos.y + 0.5f), layerMask);
            if (tileObj)
            {
                //Debug.Log("TileObject found at: " + rootPos);
                tileObj.GetComponent<TileObject>().UpdateHealthRatio(GetTileHealthRatio(rootPos));
            }
            else
            {
                Debug.Log("TileObject not found");
            }
            
        }
    }

    #endregion

    #region Checks and Getters

    public bool ContainsTileDictKey(Vector3Int gridPos)
    {
        if (_TileStatsDict.ContainsKey(gridPos))
            return true;
        else
            return false;
    }

    public Vector3Int GetRootPos(Vector3Int gridPos)
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
        Debug.Log("CheckGridIsClear started w/ rootPos and structureSO");
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

            Collider2D hit = Physics2D.OverlapBox(newWorldPos, boxSize, 0, layerMask);
            
            if (hit != null)
            {
                Debug.Log("CheckGridIsClear found a collider at the checked position");
                return false;
            }
        }

        Debug.Log("CheckGrid returned clear");
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

        Collider2D hit = Physics2D.OverlapBox(newWorldPos, boxSize, 0, layerMask);
        
        if (hit != null)
        {
            //Debug.Log("Collider detected on CheckGridIsClear");
            return false;
        }

        return true;
    }

    public List<Vector3Int> GetStructurePositions(Tilemap tilemap, Vector3Int gridPos)
    {
        if (_TileStatsDict.ContainsKey(gridPos))
        {
            // Get rootPos and structure
            Vector3Int rootPos = _TileStatsDict[gridPos].rootGridPos;
            RuleTileStructure rts = _TileStatsDict[rootPos].ruleTileStructure;
            
            // Get original positions
            List<Vector3Int> sourcePositions = new List<Vector3Int>(rts.structureSO.cellPositions);

            // Get rotation from transform matrix
            Matrix4x4 matrix = tilemap.GetTransformMatrix(rootPos);
            int rotation = StructureRotator.GetRotationFromMatrix(matrix);

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

    #endregion

    #region Effects

    public void BulletHitEffectAt(Vector2 point, Vector2 hitDir)
    {
        Vector3Int gridPos = WorldToGrid(point);
        //Debug.Log("HitEffects called");
        if (_TileStatsDict.ContainsKey(gridPos))
        {
            TileStats stats = _TileStatsDict[gridPos];
            GameObject effect = stats.ruleTileStructure.hitEffectsPrefab;
            float angle = Mathf.Atan2(-hitDir.y, -hitDir.x) * Mathf.Rad2Deg;
            effect = Instantiate(effect, point, Quaternion.Euler(0, 0, angle));
            HitEffects hitfx = effect.GetComponent<HitEffects>();
            hitfx.PlayBulletHitEffect(point, hitDir);
            hitfx.DestroyAfterSeconds();
        }
        else
        {
            //Debug.Log("Key not found: " + gridPos + " when shooting a tile");
        }
    }

    public void MeleeHitEffectAt(Vector2 point, Vector2 hitDir)
    {
        Vector3Int gridPos = WorldToGrid(point);
        //Debug.Log("HitEffects called");
        if (_TileStatsDict.ContainsKey(gridPos))
        {
            TileStats stats = _TileStatsDict[gridPos];
            GameObject effect = stats.ruleTileStructure.hitEffectsPrefab;
            float angle = Mathf.Atan2(-hitDir.y, -hitDir.x) * Mathf.Rad2Deg;
            effect = Instantiate(effect, point, Quaternion.Euler(0, 0, angle));
            HitEffects hitfx = effect.GetComponent<HitEffects>();
            hitfx.PlayMeleeHitEffect(point, hitDir);
            hitfx.DestroyAfterSeconds();
        }
        else
        {
            //Debug.Log("Key not found: " + gridPos + " when meleeing a tile");
        }
    }

    public void RepairEffectAt(Vector2 point)
    {
        Vector3Int gridPos = WorldToGrid(point);
        //Debug.Log("HitEffects called");
        if (_TileStatsDict.ContainsKey(gridPos))
        {
            repairEffect.transform.position = point;
            repairEffect.Emit(amountRepairParticles);
        }
        else
        {
            //Debug.Log("Key not found: " + gridPos + " when repairing a tile");
        }
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
        if (CheckGridIsClear(rootPos, structure, layersForBuildClearCheck, true))
        {                                                      
            // Add the tile's data to the dictionary at root location
            if (!_TileStatsDict.ContainsKey(rootPos))
            {
                // Add to structures list
                _Structures.Add(rootPos);

                float maxHP = structure.maxHealth * player.stats.structureStats.structureMaxHealthMult;
                // Add to TileStats
                _TileStatsDict.Add(rootPos, new TileStats 
                { 
                    ruleTileStructure = structure.ruleTileStructure,
                    maxHealth = maxHP,
                    health = maxHP,
                    rootGridPos = new Vector3Int(rootPos.x, rootPos.y, 0)
                });

                Debug.Log("Structure added to structures list and dict");
            }
            else
            {
                Debug.Log("RootPos was already taken when trying to add to dict");
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