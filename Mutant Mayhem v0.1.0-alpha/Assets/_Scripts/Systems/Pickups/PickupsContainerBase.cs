using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PickupsContainerBase : MonoBehaviour
{
    public List<Pickup> container = new List<Pickup>();

    public virtual void AddToContainer(Pickup pickup)
    {
        // Add to list and make child of this
        container.Add(pickup);
        pickup.transform.SetParent(transform);
        pickup.gameObject.SetActive(false);
    }
}
