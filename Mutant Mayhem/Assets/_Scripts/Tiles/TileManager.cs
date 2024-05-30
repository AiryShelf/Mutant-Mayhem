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
    // This stores the TileStats for each tile in 
    // the Structures Tilemap (on "Structures" layer)
    private static Dictionary<Vector3Int, TileStats> _TileStatsDict = 
                                            new Dictionary<Vector3Int, TileStats>();
    public static Tilemap StructureTilemap;
    public static Tilemap AnimatedTilemap;
    public LayerMask layersForGridClearCheck;

    public int numberOfTilesHit;
    public int numberofTilesMissed;

    // For debugging
    Vector2 boxPos;
    Vector2 boxSize;
    Vector2 worldPos;
    Vector2 newWorldPos;



    void Awake()
    {
        StructureTilemap = GameObject.Find("StructureTilemap").GetComponent<Tilemap>();
        AnimatedTilemap = GameObject.Find("AnimatedTilemap").GetComponent<Tilemap>();
        if (!ReadTilemapToDict())
            Debug.Log("ERROR when trying to read the starting tilemap to dict");
    }

    void OnDisable()
    {
        _TileStatsDict.Clear();
    }

    Vector3Int GetGridPos(Vector2 point)
    {
        Vector3Int gridPos = StructureTilemap.WorldToCell(point);
        return gridPos;
    }

    public bool AddTileAt(Vector3Int gridPos, RuleTileStructure rts)
    {
        if (CheckGridIsClear(gridPos, rts))
        {
            if (AddToTileStatsDict(gridPos, rts))
            {
                AnimatedTilemap.SetTile(gridPos, 
                    _TileStatsDict[gridPos].ruleTileStructure.damagedTiles[0]);
                StructureTilemap.SetTile(gridPos, rts);

                StatsCounterPlayer.StructuresBuilt++;
                //Debug.Log("Added a Tile");
                return true;
            }
            else
            {
                Debug.Log("Failed to add structure tiles to dict");
                return false;
            }
        }
        else
        {
            Debug.Log("Grid was not clear for building on");
            return false;
        }       
    }

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

    public AnimatedTile GetRootAnimTile(Vector3Int gridPos)
    {
        return _TileStatsDict[gridPos].ruleTileStructure.damagedTiles[0];
    }

    public void DestroyTileAt(Vector3Int gridPos)
    {
        List<Vector3Int> positions = _TileStatsDict[gridPos].ruleTileStructure.cellPositions;
        Vector3Int rootPos = _TileStatsDict[gridPos].rootGridPos;
        AnimatedTilemap.SetTile(rootPos, null);

        for (int x = positions.Count - 1; x >= 0; x--)
        {
            _TileStatsDict.Remove(rootPos + positions[x]);
            StructureTilemap.SetTile(rootPos + positions[x], null);           
        }
       
        //StructureTilemap.SetTile(rootPos, null);
        AnimatedTilemap.SetTile(rootPos, null);

        StatsCounterPlayer.StructuresLost++;
        //Debug.Log("DESTROYED A TILE");
    }

    public void ModifyHealthAt(Vector2 point, float amount)
    {
        Vector3Int gridPos = GetGridPos(point);
        //Debug.Log("modify health called");
        if (_TileStatsDict.ContainsKey(gridPos))
        {
            numberOfTilesHit++;
            Vector3Int rootPos = _TileStatsDict[gridPos].rootGridPos;
            _TileStatsDict[rootPos].health += amount;
            _TileStatsDict[rootPos].health = Mathf.Clamp(_TileStatsDict[rootPos].health, 
                                                         0, _TileStatsDict[rootPos].maxHealth);
            //Debug.Log("TILE HEALTH: " + _TileStatsDict[rootPos].health);

            if (_TileStatsDict[rootPos].health == 0)
            {
                DestroyTileAt(rootPos);
                return;
            }
            UpdateAnimatedTile(rootPos);
        }
        else
        {
            numberofTilesMissed++;
            //Debug.Log("Key not found: " + gridPos);
        }
    }

    public void BulletHitEffectAt(Vector2 point, Vector2 hitDir)
    {
        Vector3Int gridPos = GetGridPos(point);
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
        Vector3Int gridPos = GetGridPos(point);
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

    bool ReadTilemapToDict()
    {
        foreach (Vector3Int rootPos in StructureTilemap.cellBounds.allPositionsWithin)
        {
            if (StructureTilemap.HasTile(rootPos))
            {
                var tileInMap = StructureTilemap.GetTile(rootPos);
                if (tileInMap is RuleTileStructure rts)
                {
                    if(AddToTileStatsDict(rootPos, rts))
                    {
                        AnimatedTilemap.SetTile(rootPos, 
                            _TileStatsDict[rootPos].ruleTileStructure.damagedTiles[0]);
                    }
                    else return false;
                }
            }
        }
        return true;
    }

    bool AddToTileStatsDict(Vector3Int rootPos, RuleTileStructure rts)
    {
        if (CheckGridIsClear(rootPos, rts))
        {                                                      
            // Add the tile's data to the dictionary at root location
            if (!_TileStatsDict.ContainsKey(rootPos))
            {
                _TileStatsDict.Add(rootPos, new TileStats 
                { 
                    ruleTileStructure = rts,
                    maxHealth = rts.structureSO.maxHealth,
                    health = rts.structureSO.health,
                    rootGridPos = rootPos
                });
            }
            else
            {
                Debug.Log("RootPos was already taken when trying to add to dict");
                return false;
            }
        }

        // Add any other cellPositions;
        int length = rts.cellPositions.Count;
        List<Vector3Int> positions = rts.cellPositions;
        if (length > 1)
        {
            for (int x = 1; x < length; x++)
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

    void UpdateAnimatedTile(Vector3Int rootPos)
    {
        // AnimatedTilemap.GetTile(rootPos);
        float healthRatio = 1 - (_TileStatsDict[rootPos].health / 
                                 _TileStatsDict[rootPos].maxHealth);
        List<AnimatedTile> dTiles = _TileStatsDict[rootPos].ruleTileStructure.damagedTiles;

        int index = Mathf.FloorToInt(healthRatio * dTiles.Count);
        index = Mathf.Clamp(index, 0, dTiles.Count - 1);

        if (AnimatedTilemap.GetTile(rootPos) != null)
        {
            AnimatedTilemap.SetTile(rootPos, null);
        }
        AnimatedTilemap.SetTile(rootPos, 
            _TileStatsDict[rootPos].ruleTileStructure.damagedTiles[index]);
    }

    public bool CheckGridIsClear(Vector3Int rootPos, RuleTileStructure rts)
    {
        foreach (Vector3Int pos in rts.cellPositions)
        {   
            // Check tile dictionary
            if (_TileStatsDict.ContainsKey(rootPos + pos))
            {
                return false;
            }

            // Check for colliders
            worldPos = StructureTilemap.CellToWorld(rootPos + pos);
                   
            boxSize = StructureTilemap.cellSize;
            newWorldPos = new Vector2(worldPos.x + boxSize.x/2, worldPos.y + boxSize.y/2); 
            boxSize = StructureTilemap.cellSize * 0.9f;

            Collider2D hit = Physics2D.OverlapBox(newWorldPos, boxSize, 0, layersForGridClearCheck);
            
            if (hit != null)
            {
                Debug.Log("Collider detected when trying to build");
                return false;
            }
        }

        return true;
    }

    public bool CheckGridIsClear(Vector3Int gridPos)
    {
        // Check tile dictionary
        if (_TileStatsDict.ContainsKey(gridPos))
        {
            return false;
        }

        // Check for colliders
        worldPos = StructureTilemap.CellToWorld(gridPos);
                   
        boxSize = StructureTilemap.cellSize;
        newWorldPos = new Vector2(worldPos.x + boxSize.x/2, worldPos.y + boxSize.y/2); 
        boxSize = StructureTilemap.cellSize * 0.9f;

        Collider2D hit = Physics2D.OverlapBox(newWorldPos, boxSize, 0, layersForGridClearCheck);
        
        if (hit != null)
        {
            //Debug.Log("Collider detected when trying to build");
            return false;
        }

        return true;
    }

    public List<Vector3Int> GetStructurePositions(Vector3Int gridPos)
    {
        if (_TileStatsDict.ContainsKey(gridPos))
        {
            Vector3Int rootPos = _TileStatsDict[gridPos].rootGridPos;
            RuleTileStructure rts = _TileStatsDict[rootPos].ruleTileStructure;
            
            List<Vector3Int> positions = new List<Vector3Int>(rts.cellPositions);

            for (int i = 0; i < positions.Count; i++)
            {
                positions[i] += rootPos;
            }

            return positions;
        }
        else
        {
            List<Vector3Int> positions = new List<Vector3Int>();
        
            return positions;
        }

        
    }

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
}

