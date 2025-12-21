using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MineDetonator : MonoBehaviour
{
    [SerializeField] LayerMask triggerLayers;
    [SerializeField] LayerMask playerLayerMask;
    [SerializeField] float playerSafetyRadius;
    [SerializeField] string explosionPoolName;

    void OnTriggerStay2D(Collider2D collider)
    {
        // If player near, return
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, playerSafetyRadius, playerLayerMask);
        if (playerCollider != null) 
        {
            return;
        }

        // If colliding with valid layer, detonate
        if (((1 << collider.gameObject.layer) & triggerLayers) != 0)
        {
            GameObject obj = PoolManager.Instance.GetFromPool(explosionPoolName);
            obj.transform.position = transform.position;
                
            TileManager.Instance.ModifyHealthAt(transform.position, -int.MaxValue, 0, Vector2.zero);
            Destroy(gameObject);
        }
    }
}
