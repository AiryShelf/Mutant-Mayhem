using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BackgroundGenerator : MonoBehaviour
{
    [SerializeField] GameObject EditorObjectToClear;
    [SerializeField] Tilemap tilemap;
    [SerializeField] List<Tile> tiles;
    [SerializeField] Vector2Int startPos;
    [SerializeField] CinemachineVirtualCamera followCamera;
    [SerializeField] int chunkSizeX;
    [SerializeField] int chunkSizeY;

    void Start()
    {
        EditorObjectToClear.SetActive(false);
    }

    void Update()
    {
        startPos = (Vector2Int)tilemap.WorldToCell(followCamera.transform.position);
        GenerateWorld(startPos);
    }

    void GenerateWorld(Vector2Int startPos)
    {
        for (int x = startPos.x - chunkSizeX; x < startPos.x + chunkSizeX; x++)
        {
            for (int y = startPos.y - chunkSizeY; y < startPos.y + chunkSizeY; y++)
            {
                TileBase tileBase = tilemap.GetTile(new Vector3Int (x, y, 0));
                Tile tile = tileBase as Tile;
                if (tile == null)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tiles[0]);
                }
                
            }
        }
    }
}
