using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class DoorOpener : MonoBehaviour, ITileObject, ITileObjectExplodable
{
    [SerializeField] List<AnimatedTile> doorsOpen;
    [SerializeField] List<AnimatedTile> doorsClosed;
    [SerializeField] List<Light2D> doorPointLights;
    [SerializeField] bool usesPower;

    public string explosionPoolName;

    public void Explode()
    {
        if (!string.IsNullOrEmpty(explosionPoolName))
        {
            GameObject explosion = PoolManager.Instance.GetFromPool(explosionPoolName);
            explosion.transform.position = transform.position;
        }
    }

    Tilemap animatedTilemap;
    Vector3Int myGridPos;
    Collider2D doorColl;
    bool isOpen;
    bool destroyed;
    float healthRatio;
    int damageIndex;

    TileManager tileManager;

    void Start()
    {
        animatedTilemap = GameObject.Find("AnimatedTilemap").GetComponent<Tilemap>();
        
        doorColl = GetComponent<Collider2D>();
        if (doorColl == null) 
            Debug.LogError("TileObject Collider is not found");

        if (animatedTilemap == null) 
            Debug.LogError("AnimatedTilemap is not found");

        myGridPos = animatedTilemap.WorldToCell(transform.position);
        //Debug.Log("DoorOpener initialized at position: " + myGridPos);

        tileManager = FindObjectOfType<TileManager>();
        TileManager.AnimatedTilemap.RefreshTile(myGridPos);
        TileManager.AnimatedTilemap.GetComponent<TilemapCollider2D>().ProcessTilemapChanges();
        TileManager.AnimatedTilemap.GetComponent<CompositeCollider2D>().GenerateGeometry();
    }

    void OnDisable()
    {
        destroyed = true;
    }

    public void UpdateHealthRatio(float healthRatio)
    {
        this.healthRatio = healthRatio;
        damageIndex = GetDamageIndex(healthRatio);

        UpdateDamage();
    }

    void UpdateOpenClose()
    {
        //Debug.Log("Damage Index: " + damageIndex);
        if (isOpen)
        {
            animatedTilemap.SetTile(myGridPos, doorsOpen[damageIndex]);

            if (tileManager.shadowCaster2DTileMap != null && tileManager.shadowCaster2DTileMap.gameObject != null)
            {
                tileManager.shadowCaster2DTileMap.Generate();
            }
        }
        else
        {
            animatedTilemap.SetTile(myGridPos, doorsClosed[damageIndex]);

            if (tileManager.shadowCaster2DTileMap != null && tileManager.shadowCaster2DTileMap.gameObject != null)
            {
                tileManager.shadowCaster2DTileMap.Generate();
            }
        }

        UpdateLights();
    }

    void UpdateDamage()
    {
        if (animatedTilemap == null)
            return;
            
        if (isOpen)
        {
            animatedTilemap.SetTile(myGridPos, doorsOpen[damageIndex]);
        }
        else
        {
            animatedTilemap.SetTile(myGridPos, doorsClosed[damageIndex]);
        }
    }

    int GetDamageIndex(float healthRatio)
    {
        int damageIndex;
        int spriteCount = doorsOpen.Count;

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

    void UpdateLights()
    {
        Color lightColor = Color.green;
        if (isOpen)
            lightColor = Color.red;

        foreach(var light in doorPointLights)
        {
            light.color = lightColor;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isOpen)
            {
                isOpen = true;
                UpdateOpenClose();
            }
            //Debug.Log("Door detected Player");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!destroyed)
        {
            if (other.CompareTag("Player"))
            {
                if (isOpen)
                {
                    isOpen = false;
                    UpdateOpenClose();
                }
                //Debug.Log("Player away from door");
            }
        }
    }
}
