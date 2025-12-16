using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ShootingPlatform : MonoBehaviour, ITileObjectExplodable
{
    public string explosionPoolName;

    public void Explode()
    {
        if (!string.IsNullOrEmpty(explosionPoolName))
        {
            GameObject explosion = PoolManager.Instance.GetFromPool(explosionPoolName);
            Vector3Int rootPos = TileManager.Instance.WorldToGrid(transform.position);
            rootPos = TileManager.Instance.GridToRootPos(rootPos);
            explosion.transform.position = TileManager.Instance.TileCellsCenterToWorld(rootPos);
        }
    }
    
    PlayerShooter playerShooter;

    void Start()
    {
        playerShooter = FindObjectOfType<PlayerShooter>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug.Log("Player entered platform");
            playerShooter.SetElevated(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug.Log("Player exitted platform");
            playerShooter.SetElevated(false);
        }
    }
}
