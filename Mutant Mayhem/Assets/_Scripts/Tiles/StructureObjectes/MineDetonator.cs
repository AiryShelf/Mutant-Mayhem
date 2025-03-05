using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MineDetonator : MonoBehaviour
{
    [SerializeField] LayerMask playerLayerMask;
    [SerializeField] float playerSafetyRadius;
    [SerializeField] GameObject explosionPrefab;

    void OnTriggerStay2D(Collider2D collision)
    {
        // If player near, return
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, playerSafetyRadius, playerLayerMask);
        if (playerCollider != null) 
        {
            return;
        }

        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        TileManager.Instance.ModifyHealthAt(transform.position, -int.MaxValue, 0, Vector2.zero);
        Destroy(gameObject);
    }
}
