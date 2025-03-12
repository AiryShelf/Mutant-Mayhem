using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class DoorOpener : TileObject
{
    [SerializeField] List<AnimatedTile> doorsOpen;
    [SerializeField] List<AnimatedTile> doorsClosed;
    [SerializeField] List<Light2D> doorPointLights;

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

    public override void UpdateHealthRatio(float healthRatio)
    {
        this.healthRatio = healthRatio;

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
        damageIndex = Mathf.FloorToInt(doorsOpen.Count * healthRatio);

        if (isOpen)
        {
            animatedTilemap.SetTile(myGridPos, doorsOpen[damageIndex]);
        }
        else
        {
            animatedTilemap.SetTile(myGridPos, doorsClosed[damageIndex]);
        }
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
