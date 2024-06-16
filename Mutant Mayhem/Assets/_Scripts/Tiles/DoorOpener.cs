using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DoorOpener : MonoBehaviour
{
    [SerializeField] AnimatedTile doorOpen;
    [SerializeField] AnimatedTile doorClosed;

    Tilemap animatedTilemap;
    Vector3Int myGridPos;
    Collider2D doorColl;
    bool isOpen;

    TileManager tileManager;

    void Awake()
    {
        //doorContainer = GameObject.Find("DoorContainer").transform;

        //transform.parent.SetParent(doorContainer);
    }

    void Start()
    {
        doorColl = GetComponent<Collider2D>();
        if (doorColl == null) Debug.LogError("Collider2D is not found");

        animatedTilemap = GameObject.Find("AnimatedTilemap").GetComponent<Tilemap>();
        if (animatedTilemap == null) Debug.LogError("AnimatedTilemap is not found");

        myGridPos = animatedTilemap.WorldToCell(transform.position);
        Debug.Log("DoorOpener initialized at position: " + myGridPos);

        tileManager = FindObjectOfType<TileManager>();
        TileManager.AnimatedTilemap.RefreshTile(myGridPos);
        TileManager.AnimatedTilemap.GetComponent<TilemapCollider2D>().ProcessTilemapChanges();
        TileManager.AnimatedTilemap.GetComponent<CompositeCollider2D>().GenerateGeometry();
    }

    void OnDisable()
    {
        Debug.Log("Door OnDisable ran");
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
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            if (!isOpen)
            {
                animatedTilemap.SetTile(myGridPos, doorOpen);
                if (tileManager.shadowCaster2DTileMap != null && tileManager.shadowCaster2DTileMap.gameObject != null)
                {
                    tileManager.shadowCaster2DTileMap.Generate();
                }
                isOpen = true;
            }
            Debug.Log("Door detected Player");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            if (isOpen)
            {
                animatedTilemap.SetTile(myGridPos, doorClosed);
                if (tileManager.shadowCaster2DTileMap != null && tileManager.shadowCaster2DTileMap.gameObject != null)
                {
                    tileManager.shadowCaster2DTileMap.Generate();
                }
                isOpen = false;
            }
            Debug.Log("Player away from door");
        }
    }
}
