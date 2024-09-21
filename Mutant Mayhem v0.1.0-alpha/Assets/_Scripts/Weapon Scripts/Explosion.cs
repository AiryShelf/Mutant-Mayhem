using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] SoundSO explosionSound;
    
    [Header("Explosion Settings")]
    [SerializeField] float force;
    [SerializeField] float radius;
    [SerializeField] float damage;
    [SerializeField] WindZone wind;
    [SerializeField] float windTime;

    private List<Vector3Int> hitTiles = new List<Vector3Int>(); // For gizmos
    private List<Vector3Int> obstacleTiles = new List<Vector3Int>();

    void Start()
    {
        // CAN ADD WINDZONE COROUTINE TO CAUSE PRESSURE EFFECT

        AudioManager.Instance.PlaySoundAt(explosionSound, transform.position);

        // Find objects in range
        Vector2 pos = transform.position;
        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(pos, radius, 
                                        LayerMask.GetMask("Enemies", "Player", "Corpses"));
        // Loop through findings
        foreach (Collider2D collider in objectsInRange)
        {
            // Apply force
            Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = pos - rb.position;
                float dist = dir.magnitude;
                dir.Normalize();
                rb.AddForce(-dir * (force / dist), ForceMode2D.Impulse);

                ApplyDamage(collider, dist);
            }           
        }

        ApplyTileDamage(transform.position);
    }
    void ApplyDamage(Collider2D collider, float dist)
    {
        float totalDamage = Mathf.Clamp(damage / dist, 0, damage);

        // Apply Enemy Damage and Trigger AI
        EnemyBase enemy = collider.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.ModifyHealth(-totalDamage, gameObject);
            enemy.StartFreeze();
            enemy.SetAggroStatus(true);
            enemy.EnemyChaseSOBaseInstance.StartSprint();

            StatsCounterPlayer.TotalDamageByPlayerExplosions += totalDamage;
        }
        else
        {
            // Apply Player damage
            Player player = collider.GetComponent<Player>();
            if (player != null)
            {
                totalDamage = Mathf.Clamp(damage / 2 / dist, 0, damage);
                Health pHealth = player.GetComponent<Health>();
                pHealth.ModifyHealth(-totalDamage, gameObject);                   
                StatsCounterPlayer.TotalDamageByPlayerExplosions += damage / dist;
            }
        }
    }

    void ApplyTileDamage(Vector2 explosionPos)
    {
        TileManager tileManager = FindObjectOfType<TileManager>();
        if (tileManager == null)
        {
            Debug.LogError("Explosion could not find TileManager in scene");
            return;
        }

        List<Vector3Int> hitList = new List<Vector3Int>();

        Vector3Int explosionCenter = tileManager.WorldToGrid(explosionPos);
        int gridRadius = Mathf.CeilToInt(radius);

        for (int x = -gridRadius; x <= gridRadius; x++)
        {
            for (int y = -gridRadius; y <= gridRadius; y++)
            {
                Vector3Int gridPos = explosionCenter + new Vector3Int(x, y, 0);
                Vector2 tileCenter = tileManager.GridCenterToWorld(gridPos);
                float distanceToTileCenter = Vector2.Distance(explosionPos, tileCenter);

                if (distanceToTileCenter <= radius)
                {
                    if (IsTileVisible(explosionCenter, gridPos, tileManager, hitList))
                    {
                        float totalDamage = Mathf.Clamp(damage / 2 / distanceToTileCenter, 0, damage);
                        tileManager.ModifyHealthAt(tileCenter, -totalDamage);
                        hitList.Add(gridPos);
                        hitTiles.Add(gridPos);  // Add to hit tiles for Gizmos
                    }
                    else
                    {
                        obstacleTiles.Add(gridPos);  // Add to obstacle tiles for Gizmos
                    }
                }
            }
        }
    }

    bool IsTileVisible(Vector3Int explosionCenter, Vector3Int targetTile, TileManager tileManager, List<Vector3Int> hitList)
    {
        Vector3Int direction = targetTile - explosionCenter;
        int steps = Mathf.Max(Mathf.Abs(direction.x), Mathf.Abs(direction.y));

        for (int i = 1; i <= steps; i++)
        {
            Vector3Int intermediateTile = explosionCenter + new Vector3Int(
                Mathf.RoundToInt((float)direction.x / steps * i),
                Mathf.RoundToInt((float)direction.y / steps * i),
                0
            );

            if (hitList.Contains(intermediateTile))
            {
                return false;
            }

            if (tileManager.ContainsTileDictKey(intermediateTile)) // If this is a solid tile
            {
                return false;  // Stop the explosion from continuing past this point
            }
        }
        return true;  // No obstacles, the tile is visible
    }

    // Draw gizmos to visualize hit and obstacle tiles
    void OnDrawGizmos()
{
    TileManager tileManager = FindObjectOfType<TileManager>();
    if (tileManager == null) return;

    // Get the size of the tile in world units
    Vector3 tileSize = tileManager.StructureGrid.cellSize;

    Gizmos.color = Color.red;
    foreach (var tilePos in hitTiles)
    {
        Vector2 tileCenter = tileManager.GridCenterToWorld(tilePos) + new Vector2(tileSize.x / 2, tileSize.y / 2);  // Center the box
        Gizmos.DrawWireCube(tileCenter, tileSize);  // Adjust size to match tile size
    }

    Gizmos.color = Color.green;
    foreach (var tilePos in obstacleTiles)
    {
        Vector2 tileCenter = tileManager.GridCenterToWorld(tilePos) + new Vector2(tileSize.x / 2, tileSize.y / 2);  // Center the box
        Gizmos.DrawWireCube(tileCenter, tileSize);  // Adjust size to match tile size
    }
}
}

    /*  The OG method
    void ApplyTileDamage(Vector2 explosionPos)
    {
        TileManager tileManager = FindObjectOfType<TileManager>();
        if (tileManager == null)
        {
            Debug.LogError("Explosion could not find TileManager in scene");
            return;
        }

        // Keep a list of hit structures' root positions, to only hit each structure once?
        List<Vector3Int> hitList = new List<Vector3Int>();

        // Loop through the grid positions in explosion radius
        Vector3Int explosionCenter = tileManager.WorldToGrid(explosionPos);
        int gridRadius = Mathf.CeilToInt(radius); // Convert radius to grid tiles
        
        for (int x = -gridRadius; x <= gridRadius; x++)
        {
            for (int y = -gridRadius; y <= gridRadius; y++)
            {
                Vector3Int gridPos = explosionCenter + new Vector3Int(x, y, 0);
                Vector2 worldPos = tileManager.GridToWorld(gridPos);
                float distance = Vector2.Distance(explosionPos, worldPos);
                

                if (distance <= radius) // Only modify tiles within the actual radius
                {
                    if (!hitList.Contains(gridPos) && tileManager.ContainsTileDictKey(gridPos))
                    {
                        float totalDamage = Mathf.Clamp(damage / 2 / distance, 0, damage);
                        tileManager.ModifyHealthAt(worldPos, -totalDamage);
                        hitList.Add(gridPos);
                        Debug.Log("Explosion applied " + totalDamage + " to a tile section");
                    }
                }
            }
        }
    }
    */

