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

    void Start()
    {
        // Randomize sprite
        int i = Random.Range(0, sprites.Count);
        SR.sprite = sprites[i];
        int rot = Random.Range(0, 360);
        Quaternion q = Quaternion.Euler(0, 0, rot);
        transform.rotation = q;
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
