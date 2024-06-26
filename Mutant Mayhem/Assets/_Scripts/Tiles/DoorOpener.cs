using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DoorOpener : MonoBehaviour
{
    [SerializeField] List<AnimatedTile> doorsOpen;
    [SerializeField] List<AnimatedTile> doorsClosed;

    Tilemap animatedTilemap;
    Vector3Int myGridPos;
    Collider2D doorColl;
    bool isOpen;
    bool destroyed;
    float healthRatio;

    TileManager tileManager;

    void Start()
    {
        animatedTilemap = GameObject.Find("AnimatedTilemap").GetComponent<Tilemap>();
        
        doorColl = GetComponent<Collider2D>();
        if (doorColl == null) 
            Debug.LogError("Collider2D is not found");

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
        /*
        //Debug.Log("Door OnDisable ran");
        if (doorColl != null)
        {
            Destroy(doorColl);
        }

        if (animatedTilemap != null)
        {
            animatedTilemap.SetTile(myGridPos, null);
        }

        if (tileManager.shadowCaster2DTileMap != null)
        {
            tileManager.shadowCaster2DTileMap.Generate();
        }
        */
    }

    public void UpdateHealthRatio(float healthRatio)
    {
        this.healthRatio = healthRatio;

        UpdateSprite();
    }

    private void UpdateSprite()
    {
        int damageIndex = Mathf.FloorToInt(doorsOpen.Count * healthRatio);
        Debug.Log("Damage Index: " + damageIndex);
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
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            if (!isOpen)
            {
                isOpen = true;
                UpdateSprite();
            }
            //Debug.Log("Door detected Player");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!destroyed)
        {
            if (other.tag == "Player")
            {
                if (isOpen)
                {
                    isOpen = false;
                    UpdateSprite();
                }
                //Debug.Log("Player away from door");
            }
        }
    }
}
