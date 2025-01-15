using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PickupData
    {
        public int credits;

        public PickupData(int credits)
        {
            this.credits = credits;
        }
    }

public class Pickup : MonoBehaviour
{
    public PickupData pickupData;
    [SerializeField] List<Sprite> sprites = new List<Sprite>();
    [SerializeField] SpriteRenderer SR;
    [SerializeField] Collider2D myCollider;

    public bool unloading;
    public TileManager tileManager;

    void OnEnable()
    {
        // Randomize sprite
        int i = Random.Range(0, sprites.Count);
        SR.sprite = sprites[i];
        int rot = Random.Range(0, 360);
        Quaternion q = Quaternion.Euler(0, 0, rot);
        transform.rotation = q;
        //RepositionIfNecessary();
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    public void RepositionIfNecessary()
    {
        StartCoroutine(CheckIfClear());
    }

    IEnumerator CheckIfClear()
    {
        Vector3Int gridPos = Vector3Int.zero;
        Vector2 myPos;
        // Try repositioning if the initial spot is over a structure
        while (true)
        {
            myPos = transform.position;
            gridPos = tileManager.WorldToGrid(myPos);
            if (!tileManager.IsTileBlueprint(myPos) &&
                !tileManager.CheckGridIsClear(gridPos, LayerMask.GetMask("Structures"), true))
            {
                Vector2 dir = myPos - tileManager.GridCenterToWorld(gridPos);
                Debug.LogWarning("Pickup found over a structure, repositioning...");
                StartCoroutine(RepositionUntilFree(dir));
            }

            yield return new WaitForSeconds(3);
        }
    }

    IEnumerator RepositionUntilFree(Vector2 dir)
    {
        Vector3Int gridPos;
        bool free = false;
        while (!free)
        {
            transform.position += (Vector3)dir.normalized * 1;
            gridPos = tileManager.WorldToGrid(transform.position);
            if (tileManager.CheckGridIsClear(gridPos, LayerMask.GetMask("Structures"), true))
                free = true;
                
            yield return new WaitForFixedUpdate();
        }
    }

    void Reposition()
    {
        // Generate a new random position for the pickup
        Vector2 offset = Random.insideUnitCircle * 0.5f;
        transform.position = new Vector3(transform.position.x + offset.x, transform.position.y + offset.y, transform.position.z);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // Set collider to trigger to go over walls
        PickupsReceiverTrigger trigger = col.GetComponent<PickupsReceiverTrigger>();
        if (trigger != null)
        {
            myCollider.isTrigger = true;
            return;
        }

        PickupsContainerBase container = col.GetComponent<PickupsContainerBase>();
        if (container == null)
        {
            Debug.Log("No Pickups Container or Receiver detected by pickup");
            return;
        }
        
        // Do not add to player container when unloading
        if (unloading)
        {
            PickupsContainerPlayer player = col.GetComponent<PickupsContainerPlayer>();
            if (player != null)
                return;
        }
        container.AddToContainer(this);
    }

    void OnTriggerExit2D(Collider2D col)
    {
        PickupsReceiverTrigger trigger = col.GetComponent<PickupsReceiverTrigger>();
        if (trigger != null)
        {
            unloading = false;
            myCollider.isTrigger = false;
        }
    }
}
