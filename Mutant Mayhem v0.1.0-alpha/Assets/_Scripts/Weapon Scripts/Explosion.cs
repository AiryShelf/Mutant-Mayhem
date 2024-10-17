using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Explosion : MonoBehaviour
{
    [SerializeField] SoundSO explosionSound;
    
    [Header("Explosion Settings")]
    [SerializeField] float force;
    [SerializeField] float radius;
    [SerializeField] float damage;
    [SerializeField] WindZone wind;
    [SerializeField] float windTime;
    
    List<Vector3Int> visibleTiles = new List<Vector3Int>();

    TileManager tileManager;

    void Start()
    {
        // CAN ADD WINDZONE COROUTINE TO CAUSE PRESSURE EFFECT
        AudioManager.Instance.PlaySoundAt(explosionSound, transform.position);

        tileManager = FindObjectOfType<TileManager>();
        if (tileManager == null)
        {
            Debug.LogError("Explosion could not find TileManager in scene");
            return;
        }

        // Find grid tiles in range
        Vector2 explosionPos = transform.position;
        List<Vector3Int> tilesToCheck = GetTilesInRadius(explosionPos, tileManager);

        Explode(explosionPos, tilesToCheck, tileManager);
        ApplyDamageToEntitiesInTiles(explosionPos, visibleTiles);
    }

    void Explode(Vector2 explosionPos, List<Vector3Int> tilesToCheck, TileManager tileManager)
    {
        
        List<Vector3Int> hitTiles = new List<Vector3Int>();

        // Check each tile in radius
        foreach (Vector3Int tilePos in tilesToCheck)
        {
            Vector2 tileCenter = tileManager.GridCenterToWorld(tilePos);
            Vector2 dirToTile = tileCenter - explosionPos;

            // Perform a raycast from the explosion center to the tile center
            RaycastHit2D hit = Physics2D.Raycast(explosionPos, dirToTile, 
                               Vector2.Distance(tileCenter, explosionPos), 
                               LayerMask.GetMask("Structures"));

            if (hit.collider != null)
            {
                // We hit something, check if it's a structure
                Vector2 nudge = dirToTile.normalized * 0.01f;
                Vector2 worldPos = hit.point + nudge;
                Vector3Int gridPos = tileManager.WorldToGrid(worldPos);
                
                if (tileManager.ContainsTileDictKey(gridPos))
                {
                    // Apply half damage to the structure if tile hasn’t been hit yet
                    if (!hitTiles.Contains(gridPos))
                    {
                        Debug.Log("Explosion tried to hit a tile");
                        float distToPoint = Vector2.Distance(explosionPos, worldPos);
                        float totalDamage = Mathf.Clamp(damage / 2 / distToPoint, 0, damage / 2);
                        tileManager.ModifyHealthAt(worldPos, -totalDamage);

                        // Add the tile to the list of hit tiles
                        hitTiles.Add(gridPos);
                        visibleTiles.Add(gridPos);
                    }
                }
            }
            else
            {
                visibleTiles.Add(tilePos);
            }
        }
    }

    void ApplyDamageToEntitiesInTiles(Vector2 explosionPos, List<Vector3Int> visibleTiles)
    {
        // Only damage player or cube once
        bool playerWasHit = false;
        bool cubeWasHit = false;

        foreach (Vector3Int tilePos in visibleTiles)
        {
            // Convert the tile position back to world space
            Vector2 tileCenter = tileManager.GridCenterToWorld(tilePos);

            // Create bounds for the tile, based on tile size
            Vector3 tileSize = tileManager.StructureGrid.cellSize;
            Bounds tileBounds = new Bounds(tileCenter, tileSize);

            // Check for enemies or player within this tile
            Collider2D[] entitiesInTile = Physics2D.OverlapBoxAll(tileCenter, tileSize, 0, LayerMask.GetMask("Enemies", "Player", "QCube"));

            foreach (Collider2D entity in entitiesInTile)
            {
                // Apply damage to enemies
                EnemyBase enemy = entity.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    if (tileBounds.Contains(enemy.transform.position))
                    {
                        float distToPoint = Vector2.Distance(explosionPos, entity.transform.position);
                        
                        // Apply force
                        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
                        if (rb == null)
                        {
                            Debug.Log("Could not find enemy's Rigidbody during explosion");
                            continue;
                        }
                        Vector2 direction = enemy.transform.position - transform.position;
                        float forceMagnitude = Mathf.Clamp(force / distToPoint, 0, force);
                        rb.AddForce(direction * forceMagnitude, ForceMode2D.Impulse);

                        // Apply damage
                        float totalDamage = Mathf.Clamp(damage / distToPoint, 0, damage);
                        enemy.ModifyHealth(-totalDamage, gameObject);
                        //Debug.Log($"Enemy hit at {entity.transform.position} for {totalDamage} damage");
                        continue;
                    }
                }

                // Apply half damage to the player
                Player player = entity.GetComponent<Player>();
                if (player != null && !playerWasHit)
                {
                    float distToPoint = Vector2.Distance(explosionPos, player.transform.position);

                    // Apply force
                    Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                    Vector2 direction = player.transform.position - transform.position;
                    float forceMagnitude = Mathf.Clamp(force / distToPoint, 0, force);
                    rb.AddForce(direction * forceMagnitude, ForceMode2D.Impulse);

                    // Apply damage
                    Health pHealth = player.GetComponent<Health>();
                    float totalDamage = Mathf.Clamp(damage / 2 / distToPoint, 0, damage / 2);
                    pHealth.ModifyHealth(-totalDamage, gameObject);
                    Debug.Log($"Player hit at {player.transform.position} for {totalDamage} damage");
                    playerWasHit = true;
                    continue;
                }

                // Apply half damage to the Cube
                QCubeHealth cubeHealth = entity.GetComponent<QCubeHealth>();
                if (cubeHealth != null && !cubeWasHit)
                {
                    float distToPoint = Vector2.Distance(explosionPos, cubeHealth.transform.position);
                    float totalDamage = Mathf.Clamp(damage / 2 / distToPoint, 0, damage / 2);
                    cubeHealth.ModifyHealth(-totalDamage, gameObject);
                    Debug.Log($"QCube hit at {cubeHealth.transform.position} for {totalDamage} damage");
                    cubeWasHit = true;
                }
            }
        }
    }

    List<Vector3Int> GetTilesInRadius(Vector2 pos, TileManager tileManager)
    {
        List<Vector3Int> tilesInRadius = new List<Vector3Int>();
        Vector3Int centerTile = tileManager.WorldToGrid(pos);
        int gridRadius = Mathf.CeilToInt(radius);

        for (int x = -gridRadius; x <= gridRadius; x++)
        {
            for (int y = -gridRadius; y <= gridRadius; y++)
            {
                Vector3Int gridPos = centerTile + new Vector3Int(x, y, 0);
                Vector2 tileCenter = tileManager.GridCenterToWorld(gridPos);
                float distToTile = Vector2.Distance(pos, tileCenter);

                // Only add tiles within the explosion's circular radius
                if (distToTile <= radius)
                {
                    tilesInRadius.Add(gridPos);
                }
            }
        }

        return tilesInRadius;
    }

    void OnDrawGizmos()
    {
        // Ensure the TileManager is assigned
        if (tileManager == null)
        {
            return;
        }

        // Set the gizmo color to green to represent visible tiles
        Gizmos.color = Color.green;

        // Loop through all visible tiles
        foreach (Vector3Int tilePos in visibleTiles)
        {
            // Convert the tile position back to world space
            Vector2 tileCenter = tileManager.GridCenterToWorld(tilePos);

            // Get the size of the tile from the grid
            Vector3 tileSize = tileManager.StructureGrid.cellSize;

            // Draw a wireframe cube to represent the tile
            Gizmos.DrawWireCube(tileCenter, tileSize);
        }
    }
}
    